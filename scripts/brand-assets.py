#!/usr/bin/env python3
"""Build the six saved logo suites from editable SVG masters; never modify apps."""

import argparse
import copy
import hashlib
import html
import json
from pathlib import Path
import re
import shutil
import struct
import subprocess
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
BRAND = ROOT / "docs/brand"
NS = "http://www.w3.org/2000/svg"
ET.register_namespace("", NS)
SVG_NAMES = (
    "mark", "mark-mono", "mark-reversed", "mark-current", "wordmark",
    "logo", "logo-mono", "logo-reversed", "logo-stacked", "app-icon", "preview",
)
PNG_SIZES = {"mark": 1024, "logo": 2000, "app-icon": 512, "preview": 1600}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def receipt(folder):
    return {
        "generator": digest(Path(__file__)),
        "inputs": {name: digest(folder / name) for name in (
            "brand.json", "source/mark.svg", "source/wordmark.svg",
        )},
        "outputs": {
            path.name: digest(path) for path in sorted((folder / "exports").iterdir())
            if path.is_file() and path.name != "build.json"
        },
    }


def master(folder, name, meta, mode="primary"):
    source = ET.parse(folder / meta["source"][name]).getroot()
    _, _, width, height = map(float, source.attrib["viewBox"].split())
    group = ET.Element(f"{{{NS}}}g")
    for node in source:
        if node.tag != f"{{{NS}}}path":
            raise ValueError(f"{folder.name}/{name}: masters must contain paths only")
        node = copy.deepcopy(node)
        role = node.attrib.pop("data-role")
        if mode == "current":
            color = "currentColor"
        elif mode == "mono":
            color = meta["palette"]["ink"]
        elif mode in ("reversed", "icon"):
            color = meta["palette"]["paper"]
            if mode == "icon" and role == "accent" and meta["id"] in ("offset", "horizon"):
                color = meta["palette"]["accent"]
        else:
            color = meta["palette"][role]
        node.set("fill", color)
        group.append(node)
    return ET.tostring(group, encoding="unicode"), width, height


def fit(asset, x, y, width, height):
    content, source_w, source_h = asset
    scale = min(width / source_w, height / source_h)
    dx = x + (width - source_w * scale) / 2
    dy = y + (height - source_h * scale) / 2
    return f'<g transform="translate({dx:.5f} {dy:.5f}) scale({scale:.7f})">{content}</g>'


def svg(width, height, body, title):
    return (
        f'<svg xmlns="{NS}" width="{width}" height="{height}" '
        f'viewBox="0 0 {width} {height}" role="img" aria-label="{html.escape(title)}">\n'
        f"{body}\n</svg>\n"
    )


def label(text, x, y, size=14, color="#4F565A"):
    return (f'<text x="{x}" y="{y}" font-family="DejaVu Sans, sans-serif" '
            f'font-size="{size}" fill="{color}">{html.escape(text)}</text>')


def lockup(folder, meta, mode="primary"):
    word = master(folder, "wordmark", meta, mode)
    if meta["wordmarkOnly"]:
        width = round(word[1] / word[2] * 160 + 64)
        return svg(width, 240, fit(word, 32, 40, width - 64, 160), f'StarBlog {meta["nameEn"]}')
    word_w = round(word[1] / word[2] * 112)
    width = 32 + 176 + 36 + word_w + 32
    body = fit(master(folder, "mark", meta, mode), 32, 32, 176, 176)
    body += fit(word, 244, 64, word_w, 112)
    return svg(width, 240, body, f'StarBlog {meta["nameEn"]}')


def document_asset(document):
    root = ET.fromstring(document)
    _, _, width, height = map(float, root.attrib["viewBox"].split())
    return "".join(ET.tostring(node, encoding="unicode") for node in root), width, height


def build_vectors(folder, meta):
    files = {}
    for suffix, mode in (("", "primary"), ("-mono", "mono"), ("-reversed", "reversed"), ("-current", "current")):
        files[f"mark{suffix}.svg"] = svg(256, 256, fit(master(folder, "mark", meta, mode), 16, 16, 224, 224), "StarBlog")
    word = master(folder, "wordmark", meta)
    files["wordmark.svg"] = svg(round(word[1] + 32), round(word[2] + 32), fit(word, 16, 16, word[1], word[2]), "StarBlog")
    for suffix, mode in (("", "primary"), ("-mono", "mono"), ("-reversed", "reversed")):
        files[f"logo{suffix}.svg"] = lockup(folder, meta, mode)
    if meta["wordmarkOnly"]:
        files["logo-stacked.svg"] = files["logo.svg"]
    else:
        stacked = fit(master(folder, "mark", meta), 192, 32, 256, 256)
        stacked += fit(word, 40, 340, 560, 136)
        files["logo-stacked.svg"] = svg(640, 520, stacked, "StarBlog")
    icon = f'<rect width="256" height="256" rx="56" fill="{meta["iconBackground"]}"/>'
    icon += fit(master(folder, "mark", meta, "icon"), 44, 44, 168, 168)
    files["app-icon.svg"] = svg(256, 256, icon, "StarBlog")

    paper = meta["palette"]["paper"]
    body = f'<rect width="1600" height="1000" fill="{paper}"/>'
    body += label(f'{meta["code"]} / {meta["nameEn"]} — STARBLOG IDENTITY', 64, 62, 22)
    body += fit(document_asset(files["logo.svg"]), 96, 105, 1408, 280)
    body += label("MONOCHROME", 64, 438)
    body += label("REVERSED", 832, 438)
    body += '<rect x="64" y="460" width="704" height="210" rx="12" fill="#FFFFFF"/>'
    body += f'<rect x="832" y="460" width="704" height="210" rx="12" fill="{meta["palette"]["ink"]}"/>'
    body += fit(document_asset(files["logo-mono.svg"]), 88, 482, 656, 166)
    body += fit(document_asset(files["logo-reversed.svg"]), 856, 482, 656, 166)
    body += label("APP ICON", 64, 728)
    body += fit(document_asset(files["app-icon.svg"]), 64, 755, 160, 160)
    body += label("SYMBOL / 24 / 32 / 48 / 64 PX", 320, 728)
    for offset, size in enumerate((24, 32, 48, 64)):
        body += fit(document_asset(files["mark.svg"]), 320 + offset * 112, 778, size, size)
    body += label("PALETTE", 880, 728)
    for offset, (name, color) in enumerate(meta["palette"].items()):
        x = 880 + offset * 164
        body += f'<rect x="{x}" y="755" width="132" height="64" rx="4" fill="{color}" stroke="#C9C9C9"/>'
        body += label(name.upper(), x, 850, 12)
        body += label(color, x, 874, 14)
    files["preview.svg"] = svg(1600, 1000, body, f'StarBlog {meta["nameEn"]} identity')
    return files


def rasterize(source, target, width, background="none"):
    # ImageMagick renders our vector files; no raster concept editing is involved.
    subprocess.run([
        "convert", "-background", background, "-density", "144", str(source),
        "-resize", f"{width}x", "-depth", "8", "-strip", str(target),
    ], check=True, capture_output=True)


def build(folder, meta):
    output = folder / "exports"
    output.mkdir(exist_ok=True)
    for filename, content in build_vectors(folder, meta).items():
        (output / filename).write_text(content)
    for name, width in PNG_SIZES.items():
        rasterize(output / f"{name}.svg", output / f"{name}.png", width)
    rasterize(output / "app-icon.svg", output / "apple-touch-icon.png", 180, meta["iconBackground"])
    subprocess.run([
        "convert", "-background", "none", str(output / "app-icon.svg"),
        "-define", "icon:auto-resize=64,48,32,16", str(output / "favicon.ico"),
    ], check=True, capture_output=True)
    (output / "build.json").write_text(json.dumps(receipt(folder), indent=2) + "\n")
    print(f'Built {meta["code"]} / {folder.name}')


def overview(entries):
    body = '<rect width="1800" height="1200" fill="#F0F1F3"/>'
    for index, (folder, meta) in enumerate(entries):
        x, y = (index % 3) * 600, (index // 3) * 600
        body += f'<rect x="{x+16}" y="{y+16}" width="568" height="568" rx="20" fill="{meta["palette"]["paper"]}"/>'
        body += label(f'{meta["code"]} / {meta["nameEn"]}', x + 48, y + 65, 24)
        files = build_vectors(folder, meta)
        if meta["wordmarkOnly"]:
            body += fit(document_asset(files["logo.svg"]), x + 40, y + 144, 520, 218)
        else:
            body += fit(master(folder, "mark", meta), x + 190, y + 110, 220, 220)
            body += fit(master(folder, "wordmark", meta), x + 74, y + 372, 452, 90)
        body += fit(document_asset(files["app-icon.svg"]), x + 48, y + 482, 64, 64)
        body += fit(document_asset(files["mark-mono.svg"]), x + 140, y + 482, 64, 64)
    (BRAND / "overview.svg").write_text(svg(1800, 1200, body, "StarBlog — six logo directions"))
    rasterize(BRAND / "overview.svg", BRAND / "overview.png", 1800)


def check(entries):
    for folder, meta in entries:
        if json.loads((folder / "exports/build.json").read_text()) != receipt(folder):
            raise ValueError(f"Changed inputs or outputs, rebuild: {folder.name}")
        for color in meta["palette"].values():
            if not re.fullmatch(r"#[0-9A-Fa-f]{6}", color):
                raise ValueError(f"Invalid color: {folder.name}: {color}")
        files = build_vectors(folder, meta)
        for name in SVG_NAMES:
            path = folder / "exports" / f"{name}.svg"
            if path.read_text() != files[f"{name}.svg"]:
                raise ValueError(f"Stale export, rebuild: {path}")
            root = ET.parse(path).getroot()
            for node in root.iter():
                if node.tag in (f"{{{NS}}}image", f"{{{NS}}}script", f"{{{NS}}}foreignObject"):
                    raise ValueError(f"Not a self-contained vector: {path}")
                if node.tag == f"{{{NS}}}text" and name != "preview":
                    raise ValueError(f"Logo text must be outlined: {path}")
        for name, width in {**PNG_SIZES, "apple-touch-icon": 180}.items():
            data = (folder / "exports" / f"{name}.png").read_bytes()
            if data[:8] != b"\x89PNG\r\n\x1a\n" or struct.unpack(">I", data[16:20])[0] != width:
                raise ValueError(f"Invalid PNG: {folder.name}/{name}")
        ico = (folder / "exports/favicon.ico").read_bytes()
        if struct.unpack("<HHH", ico[:6]) != (0, 1, 4):
            raise ValueError(f"Missing favicon sizes: {folder.name}")
        sizes = [ico[6 + index * 16] for index in range(4)]
        if sorted(sizes) != [16, 32, 48, 64]:
            raise ValueError(f"Wrong favicon sizes: {folder.name}")
        for filename in ("README.md", "prompt.txt", "concept.png"):
            if not (folder / filename).is_file():
                raise ValueError(f"Missing source documentation: {folder.name}/{filename}")
        print(f'Checked {meta["code"]} / {folder.name}: vectors, PNGs, ICO, sources')


def main():
    manifest = json.loads((BRAND / "manifest.json").read_text())
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--scheme", choices=manifest["schemes"], help="Build or check just one scheme")
    parser.add_argument("--check", action="store_true", help="Validate saved exports without writing")
    parser.add_argument("--list", action="store_true", help="List stable scheme IDs")
    args = parser.parse_args()
    if args.list:
        print("\n".join(manifest["schemes"]))
        return
    entries = []
    for name in manifest["schemes"]:
        if args.scheme and name != args.scheme:
            continue
        folder = BRAND / name
        meta = json.loads((folder / "brand.json").read_text())
        if meta["id"] != name:
            raise ValueError(f"Scheme ID mismatch: {name}")
        entries.append((folder, meta))
    if args.check:
        check(entries)
        return
    if not shutil.which("convert"):
        parser.error("ImageMagick convert is required for PNG and ICO exports")
    for folder, meta in entries:
        build(folder, meta)
    if not args.scheme:
        overview(entries)


if __name__ == "__main__":
    main()
