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
        # A deterministic long-form article for heading/TOC and reading-layout checks.
        article_id = "articleread00001"
        now = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")
        paragraph = "把实践中的发现留在纸上，也让下一次出发有迹可循。好的阅读体验，应该让文字回到中心，让每一段思考都能被轻松找到。"
        headings = ["# 从一页手记开始", "## C# 与 .NET：实践！", "### 使用 `await` 与 **异步**", "## 重复标题", "## 重复标题", "#### 跳级章节", "##### 第五级", "###### 第六级", "## !!!", "## 继续阅读", "## 写在最后"]
        body = "\n\n".join(heading + "\n\n" + "\n\n".join([paragraph * 3] * 2) for heading in headings)
        body += "\n\n```csharp\npublic async Task<string> ReadAsync() => await File.ReadAllTextAsync(\"notes.md\");\n```"
        body += "\n\n| 项目 | 说明 |\n| :-- | --: |\n| 阅读 | 跟随文字，而不迷失方向 |\n\n> 将复杂的事，写成清晰的记录。\n\n- [x] 保留好奇\n- [ ] 继续记录"
        target.execute(
            """INSERT INTO post (id, title, slug, status, is_publish, summary, content, path,
               creation_time, last_update_time, category_id, categories)
               VALUES (?, ?, ?, NULL, 1, ?, ?, NULL, ?, ?, ?, NULL)""",
            (article_id, "把复杂的事，写成清晰的记录", "article-reading-fixture", "关于阅读、实践与记录的一页手记。在文字之间寻找线索，也为下一次出发留下方向。", body, now, now, first_id + 2),
        )
        target.execute(
            """INSERT INTO post_translation (id, post_id, language, title, summary, content, creation_time, last_update_time)
               VALUES (?, ?, 'en', ?, ?, ?, ?, ?)""",
            ("article-reading-en", article_id, "Making complex ideas clear", "Notes on reading, practice and keeping a record.", "## Start reading\n\n" + ("A clear reading experience keeps words at the center. " * 60) + "\n\n## Continue reading\n\n" + ("Keep a record. Stay curious. " * 60), now, now),
        )
        target.commit()
        print(f"Created {target_path} with a three-level category path, an empty category, 25 index articles and a translated reading fixture.")
