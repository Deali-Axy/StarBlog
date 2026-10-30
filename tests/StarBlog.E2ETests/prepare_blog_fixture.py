"""Copy a populated database and add blog edge cases without changing the source."""

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
        first_id = target.execute("SELECT COALESCE(MAX(id), 0) + 1 FROM category").fetchone()[0]
        categories = [
            (first_id, "Blog E2E root", 0, 1),
            (first_id + 1, "Blog E2E branch", first_id, 1),
            (first_id + 2, "Blog E2E leaf", first_id + 1, 1),
            (first_id + 3, "Blog E2E empty", first_id, 1),
        ]
        target.executemany("INSERT INTO category (id, name, parent_id, visible) VALUES (?, ?, ?, ?)", categories)
        for index in range(25):
            post_id = uuid.uuid4().hex[:16]
            date = (datetime.now(timezone.utc) + timedelta(days=7, minutes=index)).strftime("%Y-%m-%d %H:%M:%S")
            title = "Blog E2E article " + str(index)
            summary = "A reproducible article for category navigation, pagination, and responsive design checks."
            if index == 24:
                title = "Blog E2E 长标题与无摘要 " + "ReactTypeScript" * 12
                summary = None
            target.execute(
                """INSERT INTO post (id, title, slug, status, is_publish, summary, content, path,
                   creation_time, last_update_time, category_id, categories)
                   VALUES (?, ?, ?, NULL, 1, ?, ?, NULL, ?, ?, ?, NULL)""",
                (post_id, title, f"blog-e2e-{post_id}", summary, "# Blog fixture\n\nA test article.", date, date, first_id + 2),
            )
        target.commit()
        print(f"Created {target_path} with a three-level category path, an empty category and 25 articles.")
