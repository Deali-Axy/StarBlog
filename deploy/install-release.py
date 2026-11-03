#!/usr/bin/env python3
"""One-time v1 -> v2 migration. Replace with registry images after rollout (docs/todo.md)."""
from contextlib import closing
from datetime import datetime, timezone
import hashlib
import importlib.util
import ipaddress
import json
import os
from pathlib import Path
import re
import secrets
import shutil
import sqlite3
import subprocess
import sys
import tarfile
import time
import urllib.error
import urllib.parse
import urllib.request

BASE = Path("/home/deali/projects/starblog-v2")
LEGACY = Path("/home/deali/projects/starblog")
SWAG_CONFIG = "/config/nginx/proxy-confs/blog.subdomain.conf"
SUBNET = ipaddress.ip_network("172.30.84.0/24")


def command(args, *, capture=False, input=None, check=True):
    return subprocess.run(args, check=check, input=input, text=True,
                          stdout=subprocess.PIPE if capture else None,
                          stderr=subprocess.PIPE if capture else None)


def compose(*args, **kwargs):
    return command(["docker", "compose", "--project-directory", str(BASE), "--env-file",
                    str(BASE / ".env"), "-f", str(BASE / "compose.yaml"), *args], **kwargs)


def private_write(path, content):
    path.write_text(content)
    path.chmod(0o600)


def extract_release(archive, destination):
    """Allow traced Node symlinks only when their targets remain inside the release."""
    root = destination.resolve()
    with tarfile.open(archive) as tar:
        for member in tar.getmembers():
            path = root / member.name
            if Path(member.name).is_absolute() or not path.resolve().is_relative_to(root):
                raise ValueError(f"Unsafe archive path: {member.name}")
            if member.issym():
                if not (path.parent / member.linkname).resolve().is_relative_to(root):
                    raise ValueError(f"Unsafe symlink: {member.name}")
            elif member.islnk():
                if not (root / member.linkname).resolve().is_relative_to(root):
                    raise ValueError(f"Unsafe hardlink: {member.name}")
            elif not (member.isfile() or member.isdir()):
                raise ValueError(f"Unsupported archive member: {member.name}")
        tar.extractall(root)


def read_inputs():
    with tarfile.open(BASE / "migration/inputs.tar.gz") as tar:
        members = tar.getmembers()
        if {m.name for m in members} != {"translations.db", "identity.json"} or len(members) != 2:
            raise ValueError("Unexpected private migration inputs")
        for member in members:
            if not member.isfile():
                raise ValueError("Migration inputs must be regular files")
            path = BASE / "migration" / member.name
            path.write_bytes(tar.extractfile(member).read())
            path.chmod(0o600)
    users = json.loads((BASE / "migration/identity.json").read_text())
    if not users or any(len(row) != 3 or not all(isinstance(v, str) and v for v in row) for row in users):
        raise ValueError("Missing or invalid preserved administrator")
    return users


def pin_image(image):
    command(["docker", "pull", image])
    info = json.loads(command(["docker", "image", "inspect", image], capture=True).stdout)[0]
    digests = info.get("RepoDigests") or []
    repository = image.rsplit(":", 1)[0]
    return next((d for d in digests if d.split("@")[0] == repository), digests[0] if digests else info["Id"])


def save_swag(content):
    command(["docker", "exec", "-i", "swag", "sh", "-c", f"cat > {SWAG_CONFIG}"], input=content)
    command(["docker", "exec", "swag", "nginx", "-t"], capture=True)
    command(["docker", "exec", "swag", "nginx", "-s", "reload"])


def database_snapshot(source, target):
    with closing(sqlite3.connect(source.as_uri() + "?mode=ro", uri=True, timeout=30)) as db, \
            closing(sqlite3.connect(target)) as backup:
        db.backup(backup)
        if backup.execute("PRAGMA quick_check").fetchall() != [("ok",)]:
            raise ValueError("SQLite snapshot failed integrity check")
    target.chmod(0o600)


def wait_until(action, seconds=120):
    deadline = time.monotonic() + seconds
    last = None
    while time.monotonic() < deadline:
        try:
            return action()
        except (OSError, ValueError, sqlite3.Error, AssertionError) as error:
            last = error
            time.sleep(1)
    raise RuntimeError(f"Readiness timed out: {last}")


def read_url(path, *, method="GET"):
    request = urllib.request.Request("http://127.0.0.1:15080" + path,
                                     headers={"Host": "blog.deali.cn"}, method=method)
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    with opener.open(request, timeout=45) as response:
        if response.status != 200:
            raise ValueError(f"Unexpected HTTP status for {path}: {response.status}")
        return response.read()


def verify_site():
    health = json.loads(wait_until(lambda: read_url("/health/ready")))
    if health.get("status") != "Healthy":
        raise ValueError("API is not healthy")
    initialization = json.loads(read_url("/api/v1/site/initialization"))
    if initialization.get("isInitialized") is not True or initialization.get("host") != "https://blog.deali.cn":
        raise ValueError("Administrator initialization or production host is missing")
    for path in ("/", "/blog", "/photography", "/rss", "/admin/", "/admin/posts", "/feed", "/sitemap.xml"):
        read_url(path)
    if b"/admin/assets/" not in read_url("/admin/"):
        raise ValueError("Administration assets have the wrong base path")
    with closing(sqlite3.connect(BASE / "data/app.db")) as db:
        translated = db.execute("SELECT t.post_id,t.title FROM post_translation t JOIN post p ON p.id=t.post_id WHERE p.is_publish=1 LIMIT 1").fetchone()
        draft = db.execute("SELECT id FROM post WHERE is_publish=0 LIMIT 1").fetchone()
        photos = db.execute("SELECT id,file_path FROM photo").fetchall()
        published = db.execute("SELECT COUNT(*) FROM post WHERE is_publish=1").fetchone()[0]
    if json.loads(read_url("/api/v1/posts?pageSize=1"))["totalCount"] != published:
        raise ValueError("Published post count differs from database")
    if translated:
        data = json.loads(read_url(f"/api/v1/posts/{translated[0]}?lang=en"))
        if data["title"] != translated[1]:
            raise ValueError("English translation mismatch")
        read_url(f"/blog/{translated[0]}?lang=en")
    if draft:
        try:
            read_url("/api/v1/posts/" + draft[0])
        except urllib.error.HTTPError as error:
            if error.code != 404:
                raise
        else:
            raise ValueError("Draft exposed to anonymous visitors")
    for photo_id, file_path in photos:
        read_url("/media/photography/" + urllib.parse.quote(file_path.replace("\\", "/")), method="HEAD")
    if photos:
        read_url(f"/api/v1/photos/{photos[0][0]}/thumbnail?width=150")
    return {"health": health["status"], "published_posts": published, "photo_originals_checked": len(photos),
            "english_translation_checked": bool(translated), "anonymous_draft_protection": bool(draft)}


def owned_tree(path, uid, gid):
    for child in [path, *path.rglob("*")]:
        if not child.is_symlink():
            os.chown(child, uid, gid)


def main():
    version = sys.argv[1]
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
        raise ValueError("Invalid release version")
    os.umask(0o077)
    if (BASE / "active-release.json").exists():
        active = json.loads((BASE / "active-release.json").read_text())
        if active["version"] != version:
            raise ValueError("First-migration installer cannot deploy another release; use the image workflow in docs/todo.md")
        print(json.dumps(verify_site()), flush=True)
        return
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    archive = BASE / "incoming" / f"starblog-{version}.tar.gz"
    release = BASE / "releases" / version
    backup_dir = BASE / "backups" / stamp
    for directory in (release, backup_dir, BASE / "data", BASE / "media", BASE / "config", BASE / "cache/images", BASE / "migration"):
        directory.mkdir(parents=True, exist_ok=True)
    extract_release(archive, release)
    if json.loads((release / "release.json").read_text())["version"] != version:
        raise ValueError("Release archive version mismatch")
    # Nested bind mounts need existing targets when their parent /app mount is read-only.
    for directory in (release / "api/data", release / "api/wwwroot/media", release / "api/wwwroot/is-cache"):
        directory.mkdir(parents=True, exist_ok=True, mode=0o755)
    for filename in ("appsettings-email.json", "words.json"):
        (release / "api" / filename).touch(exist_ok=True)
    users = read_inputs()
    orphan = command(["docker", "inspect", "starblog-migration-v2"], capture=True, check=False)
    if orphan.returncode == 0:
        info = json.loads(orphan.stdout)[0]
        if info["State"]["Running"] or info["Config"]["Labels"].get("com.docker.compose.project") != "starblog-v2":
            raise ValueError("Unexpected existing migration container")
        command(["docker", "rm", "starblog-migration-v2"])
    networks = json.loads(command(["docker", "network", "inspect", *command(["docker", "network", "ls", "-q"], capture=True).stdout.split()], capture=True).stdout)
    for network in networks:
        for config in network["IPAM"].get("Config") or []:
            candidate = ipaddress.ip_network(config["Subnet"]) if config.get("Subnet") else None
            if candidate and candidate.version == SUBNET.version and SUBNET.overlaps(candidate) and network["Name"] != "starblog-v2_backend":
                raise ValueError(f"Network collision: {network['Name']}")
    swag = json.loads(command(["docker", "inspect", "swag"], capture=True).stdout)[0]
    swag_ip = swag["NetworkSettings"]["Networks"]["swag"]["IPAddress"]
    ipaddress.ip_address(swag_ip)
    original_config = command(["docker", "exec", "swag", "cat", SWAG_CONFIG], capture=True).stdout
    if "set $upstream_app starblog;" not in original_config:
        raise ValueError("Unexpected original SWAG configuration; refusing automatic cutover")
    private_write(backup_dir / "blog.subdomain.conf", original_config)
    private_write(backup_dir / "legacy-container.json", command(["docker", "inspect", "starblog"], capture=True).stdout)
    print("Preparing runtime images before maintenance", flush=True)
    images = {"API_IMAGE": pin_image("mcr.microsoft.com/dotnet/aspnet:10.0"),
              "WEB_IMAGE": pin_image("node:24-bookworm-slim"),
              "GATEWAY_IMAGE": pin_image("nginx:stable-alpine")}
    # Existing secrets survive a retry; credentials are never printed.
    old_env = {}
    if (BASE / ".env").exists():
        old_env = dict(line.split("=", 1) for line in (BASE / ".env").read_text().splitlines() if "=" in line)
    env = {**images, "JWT_KEY": old_env.get("JWT_KEY", secrets.token_urlsafe(48)), "RELEASE_DIR": str(release)}
    private_write(BASE / ".env", "".join(f"{key}={value}\n" for key, value in env.items()))
    shutil.copy2(release / "deploy/compose.server.yaml", BASE / "compose.yaml")
    (BASE / "gateway.conf").write_text((release / "deploy/gateway.conf.template").read_text().replace("__SWAG_IP__", swag_ip))
    # Container runtime files must be readable by their non-root users.
    command(["docker", "run", "--rm", "--user", "0", "-v", f"{BASE}:/deployment", "-v", f"{release}/web:/web", "--entrypoint", "sh", images["GATEWAY_IMAGE"], "-c",
             "chmod 755 /deployment/gateway.conf; chown -R 1654:1654 /deployment/data /deployment/media /deployment/cache; chmod 755 /deployment/data /deployment/media /deployment/cache /deployment/cache/images; chown -R 1000:1000 /web"])
    for filename in ("appsettings-email.json", "words.json"):
        destination = BASE / "config" / filename
        if not destination.exists():
            source = LEGACY / filename
            destination.write_bytes(source.read_bytes() if source.exists() else (b"{}" if filename.endswith("email.json") else b"[]"))
        destination.chmod(0o644)
    # Keep private files protected even though the containing directory is not mounted.
    compose("config", "--quiet")
    stopped = False
    try:
        print("Entering maintenance; preserving the old container and all its data", flush=True)
        maintenance = '''server { listen 443 ssl; listen [::]:443 ssl; server_name blog.*;
include /config/nginx/ssl.conf; default_type text/html; charset utf-8;
add_header Retry-After 300 always; return 503 '<h1>StarBlog 正在更新</h1><p>请稍后再试。</p>'; }\n'''
        save_swag(maintenance)
        command(["docker", "stop", "starblog"])
        stopped = True
        target = BASE / "data/app.db"
        completed = BASE / "migration/completed.json"
        if completed.exists():
            raise ValueError("A previous migration exists without an active release; preserve and reconcile it before retrying")
        if target.exists():
            with closing(sqlite3.connect(target)) as db:
                exists = db.execute("SELECT COUNT(*) FROM sqlite_master WHERE name='post'").fetchone()[0]
                if exists and db.execute("SELECT COUNT(*) FROM post").fetchone()[0]:
                    raise ValueError("Target already contains content; refusing overwrite")
        print("Creating a fresh EF schema in an isolated temporary API container", flush=True)
        compose("run", "--rm", "-d", "--no-deps", "--name", "starblog-migration-v2", "api")

        def schema_ready():
            if not target.exists():
                raise ValueError("Waiting for database")
            with closing(sqlite3.connect(target.resolve().as_uri() + "?mode=ro", uri=True)) as db:
                if db.execute("SELECT COUNT(*) FROM category").fetchone()[0] != 1:
                    raise ValueError("Waiting for default category")
        try:
            wait_until(schema_ready)
        finally:
            command(["docker", "stop", "starblog-migration-v2"], check=False, capture=True)
        # The SQLite file belongs to the API UID; perform migration using a short root container
        # only for ownership adjustment, while the migration itself runs as the SSH user.
        command(["docker", "run", "--rm", "--user", "0", "-v", f"{BASE}/data:/data", "--entrypoint", "sh", images["GATEWAY_IMAGE"], "-c", f"chown -R {os.getuid()}:{os.getgid()} /data"])
        source_snapshot = backup_dir / "legacy-final.db"
        database_snapshot(LEGACY / "app.db", source_snapshot)
        spec = importlib.util.spec_from_file_location("legacy_import", release / "deploy/Import-LegacyBlog.py")
        migration = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(migration)
        options = {"source_media": LEGACY / "wwwroot/media", "target_media": BASE / "media",
                   "translations_from": BASE / "migration/translations.db"}
        print("Checking final content, paid translations and server media", flush=True)
        preview = migration.migrate(source_snapshot, target, backup_dir, True, **options)
        # Source media remains untouched; destination ownership is adjusted after import.
        command(["docker", "run", "--rm", "--user", "0", "-v", f"{BASE}/media:/media", "--entrypoint", "sh", images["GATEWAY_IMAGE"], "-c", f"chown -R {os.getuid()}:{os.getgid()} /media"])
        report = migration.migrate(source_snapshot, target, backup_dir, **options)
        with closing(sqlite3.connect(target)) as db, closing(sqlite3.connect((BASE / "migration/translations.db").as_uri() + "?mode=ro", uri=True)) as translated:
            if db.execute("SELECT * FROM post_translation ORDER BY id").fetchall() != translated.execute("SELECT * FROM post_translation ORDER BY id").fetchall():
                raise ValueError("Preserved translations differ")
            with db:
                db.executemany('INSERT INTO user (id,name,password) VALUES (?,?,?)', users)
                for key, value in (("host", "https://blog.deali.cn"), ("default_render", "frontend"), ("is_init", "true")):
                    db.execute('INSERT INTO config (key,value,description) VALUES (?,?,NULL) ON CONFLICT(key) DO UPDATE SET value=excluded.value', (key, value))
            if db.execute("PRAGMA quick_check").fetchall() != [("ok",)] or db.execute("PRAGMA foreign_key_check").fetchall():
                raise ValueError("Final database integrity check failed")
            if db.execute("SELECT COUNT(*) FROM visit_record").fetchone()[0] != 0:
                raise ValueError("Unexpected legacy access records imported")
        pending, existing = migration.media_plan(LEGACY / "wwwroot/media", BASE / "media")
        if pending:
            raise ValueError("Some legacy media were not copied")
        report["verified_media_files"] = existing
        report["release"] = version
        private_write(backup_dir / "migration-report.json", json.dumps(report, ensure_ascii=False, indent=2))
        private_write(completed, json.dumps({"version": version, "backup": str(backup_dir)}))
        command(["docker", "run", "--rm", "--user", "0", "-v", f"{BASE}:/deployment", "--entrypoint", "sh", images["GATEWAY_IMAGE"], "-c", "chown -R 1654:1654 /deployment/data /deployment/media /deployment/cache; chmod 755 /deployment/data /deployment/media /deployment/cache /deployment/cache/images"])
        print("Starting new services and verifying before cutover", flush=True)
        compose("up", "-d", "--wait", "--wait-timeout", "180")
        validation = verify_site()
        new_config = original_config.replace("set $upstream_app starblog;", "set $upstream_app starblog-gateway-v2;").replace("set $upstream_port 80;", "set $upstream_port 8080;")
        save_swag(new_config)
        private_write(BASE / "active-release.json", json.dumps({"version": version, "backup": str(backup_dir),
                      "archive_sha256": hashlib.sha256(archive.read_bytes()).hexdigest(), "validation": validation}, indent=2))
        print(json.dumps({"deployed": version, "validation": validation, "migration": report}, ensure_ascii=False), flush=True)
    except BaseException:
        print("Deployment failed; restoring old routing and preserving new data for investigation", flush=True)
        compose("down", check=False)
        if stopped:
            command(["docker", "start", "starblog"], check=False)
        save_swag(original_config)
        raise


if __name__ == "__main__":
    main()
