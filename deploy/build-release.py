#!/usr/bin/env python3
"""Build a runtime-only release archive for ship's command/SCP driver."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tarfile

ROOT = Path(__file__).resolve().parents[1]
version = sys.argv[1]
if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
    raise SystemExit("Invalid release version")
for name in ("compose.server.yaml", "gateway.conf.template", "install-release.py"):
    if not (ROOT / "deploy" / name).is_file():
        raise SystemExit(f"Missing deployment input: {name}")
release = ROOT / "artifacts/releases" / version
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
deployment = release / "deploy"
deployment.mkdir()
for name in ("compose.server.yaml", "gateway.conf.template", "install-release.py"):
    shutil.copy2(ROOT / "deploy" / name, deployment / name)
shutil.copy2(ROOT / "scripts/Import-LegacyBlog.py", deployment / "Import-LegacyBlog.py")
metadata = {
    "version": version,
    "commit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
    "working_tree_diff_sha256": hashlib.sha256(subprocess.check_output(["git", "diff", "HEAD"], cwd=ROOT)).hexdigest(),
}
(release / "release.json").write_text(json.dumps(metadata, indent=2))
for path in release.rglob("*"):
    if path.is_file() and (path.name.endswith((".db", ".db-wal", ".db-shm")) or path.name.startswith(".env")):
        raise SystemExit(f"Private data unexpectedly present in runtime archive: {path.relative_to(release)}")
archive = ROOT / "artifacts" / f"starblog-{version}.tar.gz"
with tarfile.open(archive, "w:gz") as output:
    for path in sorted(release.iterdir()):
        output.add(path, arcname=path.name)
print(f"Release ready: {archive.name}, {archive.stat().st_size:,} bytes", flush=True)
