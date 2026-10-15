#!/usr/bin/env python3
"""Copy legacy blog content into an initialized EF Core database using only stdlib."""

import argparse
from contextlib import closing
from datetime import datetime, timezone
import filecmp
from pathlib import Path
import shutil
import sqlite3


ROOT = Path(__file__).resolve().parents[1]
TABLES = (
    "category", "post", "post_translation", "photo", "featured_post",
    "top_post", "featured_category", "featured_photo", "link", "link_exchange",
    "anonymous_user", "comment",
)
CONFIG_KEYS = ("default_render", "home_random_photo_visible", "home_chart_visible")


def quote(name):
    return '"' + name.replace('"', '""') + '"'


def tables(db):
    return {row[0] for row in db.execute("SELECT name FROM sqlite_master WHERE type='table'")}


def read_database(path):
    """Use read-only mode so a wrong path never creates or changes the source."""
    return sqlite3.connect(path.resolve().as_uri() + "?mode=ro", uri=True, timeout=30)


def import_rows(source, target):
    source_tables, target_tables = tables(source), tables(target)
    if not {"category", "post"} <= source_tables:
        raise ValueError("旧库缺少 category / post 表。")
    if not {"__EFMigrationsHistory", "category", "post"} <= target_tables:
        raise ValueError("目标必须是新版 EF Core 数据库；请先启动 API 或运行 task api:db:update。")
    selected = [table for table in TABLES if table in source_tables]
    if not set(selected) <= target_tables:
        raise ValueError("目标数据库缺少新版表，请先应用 EF Core migrations。")
    categories = target.execute('SELECT id, name, parent_id, visible FROM category').fetchall()
    placeholder = categories == [(1, "未分类", 0, 1)]
    for table in TABLES:
        if table not in target_tables or (table == "category" and placeholder):
            continue
        if target.execute(f"SELECT COUNT(*) FROM {quote(table)}").fetchone()[0]:
            raise ValueError(f"目标 {table} 已有数据；导入只支持空博客，不会覆盖已有内容。")
    if placeholder:
        target.execute('DELETE FROM category WHERE id = 1')

    # SQLite checks reply and author references at the end, regardless of row order.
    target.execute("PRAGMA defer_foreign_keys = ON")
    counts = {}
    for table in selected:
        source_columns = {row[1] for row in source.execute(f"PRAGMA table_info({quote(table)})")}
        columns = [row[1] for row in target.execute(f"PRAGMA table_info({quote(table)})")
                   if row[1] in source_columns]
        names = ", ".join(map(quote, columns))
        where = ' WHERE is_deleted = 0' if "is_deleted" in source_columns else ''
        rows = source.execute(f"SELECT {names} FROM {quote(table)}{where}").fetchall()
        if table == "comment":
            parents = {row[columns.index("id")] for row in rows}
            authors = {row[0] for row in target.execute('SELECT id FROM anonymous_user')}
            normalized = []
            for row in rows:
                values = dict(zip(columns, row))
                if values.get("parent_id") not in parents:
                    values["parent_id"] = None
                if values.get("anonymous_user_id") not in authors:
                    values["anonymous_user_id"] = None
                normalized.append(tuple(values[column] for column in columns))
            rows = normalized
        if table == "photo":
            index = columns.index("file_path")
            rows = [tuple(value.replace("\\", "/") if i == index and value else value
                          for i, value in enumerate(row)) for row in rows]
        placeholders = ", ".join("?" for _ in columns)
        target.executemany(f"INSERT INTO {quote(table)} ({names}) VALUES ({placeholders})", rows)
        counts[table] = len(rows)

    if "config" in source_tables and "config" in target_tables:
        rows = source.execute(
            'SELECT key, value, description FROM config WHERE key IN (?, ?, ?)', CONFIG_KEYS,
        ).fetchall()
        before = target.total_changes
        target.executemany(
            'INSERT INTO config (key, value, description) VALUES (?, ?, ?) ON CONFLICT(key) DO NOTHING', rows,
        )
        counts["config"] = target.total_changes - before
    violations = target.execute("PRAGMA foreign_key_check").fetchall()
    if violations:
        raise ValueError(f"导入数据存在无效外键：{violations[:5]}")
    return counts


def media_plan(source_root, target_root):
    if not source_root.is_dir():
        raise ValueError(f"找不到旧版媒体目录：{source_root}")
    pending, existing = [], 0
    for source in sorted(source_root.rglob("*")):
        if source.is_symlink():
            raise ValueError(f"媒体目录包含符号链接，请先检查：{source}")
        if not source.is_file():
            continue
        target = target_root / source.relative_to(source_root)
        if not target.resolve().is_relative_to(target_root.resolve()):
            raise ValueError(f"媒体路径越出目标目录：{target}")
        if target.exists():
            if not target.is_file() or not filecmp.cmp(source, target, shallow=False):
                raise ValueError(f"目标图片已存在且内容不同，不会覆盖：{target}")
            existing += 1
        else:
            pending.append((source, target))
    return pending, existing


def migrate(source_path, target_path, backup_dir, dry_run=False):
    if source_path.resolve() == target_path.resolve():
        raise ValueError("旧库和目标库必须是两个不同文件。")
    source_media = source_path.parent / "wwwroot" / "media"
    target_media = target_path.parent / "wwwroot" / "media"
    copied = []
    with closing(read_database(source_path)) as original, closing(sqlite3.connect(":memory:")) as source:
        original.backup(source)
        pending, existing = media_plan(source_media, target_media)
        if "photo" in tables(source):
            for (file_path,) in source.execute('SELECT file_path FROM photo'):
                path = (source_media / "photography" / file_path.replace("\\", "/")).resolve()
                if not path.is_relative_to(source_media.resolve()) or not path.is_file():
                    raise ValueError(f"旧版照片文件缺失或路径无效：{file_path}")
        # Validate all rows against the real target schema before writing any files.
        with closing(read_database(target_path)) as original_target, closing(sqlite3.connect(":memory:")) as preview:
            original_target.backup(preview)
            preview.execute("PRAGMA foreign_keys = ON")
            with preview:
                preview.execute("BEGIN IMMEDIATE")
                counts = import_rows(source, preview)
        report = {"rows": counts, "media_to_copy": len(pending), "media_existing": existing}
        if dry_run:
            return report
        backup_dir.mkdir(parents=True, exist_ok=True)
        stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
        backup_path = backup_dir / f"{target_path.stem}-before-legacy-import-{stamp}.db"
        with closing(read_database(target_path)) as original_target, closing(sqlite3.connect(backup_path)) as backup:
            original_target.backup(backup)
        try:
            with closing(sqlite3.connect(target_path, timeout=30)) as target:
                target.execute("PRAGMA foreign_keys = ON")
                with target:
                    target.execute("BEGIN IMMEDIATE")
                    counts = import_rows(source, target)
                    for source_file, target_file in pending:
                        target_file.parent.mkdir(parents=True, exist_ok=True)
                        # Exclusive creation also protects files created since preflight.
                        with target_file.open("xb") as output:
                            copied.append(target_file)
                            with source_file.open("rb") as input_file:
                                shutil.copyfileobj(input_file, output)
                        shutil.copystat(source_file, target_file)
        except BaseException:
            for path in reversed(copied):
                path.unlink(missing_ok=True)
            raise
        report["rows"] = counts
        report["backup"] = str(backup_path)
        return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=ROOT / "apps/web-legacy/app.db")
    parser.add_argument("--target", type=Path, default=ROOT / "apps/api/src/StarBlog.Api/app.db")
    parser.add_argument("--backup-dir", type=Path, default=ROOT / "backups/legacy-import")
    parser.add_argument("--dry-run", action="store_true", help="检查数据和图片，不写入任何文件")
    args = parser.parse_args()
    try:
        report = migrate(args.source, args.target, args.backup_dir, args.dry_run)
    except (ValueError, sqlite3.Error, OSError) as error:
        parser.exit(1, f"导入失败：{error}\n")
    print("检查通过（未写入）" if args.dry_run else "旧版博客数据导入完成")
    for table, count in report["rows"].items():
        print(f"  {table}: {count}")
    print(f"  媒体文件：新增 {report['media_to_copy']}，已有相同文件 {report['media_existing']}")
    if "backup" in report:
        print(f"  导入前数据库备份：{report['backup']}")


if __name__ == "__main__":
    main()
