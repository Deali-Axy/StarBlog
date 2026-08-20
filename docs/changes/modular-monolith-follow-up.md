# 模块化单体重建后续事项

日期：2026-08-20

本次全量重建已经把可部署后端收敛为 `apps/api/src/StarBlog.Api`，测试归并到 `apps/api/tests/StarBlog.Api.Tests`，管理端已切换到无 `ApiResponse` 包装的新契约。以下项目仍留在磁盘上，但已移出 `StarBlog.slnx` 和默认 `task build`：

## 待从仓库删除的旧目录

这些目录不再参与编译，可以在确认备份后删除：

- `apps/api/src/StarBlog.Application`
- `apps/api/src/StarBlog.Content`
- `apps/api/src/StarBlog.Data`
- `apps/api/src/StarBlog.Infrastructure`
- `apps/web-legacy`
- `tests/StarBlog.Api.IntegrationTests`
- `tests/StarBlog.Application.UnitTests`
- `tests/StarBlog.IntegrationTests`
- `tests/StarBlog.Testing`
- `tests/StarBlog.UnitTests`
- `demo/` 下仍依赖 FreeSql 的烟雾测试

## 暂未迁入新数据模型的工具

`tools/DataProc` 与 `tools/MarkdownImporter` 仍引用旧 Data/Content 项目，因此未加入解决方案。等 CLI 改为调用 HTTP 契约或独立文件协议后再恢复构建。

继续保留的独立工具：

- `tools/BlogImageOptimizer`
- `tools/StarBlogBackup`

## 已知非阻塞项

- `BlogImageOptimizer` 存在既有可空引用警告，与本次重构无关。
- 密码算法改为 ASP.NET Identity PBKDF2，旧 SHA-256 哈希与旧 SQLite 数据均不兼容。
- `Microsoft.OpenApi` 已固定到 2.12.0，以避开传递依赖中的已知高危版本。
