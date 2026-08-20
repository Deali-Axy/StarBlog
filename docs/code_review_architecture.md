# StarBlog 架构重构 Code Review 报告

**日期**: 2026-02-13  
**审查范围**: `StarBlog.Api`, `StarBlog.Application` 及其依赖项  
**审查者**: 资深架构师 AI

---

## 1. 总体概述
本次重构成功地将业务逻辑从 Web 层剥离到了 Application 层，形成了清晰的 **N-Tier（多层）架构**。项目结构整洁，职责划分基本合理，但也存在一些典型的架构耦合问题和代码异味（Code Smell）。

## 2. 架构与分层设计

### ✅ 优点
1.  **清晰的依赖方向**：遵循 `Api -> Application -> Data` 的引用链，符合分层原则。
2.  **Application 层职责明确**：
    *   **Services**（如 `PostService`）承载了核心业务逻辑（Markdown 解析、图片处理、数据存取）。
    *   **Api**（Controllers）保持轻量，仅负责 HTTP 协议处理、参数解析和响应封装。
3.  **混合配置管理**：`ConfigService` 实现了“数据库覆盖配置文件”的策略，既保留了 `appsettings.json` 的灵活性，又支持运行时修改配置。

### ⚠️ 改进点
*   **Application 层对 Web 框架的泄漏**：
    *   `StarBlog.Application` 引用了 `Microsoft.AspNetCore.App` 框架。
    *   **影响**：这使得应用层包含了大量 Web 特有的 API，模糊了边界，容易导致代码不小心依赖 `HttpContext`。

---

## 3. 关键问题 (Critical Issues)

### 3.1. `MessageService` 强耦合 `HttpContext`
*   **位置**：[MessageService.cs](../apps/api/src/StarBlog.Application/Contrib/SiteMessage/MessageService.cs)
*   **问题**：该服务直接注入 `IHttpContextAccessor` 并依赖 `Session` 存储消息。
    ```csharp
    if (HttpContext == null) {
        throw new Exception("There is no active HttpContext...");
    }
    ```
*   **风险**：导致该服务**无法在非 Web 环境下运行**（如后台任务 `IHostedService`、单元测试、或控制台工具）。
*   **建议**：
    *   **方案 A (推荐)**：定义 `IMessageStore` 接口，Web 层实现 `SessionMessageStore`，后台任务实现 `MemoryMessageStore`。
    *   **方案 B**：将此服务移动到 `StarBlog.Web` 或 `StarBlog.Api` 层，因为它本质上是 UI 交互的一部分。

### 3.2. API 直接暴露数据库实体 (Domain Entity Leakage)
*   **位置**：[PostService.cs](../apps/api/src/StarBlog.Application/Services/PostService.cs) 及相关 Controllers。
*   **问题**：`GetById`、`GetPagedList` 等方法直接返回 `Post` 实体（定义在 Data 层）。
    ```csharp
    public async Task<Post?> GetById(string id) { ... }
    ```
*   **风险**：
    *   **契约不稳**：数据库表结构的变化会直接破坏 API 契约（Breaking Change）。
    *   **过度暴露**：可能意外暴露不应公开的字段（如 `IsDeleted`、内部状态字段等）。
    *   **逻辑混杂**：`PostService` 中直接修改实体内容（`post.Content = ...`）用于前端展示，污染了实体本身的数据纯度。
*   **建议**：引入 `PostDto` 或 `PostViewModel`，使用 `AutoMapper` 在 Service 层或 Controller 层完成映射。

---

## 4. 代码质量与最佳实践

### 4.1. 异步 IO 的隐患
*   **位置**：`PostService.cs` -> `MdExternalUrlDownloadAsync`
*   **观察**：在保存文章时，会同步等待外部图片下载。
*   **建议**：建议引入后台任务队列（如 `Channel<T>` 或 Hangfire），将“图片本地化”作为后台作业异步处理，避免阻塞文章保存操作。

### 4.2. 混合 ORM 的使用
*   **观察**：项目同时使用了 `FreeSql` (业务数据) 和 `EF Core` (SQLite 日志)。
*   **评价**：增加了维护成本和认知负担（开发人员需掌握两套查询语法）。若非为了兼容旧代码，建议逐步统一。

---

## 5. 总结与行动指南

**综合评分**: **B+**  
重构方向正确，代码组织良好，但在**层级隔离**和**API 契约设计**上仍有提升空间。

**接下来的行动建议：**

1.  **Refactor `MessageService`**：优先将其解耦，确保 Application 层的纯洁性。
2.  **Introduce DTOs**：从最核心的 `Post` 接口开始，使用 ViewModel 隔离数据库实体。
3.  **Optimize Image Downloading**：改为后台队列处理图片下载。
