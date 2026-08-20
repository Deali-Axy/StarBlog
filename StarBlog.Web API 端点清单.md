# StarBlog API 端点清单

当前对外 API 由 `apps/api/src/StarBlog.Api` 提供，路由以 `/api/v1/` 为主，成功响应直接返回 JSON，错误使用 `ProblemDetails`。旧的 `apps/web-legacy` `/Api/*` 地址已不再暴露。

完整路由表见 [docs/api-migration.md](docs/api-migration.md)。
