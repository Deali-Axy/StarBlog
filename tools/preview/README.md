# 真实文章配图预览

`pull-public-blog.py` 只使用匿名 GET 请求下载公开文章和站内图片，不连接生产 SQLite，不复制管理员、站点密钥、评论或后台任务。输出目录必须不存在，避免覆盖本地数据。下载结果放在已忽略的 `artifacts/` 中，不提交原始业务内容与图片。

从本地现有数据库复制**表结构和迁移版本**，再导入 24 篇公开文章及每篇最多 12 张站内图片：

```sh
python3 tools/preview/pull-public-blog.py \
  --schema-db apps/api/src/StarBlog.Api/app.db \
  --output artifacts/blog-preview-live-20261117 \
  --posts 24
```

脚本保留原文章 ID、分类、时间、slug 和图片路径；本站绝对媒体地址转为本地相对地址。最多下载 100 篇文章，每张图片最多 20 MiB、总下载不超过 300 MiB，最多三个并行下载。外站图片不代理下载。`report.json` 记录文章数、图片数、字节数和失败图片；SQLite 完成完整性及外键检查。正文引用式图片和含特殊字符的复杂 Markdown 图片语法不属于这个轻量同步脚本的解析范围；应用本身的选图使用 Markdig 完整解析。

启动独立 API（将下面的绝对路径替换成当前检出路径）：

```sh
cd apps/api/src/StarBlog.Api
ConnectionStrings__Default='Data Source=/home/deali/code/starblog/starblog/artifacts/blog-preview-live-20261117/app.db' \
ASPNETCORE_URLS=http://127.0.0.1:15039 \
Outbox__BatchSize=0 \
dotnet run --no-launch-profile -- \
  --webroot /home/deali/code/starblog/starblog/artifacts/blog-preview-live-20261117/wwwroot
```

API 会将新迁移应用到这个测试副本。另一个终端启动 Web：

```sh
cd apps/web
INTERNAL_API_BASE_URL=http://127.0.0.1:15039 \
node node_modules/next/dist/bin/next dev --port 3002
```

如果 Web 已经在 `3001` 运行，可以让它直接使用这个测试 API：在 `apps/web/.env.local` 中设置 `INTERNAL_API_BASE_URL=http://127.0.0.1:15039`，保持上面的 API 进程运行，然后刷新 `http://localhost:3001/blog?bv=editorial`。这只切换 Web 的本地数据源，不修改原来的 `5039` API 或数据库。恢复原数据源时，删除 `.env.local` 中这一行；若运行中的 Next 未自动更新 rewrite，重启 Web 开发服务即可。

同一源码目录已有 Next 开发服务器时不能再启动第二个实例。请在已有实例中使用同一 API，或将 Web 源码复制到另一个忽略目录，独立使用构建缓存；不要终止不属于当前任务的开发服务器。

预览入口：`http://localhost:3002/blog?bv=editorial`。浏览器回归及截图：

```sh
E2E_BASE_URL=http://127.0.0.1:3002 \
E2E_CHROMIUM_EXECUTABLE_PATH=/usr/bin/google-chrome \
E2E_ILLUSTRATION_SCREENSHOT_DIR="$PWD/artifacts/blog-preview-live-20261117/screenshots" \
dotnet test tests/StarBlog.E2ETests \
  --filter FullyQualifiedName~ArticleIllustrationTests
```

测试还覆盖单图/拼图切换、刷新后记忆、未提交搜索的保留，以及图片失败后拼图重排。测试涵盖 375/768/1440 宽度、浅深主题、图片加载与文章跳转、失败回退、紧凑列表及其他主题兼容性。它要求测试 API 的第一篇文章带有合适的站内配图，因此应连接下载后的独立数据，而非空库。
