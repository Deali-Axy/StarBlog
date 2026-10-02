# StarBlog Web

公开博客前台，Next.js 16 App Router，调用 `apps/api` 的 `/api/v1` 契约。

## 本地开发

先启动 API（默认 `http://localhost:5039`），再启动本应用：

```bash
task api:dev
task web:dev
```

或在仓库根目录执行 `task dev`，会并行启动 API、Admin 和 Web。

Web 开发服务器：<http://localhost:3001>

复制 [`.env.example`](.env.example) 为 `.env.local` 可覆盖站点名和后端地址。Next 会把 `/api`、`/media`、`/feed` 代理到 API。
