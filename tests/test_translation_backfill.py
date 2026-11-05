import importlib.util
from contextlib import closing, contextmanager
from pathlib import Path
import sqlite3
import tempfile
import unittest


spec = importlib.util.spec_from_file_location(
    "translation_backfill",
    Path(__file__).resolve().parents[1] / "scripts/translation_backfill.py",
)
backfill = importlib.util.module_from_spec(spec)
spec.loader.exec_module(backfill)


SCHEMA = """
CREATE TABLE post (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    summary TEXT,
    content TEXT,
    is_publish INTEGER NOT NULL,
    last_update_time TEXT
);
CREATE TABLE post_translation (
    id TEXT PRIMARY KEY,
    post_id TEXT NOT NULL,
    language TEXT NOT NULL,
    title TEXT NOT NULL,
    summary TEXT,
    content TEXT,
    creation_time TEXT NOT NULL,
    last_update_time TEXT NOT NULL,
    FOREIGN KEY (post_id) REFERENCES post(id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_post_translation_post_id_language
    ON post_translation (post_id, language);
"""


@contextmanager
def database(path):
    with closing(sqlite3.connect(path)) as db:
        db.execute("PRAGMA foreign_keys = ON")
        with db:
            yield db


class TranslationBackfillTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.db_path = self.root / "app.db"
        with database(self.db_path) as db:
            db.executescript(SCHEMA)
            db.execute(
                "INSERT INTO post VALUES (?, ?, ?, ?, ?, ?)",
                (
                    "post-missing",
                    "中文标题",
                    "摘要",
                    "Intro\n\n```python\nprint(1)\n```\n\nSee ![img](./pic.png) and [doc](https://example.com/a).",
                    1,
                    "2026-11-05 00:00:00",
                ),
            )
            db.execute(
                "INSERT INTO post VALUES (?, ?, ?, ?, ?, ?)",
                ("post-existing", "已有译文的文章", "s", "body", 1, "2026-01-01 00:00:00"),
            )
            db.execute(
                "INSERT INTO post_translation VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                ("old-en-id", "post-existing", "en", "Existing title", "sum", "Existing body", "2020-01-01", "2020-01-01"),
            )

    def source(self, **overrides):
        title, summary, content = "中文标题", "摘要", (
            "Intro\n\n```python\nprint(1)\n```\n\nSee ![img](./pic.png) and [doc](https://example.com/a)."
        )
        src = {
            "id": "post-missing",
            "title": title,
            "summary": summary,
            "content": content,
            "sourceHash": backfill.source_hash(title, summary, content),
        }
        src.update(overrides)
        return src

    def good_result(self, **overrides):
        result = {
            "title": "English title",
            "summary": "Summary",
            "content": (
                "Intro in English\n\n```python\nprint(1)\n```\n\n"
                "See ![img](./pic.png) and [doc](https://example.com/a)."
            ),
        }
        result.update(overrides)
        return result

    def test_source_hash_distinguishes_null_from_empty(self):
        empty = backfill.source_hash("t", "", "c")
        none = backfill.source_hash("t", None, "c")
        self.assertNotEqual(empty, none)
        self.assertEqual(backfill.source_hash("t", "", "c"), empty)

    def test_insert_succeeds_for_missing_published_post(self):
        src, result = self.source(), self.good_result()
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            report = backfill.insert_translation(db, src, result, now="2026-11-05 00:00:00")
            self.assertEqual(report["status"], "inserted")
            row = db.execute(
                "SELECT title, content, language FROM post_translation WHERE post_id=?",
                ("post-missing",),
            ).fetchone()
            self.assertEqual(row["title"], "English title")
            self.assertIn("print(1)", row["content"])
            self.assertEqual(row["language"], "en")

    def test_second_insert_does_not_change_stored_title_or_content(self):
        src = self.source()
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            first = backfill.insert_translation(db, src, self.good_result(), now="2026-11-05 00:00:00")
            self.assertEqual(first["status"], "inserted")
            second = backfill.insert_translation(
                db,
                src,
                self.good_result(title="CHANGED TITLE", content="changed body without fences"),
                now="2026-11-05 01:00:00",
            )
            self.assertIn(second["status"], {"exists", "conflict", "rejected"})
            row = db.execute(
                "SELECT id, title, content FROM post_translation WHERE post_id=? AND language='en'",
                ("post-missing",),
            ).fetchone()
            self.assertEqual(row["id"], first["id"])
            self.assertEqual(row["title"], "English title")
            self.assertIn("print(1)", row["content"])

    def test_rejects_dropped_code_fence(self):
        src = self.source()
        result = self.good_result(content="Intro in English with no fence. See ![img](./pic.png) and [doc](https://example.com/a).")
        errors = backfill.validate_translation(src, result)
        self.assertIn("code-fence-changed", errors)
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            report = backfill.insert_translation(db, src, result)
            self.assertEqual(report["status"], "rejected")
            self.assertIsNone(
                db.execute("SELECT id FROM post_translation WHERE post_id='post-missing'").fetchone()
            )

    def test_accepts_reordered_link_targets(self):
        src = self.source()
        result = self.good_result(
            content=(
                "See [doc](https://example.com/a) then ![img](./pic.png).\n\n"
                "```python\nprint(1)\n```\n"
            )
        )
        self.assertEqual(backfill.validate_translation(src, result), [])

    def test_rejects_changed_image_target(self):
        src = self.source()
        result = self.good_result(
            content="Intro\n\n```python\nprint(1)\n```\n\nSee ![img](https://evil.example/x.png) and [doc](https://example.com/a)."
        )
        errors = backfill.validate_translation(src, result)
        self.assertIn("image-target-changed", errors)
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            report = backfill.insert_translation(db, src, result)
            self.assertEqual(report["status"], "rejected")

    def test_rejects_title_longer_than_256(self):
        src = self.source()
        result = self.good_result(title="E" * 257)
        errors = backfill.validate_translation(src, result)
        self.assertIn("title-too-long", errors)
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            report = backfill.insert_translation(db, src, result)
            self.assertEqual(report["status"], "rejected")

    def test_does_not_overwrite_preexisting_en_row(self):
        src = {
            "id": "post-existing",
            "title": "已有译文的文章",
            "summary": "s",
            "content": "body",
            "sourceHash": backfill.source_hash("已有译文的文章", "s", "body"),
        }
        result = {"title": "New", "summary": "n", "content": "new body"}
        with database(self.db_path) as db:
            db.row_factory = sqlite3.Row
            report = backfill.insert_translation(db, src, result)
            self.assertEqual(report["status"], "conflict")
            row = db.execute(
                "SELECT id, title, content FROM post_translation WHERE post_id='post-existing'"
            ).fetchone()
            self.assertEqual(row["id"], "old-en-id")
            self.assertEqual(row["title"], "Existing title")
            self.assertEqual(row["content"], "Existing body")

    def test_plan_and_apply_dry_run_from_snapshot_copy(self):
        snapshot = self.root / "snapshot.db"
        snapshot.write_bytes(self.db_path.read_bytes())
        work = self.root / "work"
        manifest = backfill.plan_from_snapshot(snapshot, work)
        self.assertEqual(manifest["missing_count"], 1)
        self.assertEqual(manifest["posts"][0]["id"], "post-missing")
        src = backfill.load_source(work, "post-missing")
        (work / "tasks" / "post-missing.out.json").write_text(
            __import__("json").dumps(self.good_result()), encoding="utf-8"
        )
        reports = backfill.apply_directory(self.db_path, work, dry_run=True)
        statuses = {row["post_id"]: row["status"] for row in reports}
        self.assertEqual(statuses["post-missing"], "would-insert")
        with database(self.db_path) as db:
            self.assertIsNone(
                db.execute("SELECT id FROM post_translation WHERE post_id='post-missing'").fetchone()
            )
            existing = db.execute(
                "SELECT title FROM post_translation WHERE id='old-en-id'"
            ).fetchone()
            self.assertEqual(existing[0], "Existing title")
        self.assertEqual(src["sourceHash"], backfill.source_hash(src["title"], src["summary"], src["content"]))

    def test_apply_jsonl_inserts_and_refuses_overwrite(self):
        src, result = self.source(), self.good_result()
        jsonl = self.root / "accepted.jsonl"
        jsonl.write_text(__import__("json").dumps({"source": src, "result": result}) + "\n", encoding="utf-8")
        dry = backfill.apply_jsonl(self.db_path, jsonl, dry_run=True)
        self.assertEqual(dry[0]["status"], "would-insert")
        with database(self.db_path) as db:
            self.assertIsNone(db.execute("SELECT id FROM post_translation WHERE post_id='post-missing'").fetchone())
        live = backfill.apply_jsonl(self.db_path, jsonl)
        self.assertEqual(live[0]["status"], "inserted")
        again = backfill.apply_jsonl(
            self.db_path,
            jsonl,
        )
        self.assertEqual(again[0]["status"], "exists")
        with database(self.db_path) as db:
            row = db.execute(
                "SELECT title FROM post_translation WHERE post_id='post-missing'"
            ).fetchone()
            self.assertEqual(row[0], "English title")
            existing = db.execute("SELECT title FROM post_translation WHERE id='old-en-id'").fetchone()
            self.assertEqual(existing[0], "Existing title")


if __name__ == "__main__":
    unittest.main()
