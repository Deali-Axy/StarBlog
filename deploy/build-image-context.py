#!/usr/bin/env python3
"""Prepare runtime-only build contexts for ship's three application images."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
version = sys.argv[1]
if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
    raise SystemExit("Invalid release version")
release = ROOT / "artifacts/image-context"
if release.exists():
    shutil.rmtree(release)
release.mkdir(parents=True)


def run(args, cwd=ROOT, env=None):
    subprocess.run(args, cwd=cwd, env=env, check=True)


run(["dotnet", "publish", "apps/api/src/StarBlog.Api/StarBlog.Api.csproj", "-c", "Release",
     "--no-restore", "--nologo", "-o", str(release / "api")])
(release / "api/wwwroot/media").mkdir(parents=True, exist_ok=True)
for name in ("appsettings.Development.json", "appsettings-email.json", "appsettings-monitoring.json", "words.json"):
    (release / "api" / name).unlink(missing_ok=True)

settings_path = release / "api/appsettings.json"
settings = json.loads(settings_path.read_text())
settings["Auth"]["Jwt"]["Key"] = ""
settings_path.write_text(json.dumps(settings, ensure_ascii=False, indent=2))
for directory in ("data", "wwwroot/media", "wwwroot/is-cache"):
    (release / "api" / directory).mkdir(parents=True, exist_ok=True)
for filename in ("appsettings-email.json", "words.json"):
    (release / "api" / filename).touch()

env = os.environ.copy()
env.update({"INTERNAL_API_BASE_URL": "http://api:8080", "NEXT_PUBLIC_API_BASE_URL": "",
            "NEXT_PUBLIC_SITE_URL": "https://blog.deali.cn", "NEXT_TELEMETRY_DISABLED": "1"})
run(["node", "node_modules/next/dist/bin/next", "build"], ROOT / "apps/web", env)
standalone = ROOT / "apps/web/.next/standalone"
if not (standalone / "server.js").is_file():
    raise SystemExit("Expected standalone/server.js")
shutil.copytree(standalone, release / "web", symlinks=True)
shutil.copytree(ROOT / "apps/web/.next/static", release / "web/.next/static", dirs_exist_ok=True)
if (ROOT / "apps/web/public").exists():
    shutil.copytree(ROOT / "apps/web/public", release / "web/public", dirs_exist_ok=True)
for path in (release / "web").rglob(".env*"):
    if path.is_file() or path.is_symlink():
        path.unlink()

admin_env = os.environ.copy()
admin_env.update({"VITE_BASE_PATH": "/admin/", "VITE_API_URL": ""})
run(["npm", "run", "build"], ROOT / "apps/admin", admin_env)
shutil.copytree(ROOT / "apps/admin/dist", release / "admin")
metadata = {
    "version": version,
    "commit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
    "working_tree_diff_sha256": hashlib.sha256(subprocess.check_output(["git", "diff", "HEAD"], cwd=ROOT)).hexdigest(),
}
(release / "release.json").write_text(json.dumps(metadata, indent=2))
for path in release.rglob("*"):
    if path.is_file() and (path.name.endswith((".db", ".db-wal", ".db-shm")) or path.name.startswith(".env")):
        raise SystemExit(f"Private data unexpectedly present in runtime archive: {path.relative_to(release)}")
print(f"Image build contexts ready: {version}", flush=True)
