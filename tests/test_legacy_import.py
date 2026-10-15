import importlib.util
from contextlib import closing, contextmanager
from pathlib import Path
import sqlite3
import tempfile
import unittest
from unittest.mock import patch


spec = importlib.util.spec_from_file_location(
    "legacy_import", Path(__file__).resolve().parents[1] / "scripts/Import-LegacyBlog.py",
)
legacy_import = importlib.util.module_from_spec(spec)
spec.loader.exec_module(legacy_import)


@contextmanager
def database(path):
    with closing(sqlite3.connect(path)) as db:
        with db:
            yield db


class LegacyImportTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        root = Path(self.temp.name)
        self.source = root / "legacy/app.db"
        self.target = root / "api/app.db"
        self.backups = root / "backups"
        self.source.parent.mkdir()
        self.target.parent.mkdir()
        self.source_media = self.source.parent / "wwwroot/media/photography/photo.jpg"
        self.source_media.parent.mkdir(parents=True)
        self.source_media.write_bytes(b"legacy-photo")
        self.target_media = self.target.parent / "wwwroot/media/photography/photo.jpg"
        with database(self.source) as db:
            db.executescript("""
                CREATE TABLE category (id INTEGER PRIMARY KEY, name TEXT, parent_id INTEGER, visible INTEGER);
                INSERT INTO category VALUES (1, 'AI', 0, 1), (2, 'Python', 1, 1);
                CREATE TABLE post (id TEXT PRIMARY KEY, title TEXT, category_id INTEGER, is_publish INTEGER);
                INSERT INTO post VALUES ('published', 'Article', 2, 1), ('draft', 'Draft', 1, 0);
                CREATE TABLE anonymous_user (id TEXT PRIMARY KEY, email TEXT, is_deleted INTEGER);
                INSERT INTO anonymous_user VALUES ('author', 'author@example.com', 0), ('deleted', 'deleted@example.com', 1);
                CREATE TABLE comment (id TEXT PRIMARY KEY, parent_id TEXT, anonymous_user_id TEXT, is_deleted INTEGER);
                INSERT INTO comment VALUES ('reply', 'root', 'author', 0), ('root', '', 'author', 0),
                    ('removed', '', 'author', 1), ('orphan', 'removed', 'deleted', 0);
                CREATE TABLE photo (id TEXT PRIMARY KEY, file_path TEXT);
                INSERT INTO photo VALUES ('photo', 'photo.jpg');
                CREATE TABLE config (id INTEGER PRIMARY KEY, key TEXT, value TEXT, description TEXT);
                INSERT INTO config VALUES (1, 'host', 'https://old.example.com', NULL),
                    (2, 'home_chart_visible', 'true', NULL), (3, 'is_init', 'true', NULL);
            """)
        with database(self.target) as db:
            db.executescript("""
                CREATE TABLE __EFMigrationsHistory (migration_id TEXT PRIMARY KEY);
                INSERT INTO __EFMigrationsHistory VALUES ('InitialCreate');
                CREATE TABLE category (id INTEGER PRIMARY KEY, name TEXT NOT NULL, parent_id INTEGER, visible INTEGER);
                INSERT INTO category VALUES (1, '未分类', 0, 1);
                CREATE TABLE post (id TEXT PRIMARY KEY, title TEXT NOT NULL, category_id INTEGER REFERENCES category(id), is_publish INTEGER);
                CREATE TABLE anonymous_user (id TEXT PRIMARY KEY, email TEXT NOT NULL UNIQUE);
                CREATE TABLE comment (id TEXT PRIMARY KEY, parent_id TEXT REFERENCES comment(id), anonymous_user_id TEXT REFERENCES anonymous_user(id));
                CREATE TABLE photo (id TEXT PRIMARY KEY, file_path TEXT NOT NULL);
                CREATE TABLE config (id INTEGER PRIMARY KEY, key TEXT UNIQUE, value TEXT, description TEXT);
                INSERT INTO config VALUES (1, 'host', 'http://localhost:5039', NULL);
                CREATE TABLE user (id TEXT PRIMARY KEY, password TEXT);
                INSERT INTO user VALUES ('admin', 'existing-password');
            """)

    def migrate(self, dry_run=False):
        return legacy_import.migrate(self.source, self.target, self.backups, dry_run)

    def test_import_preserves_relations_drafts_local_settings_and_source(self):
        source_before = self.source.read_bytes()
        report = self.migrate()
        self.assertEqual(source_before, self.source.read_bytes())
        self.assertEqual(self.target_media.read_bytes(), b"legacy-photo")
        with database(self.target) as db:
            self.assertEqual(db.execute('SELECT name FROM category WHERE id=1').fetchone(), ('AI',))
            self.assertEqual(db.execute('SELECT is_publish FROM post WHERE id="draft"').fetchone(), (0,))
            self.assertEqual(db.execute('SELECT parent_id FROM comment WHERE id="reply"').fetchone(), ('root',))
            self.assertEqual(db.execute('SELECT parent_id FROM comment WHERE id="root"').fetchone(), (None,))
            self.assertEqual(db.execute('SELECT parent_id, anonymous_user_id FROM comment WHERE id="orphan"').fetchone(), (None, None))
            self.assertEqual(db.execute('SELECT COUNT(*) FROM comment').fetchone(), (3,))
            self.assertEqual(db.execute('SELECT value FROM config WHERE key="host"').fetchone(), ('http://localhost:5039',))
            self.assertEqual(db.execute('SELECT value FROM config WHERE key="home_chart_visible"').fetchone(), ('true',))
            self.assertIsNone(db.execute('SELECT value FROM config WHERE key="is_init"').fetchone())
            self.assertEqual(db.execute('SELECT password FROM user').fetchone(), ('existing-password',))
            self.assertEqual(db.execute('PRAGMA foreign_key_check').fetchall(), [])
        with database(report['backup']) as db:
            self.assertEqual(db.execute('SELECT COUNT(*) FROM post').fetchone(), (0,))
            self.assertEqual(db.execute('SELECT name FROM category').fetchone(), ('未分类',))

    def test_dry_run_checks_schema_without_writing(self):
        before = self.target.read_bytes()
        self.assertEqual(self.migrate(dry_run=True)['rows']['post'], 2)
        self.assertEqual(self.target.read_bytes(), before)
        self.assertFalse(self.backups.exists())
        self.assertFalse(self.target_media.exists())

    def test_repeat_import_refuses_to_overwrite_existing_content(self):
        self.migrate()
        before = self.target.read_bytes()
        with self.assertRaisesRegex(ValueError, '已有数据'):
            self.migrate()
        self.assertEqual(self.target.read_bytes(), before)

    def test_conflicting_media_is_never_overwritten(self):
        self.target_media.parent.mkdir(parents=True)
        self.target_media.write_bytes(b"new-photo")
        before = self.target.read_bytes()
        with self.assertRaisesRegex(ValueError, '内容不同'):
            self.migrate()
        self.assertEqual(self.target.read_bytes(), before)
        self.assertEqual(self.target_media.read_bytes(), b"new-photo")
        self.assertFalse(self.backups.exists())

    def test_invalid_source_relation_fails_before_any_writes(self):
        with database(self.source) as db:
            db.execute('UPDATE post SET category_id=999')
        before = self.target.read_bytes()
        with self.assertRaisesRegex(ValueError, '无效外键'):
            self.migrate()
        self.assertEqual(self.target.read_bytes(), before)
        self.assertFalse(self.target_media.exists())
        self.assertFalse(self.backups.exists())

    def test_failed_media_copy_rolls_back_database_and_partial_file(self):
        with patch.object(legacy_import.shutil, 'copyfileobj', side_effect=OSError('disk full')):
            with self.assertRaisesRegex(OSError, 'disk full'):
                self.migrate()
        self.assertFalse(self.target_media.exists())
        with database(self.target) as db:
            self.assertEqual(db.execute('SELECT COUNT(*) FROM post').fetchone(), (0,))
            self.assertEqual(db.execute('SELECT name FROM category').fetchone(), ('未分类',))


if __name__ == '__main__':
    unittest.main()
