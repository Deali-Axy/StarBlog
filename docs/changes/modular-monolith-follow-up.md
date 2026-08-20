# 模块化单体重建后续事项

日期：2026-08-20

可部署后端是 `apps/api/src/StarBlog.Api`，测试归并到 `apps/api/tests/StarBlog.Api.Tests`，管理端使用无 `ApiResponse` 包装的新契约。旧分层项目和旧测试目录已从仓库删除。

## 仍留在磁盘、但不参与默认构建的目录

这些路径已移出 `StarBlog.slnx` 和 `task build`。删除前请确认没有本地未备份数据：

- `apps/web-legacy`：旧 Razor/MVC 宿主
- `demo/`：部分烟雾测试仍依赖已删除的 FreeSql 项目，当前无法编译
- `dist/`：旧 `StarBlog.Web` 发布产物

## 暂未迁入新数据模型的工具

`tools/DataProc` 与 `tools/MarkdownImporter` 仍引用已删除的 Data/Content 项目，因此未加入解决方案。Markdown 批量导入已由 API `POST /api/v1/posts/imports` 承接；CLI 若仍需要独立运行，应改为调用该 HTTP 契约或纯文件协议后再恢复构建。

继续保留并纳入解决方案的独立工具：

- `tools/BlogImageOptimizer`
- `tools/StarBlogBackup`（默认备份 `apps/api/src/StarBlog.Api`）

## 已知非阻塞项

- `BlogImageOptimizer` 存在既有可空引用警告，与本次重构无关。
- 密码算法改为 ASP.NET Identity PBKDF2，旧 SHA-256 哈希与旧 SQLite 数据均不兼容。
- `Microsoft.OpenApi` 已固定到 2.12.0，以避开传递依赖中的已知高危版本。
- 模块内部类型目前多为 `public`。跨模块写入仍靠命名空间约定和代码审查约束；只有出现边界失控时才拆独立程序集。
