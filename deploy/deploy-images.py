#!/usr/bin/env python3
"""Update the existing site from registry images, retaining its persistent data."""
from datetime import datetime, timezone
import json
import ipaddress
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
from urllib.request import Request, urlopen

BASE = Path("/home/deali/projects/starblog-v2")


def run(args):
    subprocess.run(args, check=True)


def compose(*args):
    run(["docker", "compose", "--env-file", str(BASE / ".env"), "-f", str(BASE / "compose.yaml"), *args])


def read_url(route):
    with urlopen(Request("http://127.0.0.1:15080" + route, headers={"Host": "blog.deali.cn"}), timeout=20) as response:
        return response.read()


def featured_state():
    home = json.loads(read_url("/api/v1/site/home"))
    return {"categories": sorted(c["categoryId"] for c in home["featuredCategories"]),
            "photos": sorted(p["id"] for p in home["featuredPhotos"])}


def verify(expected):
    health = json.loads(read_url("/health/ready"))
    if health["status"] != "Healthy":
        raise ValueError("API is not ready")
    if featured_state() != expected:
        raise ValueError("Featured content differs from the pre-deployment state")
    html = read_url("/").decode()
    for marker in ('data-home-design="journal"', 'data-home-texture="dots"', 'data-home-border="dashed"'):
        if marker not in html:
            raise ValueError("Default home design mismatch: " + marker)
    for section, variant in (("featuredPosts", "notes"), ("photos", "prints"), ("links", "cards")):
        if not re.search(r'data-home-section="' + section + r'"[^>]*data-home-variant="' + variant + r'"', html):
            raise ValueError("Default home section mismatch: " + section)
    for pid in expected["photos"]:
        if pid not in html:
            raise ValueError("Featured photo missing from homepage")
    if "<html" not in read_url("/admin/").decode().lower():
        raise ValueError("Admin entry failed")


def private_write(filename, data):
    filename.write_text(data)
    filename.chmod(0o600)


def main():
    os.umask(0o077)
    version = sys.argv[1]
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
        raise ValueError("Invalid release version")
    release = BASE / "image-releases" / version
    recipe = json.loads((release / "release.json").read_text())
    if recipe["version"] != version or set(recipe["images"]) != {"API_IMAGE", "WEB_IMAGE", "GATEWAY_IMAGE"}:
        raise ValueError("Incomplete image recipe")
    for pin in recipe["images"].values():
        if not re.fullmatch(r"ccr\.ccs\.tencentyun\.com/deali/starblog@sha256:[a-f0-9]{64}", pin):
            raise ValueError("Unexpected image reference")
        run(["docker", "pull", pin])
    expected = featured_state()
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    backup = BASE / "backups" / ("images-" + stamp)
    backup.mkdir(mode=0o700)
    for name in (".env", "compose.yaml", "active-release.json", "gateway.conf"):
        shutil.copy2(BASE / name, backup / name)
        (backup / name).chmod(0o600)
    private_write(backup / "featured-before.json", json.dumps(expected))
    env = dict(line.split("=", 1) for line in (BASE / ".env").read_text().splitlines() if line and not line.startswith("#") and "=" in line)
    env.update(recipe["images"])
    env.pop("RELEASE_DIR", None)
    # The API is stopped only after every new image is already local.
    # Preserve the complete SQLite directory, including any remaining WAL files.
    try:
        run(["docker", "stop", "starblog-api-v2"])
        run(["docker", "cp", "starblog-api-v2:/app/data", str(backup / "data")])
        private_write(BASE / ".env", "".join(f"{key}={value}\n" for key, value in env.items()))
        shutil.copy2(release / "compose.images.yaml", BASE / "compose.yaml")
        swag = json.loads(subprocess.check_output(["docker", "inspect", "swag"], text=True))[0]
        swag_ip = swag["NetworkSettings"]["Networks"]["swag"]["IPAddress"]
        ipaddress.ip_address(swag_ip)
        (BASE / "gateway.conf").write_text((release / "gateway.conf.template").read_text().replace("__SWAG_IP__", swag_ip))
        (BASE / "gateway.conf").chmod(0o644)
        compose("config", "--quiet")
        compose("up", "-d", "--force-recreate", "--wait", "--wait-timeout", "180")
        for attempt in range(12):
            try:
                verify(expected)
                break
            except Exception:
                if attempt == 11:
                    raise
                time.sleep(3)
        record = {**recipe, "backup": str(backup), "deployed_at": stamp, "featured": expected}
        private_write(BASE / "active-release.json", json.dumps(record, indent=2))
        private_write(release / "deployed.json", json.dumps(record, indent=2))
        print(json.dumps({"deployed": version, "backup": str(backup), "featured_categories": len(expected["categories"]), "featured_photos": len(expected["photos"])}), flush=True)
    except BaseException:
        print("Image update failed; restoring the previous containers with the current persistent database.", flush=True)
        for name in (".env", "compose.yaml", "active-release.json", "gateway.conf"):
            shutil.copy2(backup / name, BASE / name)
        compose("up", "-d", "--force-recreate", "--wait", "--wait-timeout", "180")
        raise


if __name__ == "__main__":
    main()
