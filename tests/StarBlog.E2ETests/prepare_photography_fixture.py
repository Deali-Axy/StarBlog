"""Copy a populated database and add photo fixtures without changing the source."""

import argparse
import sqlite3
import uuid
from datetime import datetime, timedelta, timezone
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("source", type=Path)
parser.add_argument("target", type=Path)
args = parser.parse_args()
source_path = args.source.resolve(strict=True)
target_path = args.target.resolve()
if source_path == target_path or target_path.exists():
    parser.error("target must be a new database file, different from source")

target_path.parent.mkdir(parents=True, exist_ok=True)
with sqlite3.connect(f"{source_path.as_uri()}?mode=ro", uri=True) as source:
    with sqlite3.connect(target_path) as target:
        source.backup(target)
        columns = [row[1] for row in target.execute("PRAGMA table_info(photo)")]
        photos = target.execute("SELECT * FROM photo LIMIT 35").fetchall()
        if not photos:
            parser.error("source must contain photos and their media files")
        count = target.execute("SELECT COUNT(*) FROM photo").fetchone()[0]
        insert = f"INSERT INTO photo ({','.join(columns)}) VALUES ({','.join('?' for _ in columns)})"
        for index in range(max(35, 70 - count)):
            values = list(photos[index % len(photos)])
            values[columns.index("id")] = uuid.uuid4().hex[:16]
            values[columns.index("title")] = f"Photography fixture {index}"
            values[columns.index("create_time")] = (
                datetime.now(timezone.utc) + timedelta(days=7, minutes=index)
            ).strftime("%Y-%m-%d %H:%M:%S")
            if index == max(35, 70 - count) - 1:
                values[columns.index("title")] = "Composition and light " * 20
                values[columns.index("location")] = "Mountains and sea " * 12
            target.execute(insert, values)
        target.commit()
        print(f"Created {target_path} with {target.execute('SELECT COUNT(*) FROM photo').fetchone()[0]} photos")
