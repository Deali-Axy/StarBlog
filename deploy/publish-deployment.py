#!/usr/bin/env python3
"""Upload a small deployment recipe using immutable images from ship's manifest."""
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REGISTRY = "ccr.ccs.tencentyun.com/deali/starblog"


def image_pins(manifest, version):
    if manifest["version"] != version:
        raise ValueError("Release manifest version mismatch")
    pins = {}
    for artifact in manifest["artifacts"]:
        profile = artifact.get("profile")
        if artifact["type"] != "container-image" or profile not in ("api", "web", "gateway"):
            continue
        if artifact.get("ref") != f"{REGISTRY}:{version}-{profile}":
            continue
        digest = artifact.get("digest", "")
        if not re.fullmatch(r"sha256:[a-f0-9]{64}", digest):
            raise ValueError("Image is not pinned by a registry digest")
        pin = REGISTRY + "@" + digest
        if profile in pins and pins[profile] != pin:
            raise ValueError("Conflicting image digests")
        pins[profile] = pin
    if set(pins) != {"api", "web", "gateway"}:
        raise ValueError("A complete three-image release is required")
    return {profile.upper() + "_IMAGE": pin for profile, pin in pins.items()}


def main():
    version = sys.argv[1]
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
        raise ValueError("Invalid release version")
    manifest_file = ROOT / ".ship/releases" / (version + ".json")
    manifest = json.loads(manifest_file.read_text())
    pins = image_pins(manifest, version)
    output = ROOT / ".ship/image-deploy" / version
    output.mkdir(parents=True, exist_ok=True)
    recipe = {"version": version, "images": pins, "source": manifest["source"]}
    (output / "release.json").write_text(json.dumps(recipe, indent=2))
    destination = f"/home/deali/projects/starblog-v2/image-releases/{version}"
    subprocess.run(["ssh", "deali.cn", f"umask 077; mkdir -p {destination}"], check=True)
    subprocess.run(["scp", str(output / "release.json"), str(ROOT / "deploy/compose.images.yaml"),
                    str(ROOT / "deploy/deploy-images.py"), str(ROOT / "deploy/gateway.conf.template"), f"deali.cn:{destination}/"], check=True)
    print("Uploaded image deployment recipe; no application archive or private data transferred.")


if __name__ == "__main__":
    main()
