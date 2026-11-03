#!/usr/bin/env python3
"""Snapshot paid translations and current EF administrator without printing credentials."""
from contextlib import closing
import json
import os
from pathlib import Path
import re
import sqlite3
import subprocess
import sys
import tarfile

ROOT = Path(__file__).resolve().parents[1]
version = sys.argv[1]
if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]+", version):
    raise SystemExit("Invalid release version")
os.umask(0o077)
backup_dir = ROOT / "backups/production-releases" / version
backup_dir.mkdir(parents=True, exist_ok=True)
snapshot = backup_dir / "local.db"
local = ROOT / "apps/api/src/StarBlog.Api/app.db"
if not snapshot.exists():
    temporary = snapshot.with_suffix(".partial")
    with closing(sqlite3.connect(local.as_uri() + "?mode=ro", uri=True)) as original, closing(sqlite3.connect(temporary)) as backup:
        original.backup(backup)
        if backup.execute('PRAGMA quick_check').fetchall() != [('ok',)]:
            raise SystemExit("Local snapshot failed integrity check")
    temporary.replace(snapshot)

inputs = ROOT / ".ship/inputs" / version
inputs.mkdir(parents=True, exist_ok=True)
translations = inputs / "translations.db"
translations.unlink(missing_ok=True)
with closing(sqlite3.connect(snapshot.as_uri() + "?mode=ro", uri=True)) as source, closing(sqlite3.connect(translations)) as target:
    target.executescript('''CREATE TABLE __EFMigrationsHistory (migration_id TEXT PRIMARY KEY, product_version TEXT NOT NULL);
        CREATE TABLE post (id TEXT PRIMARY KEY, title TEXT NOT NULL, content TEXT);''')
    target.execute(source.execute("SELECT sql FROM sqlite_master WHERE type='table' AND name='post_translation'").fetchone()[0])
    with target:
        target.executemany('INSERT INTO __EFMigrationsHistory VALUES (?, ?)', source.execute('SELECT * FROM __EFMigrationsHistory'))
        target.executemany('INSERT INTO post VALUES (?, ?, ?)', source.execute('SELECT id,title,content FROM post WHERE id IN (SELECT post_id FROM post_translation)'))
        target.executemany('INSERT INTO post_translation VALUES (?, ?, ?, ?, ?, ?, ?, ?)', source.execute('SELECT * FROM post_translation'))
    users = source.execute('SELECT id,name,password FROM user').fetchall()
    if not users:
        raise SystemExit("No existing EF administrator to preserve")
    (inputs / "identity.json").write_text(json.dumps(users))
    count = target.execute('SELECT COUNT(*) FROM post_translation').fetchone()[0]
archive = inputs / "migration-inputs.tar.gz"
temporary_archive = archive.with_suffix(".partial")
with tarfile.open(temporary_archive, "w:gz") as output:
    output.add(translations, arcname="translations.db")
    output.add(inputs / "identity.json", arcname="identity.json")
temporary_archive.replace(archive)
subprocess.run(["ssh", "deali.cn", "umask 077; mkdir -p /home/deali/projects/starblog-v2/migration"], check=True)
subprocess.run(["scp", str(archive), "deali.cn:/home/deali/projects/starblog-v2/migration/inputs.tar.gz"], check=True)
print(f"Preserved {count} translations and {len(users)} existing EF administrator(s); credentials were not printed.")
