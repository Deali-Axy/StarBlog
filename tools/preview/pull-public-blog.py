#!/usr/bin/env python3
"""Download a bounded, public-only blog fixture. Never reads production SQLite or credentials."""
import argparse
from concurrent.futures import ThreadPoolExecutor
import json
from pathlib import Path
import re
import sqlite3
import time
from urllib.parse import quote, unquote, urljoin, urlsplit
from urllib.request import Request, urlopen


def download(url, limit=20 * 1024 * 1024):
    for attempt in range(3):
        try:
            with urlopen(Request(url, headers={'User-Agent': 'StarBlog-local-preview/1.0'}), timeout=40) as response:
                data = response.read(limit + 1)
                if len(data) > limit:
                    raise ValueError('Response exceeds size limit')
                return data
        except Exception:
            if attempt == 2:
                raise
            time.sleep(attempt + 1)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--origin', default='https://blog.deali.cn')
    parser.add_argument('--schema-db', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--posts', type=int, default=24)
    parser.add_argument('--images-per-post', type=int, default=12)
    args = parser.parse_args()
    origin = args.origin.rstrip('/')
    if urlsplit(origin).scheme not in ('http', 'https'):
        parser.error('origin must be an HTTP(S) URL')
    if not 1 <= args.posts <= 100 or not 1 <= args.images_per_post <= 50:
        parser.error('posts: 1-100; images-per-post: 1-50')
    if args.output.exists():
        parser.error('output must be a new directory; existing local data is never overwritten')
    def api(path):
        return json.loads(download(origin + path))
    summaries = api(f'/api/v1/posts?page=1&pageSize={args.posts}&sortBy=lastUpdateTime&sortType=desc')['items']
    tree = api('/api/v1/categories/tree')
    args.output.mkdir(parents=True)
    source = sqlite3.connect(args.schema_db.resolve().as_uri() + '?mode=ro', uri=True)
    db = sqlite3.connect(args.output / 'app.db')
    # Copy schema, not rows: excludes local accounts, settings, drafts, comments and queued jobs.
    for (sql,) in source.execute("select sql from sqlite_master where type='table' and name not like 'sqlite_%' and sql is not null"):
        db.execute(sql)
    for (sql,) in source.execute("select sql from sqlite_master where type='index' and sql is not null"):
        db.execute(sql)
    for row in source.execute('select * from __EFMigrationsHistory'):
        db.execute('insert into __EFMigrationsHistory values (?,?)', row)
    source.close()
    def categories(nodes, parent=0):
        for node in nodes:
            db.execute('insert or ignore into category(id,name,parent_id,visible) values (?,?,?,1)', (node['id'], node['text'], parent))
            categories(node.get('nodes') or [], node['id'])
    categories(tree)
    posts, jobs = [], {}
    for summary in summaries:
        post = api('/api/v1/posts/' + quote(summary['id'], safe='') + '?includeToc=false')
        if not post.get('isPublish'):
            continue
        body = post.get('content') or ''
        urls = re.findall(r'!\[[^\]]*\]\(\s*(<[^>]+>|[^\s)]+)', body)
        urls += re.findall(r'<img\b[^>]*\bsrc=["\x27]([^"\x27]+)', body, re.I)
        for raw in list(dict.fromkeys(urls))[:args.images_per_post]:
            raw = raw.strip('<>')
            url = urljoin(origin + f"/media/blog/{post['id']}/", raw.replace('\\', '/'))
            parsed = urlsplit(url)
            if parsed.netloc != urlsplit(origin).netloc or not parsed.path.startswith('/media/blog/'):
                continue
            relative = Path(unquote(parsed.path).lstrip('/'))
            if '..' in relative.parts or relative.is_absolute():
                continue
            url = parsed._replace(path=quote(unquote(parsed.path), safe='/')).geturl()
            jobs[url] = args.output / 'wwwroot' / relative
        post['content'] = body.replace(origin + '/media/', '/media/')
        posts.append(post)
    total_bytes, successful, failures = 0, 0, []
    def media(job):
        url, destination = job
        try:
            data = download(url)
            return destination, data, None
        except Exception as error:
            return destination, None, type(error).__name__
    with ThreadPoolExecutor(max_workers=3) as pool:
        for destination, data, error in pool.map(media, jobs.items()):
            if error or total_bytes + len(data) > 300 * 1024 * 1024:
                failures.append(str(destination.relative_to(args.output)))
                continue
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_bytes(data)
            total_bytes += len(data)
            successful += 1
    for post in posts:
        db.execute('insert or ignore into category(id,name,parent_id,visible) values (?,?,0,1)', (post['categoryId'], post.get('categoryName') or 'Uncategorized'))
        db.execute('''insert into post(id,title,slug,status,is_publish,summary,content,path,creation_time,last_update_time,category_id,categories)
            values (?,?,?,?,?,?,?,?,?,?,?,?)''', (post['id'], post['title'], post.get('slug'), post.get('status'), 1,
            post.get('summary'), post['content'], post.get('path'), post['creationTime'], post['lastUpdateTime'], post['categoryId'], post.get('categories')))
    db.commit()
    assert db.execute('pragma quick_check').fetchone()[0] == 'ok'
    assert not db.execute('pragma foreign_key_check').fetchall()
    db.close()
    report = {'origin': origin, 'posts': len(posts), 'downloadedImages': successful, 'imageBytes': total_bytes, 'failedImages': failures}
    (args.output / 'report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2))
    print(json.dumps(report, ensure_ascii=False), flush=True)


if __name__ == '__main__':
    main()
