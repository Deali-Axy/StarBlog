# StarBlog 模块化单体与 EF Core 全量重建方案

日期：2026-08-20  
状态：已批准，待实施

## 1. 背景与重构前提

StarBlog 当前后端主要采用横向分层结构：

```text
StarBlog.Api -> StarBlog.Application -> StarBlog.Data / StarBlog.Content / StarBlog.Infrastructure
```

这种结构把同一个业务能力拆散到 Controller、Service、ViewModel、Data Model 和 DI 注册等多个全局目录中。与此同时，旧 `StarBlog.Web` 与新 `StarBlog.Api` 仍保留大量重复能力，FreeSql 与 EF Core 并存也增加了查询、事务、测试和维护成本。

本次重构不承担历史兼容责任，明确接受以下前提：

- 不保留现有数据库数据。
- 不兼容生产环境中的旧数据库结构。
- 不保留现有 EF Core migration 历史。
- 不要求保持现有内部类型、命名空间和项目引用结构。
- API 契约可以重新设计，但需要同步修改当前前端调用方并为新契约建立测试。
- 最终代码中彻底移除 FreeSql，不保留双 ORM 运行模式。

因此，本次工作不是对现有分层架构的小幅整理，而是以全新后端为目标的模块化单体重建。

## 2. 架构目标

最终后端仅保留一个 ASP.NET Core 部署单元、一个主要业务程序集和一个 EF Core `StarBlogDbContext`。代码首先按用户可识别的业务能力组织，其次才按模块内部的技术职责组织。

目标结构：

```text
src/
  StarBlog.Api/
    Program.cs

    Hosting/                         # ASP.NET Core 宿主、OpenAPI、认证和中间件
      Authentication/
      HealthChecks/
      Middleware/
      OpenApi/

    Modules/
      Identity/                      # 用户、登录、密码、JWT、首次初始化
      Content/                       # 文章、分类、精选、Markdown、翻译、发布
      Comments/                      # 评论、匿名用户、OTP、审核和回复通知
      Media/                         # 照片、精选照片、缩略图和文件生命周期
      Links/                         # 友链、友链申请与审核
      Analytics/                     # 访问记录、IP/UA 解析和统计
      Site/                          # 首页聚合、搜索、Feed 和 Sitemap
      Configuration/                 # 运行时站点配置
      Notifications/                 # Outbox、通知任务和邮件模板

    Infrastructure/
      Persistence/                   # DbContext、EF Core 通用约定和 migrations
      Storage/                       # 本地或远程文件存储实现
      Email/                         # SMTP 适配器
      Background/                    # 有界任务队列与 HostedService
      Time/                          # 系统时钟实现

    Shared/                          # 仅放置已经被多个模块实际复用的少量契约

tests/
  StarBlog.Api.Tests/
```

模块内部建议采用以下结构；简单模块可以省略不需要的文件或目录：

```text
Modules/Links/
  LinksModule.cs                     # 模块服务注册与 endpoint 映射
  LinkEndpoints.cs                   # HTTP 边界
  LinkContracts.cs                   # 请求与响应契约
  LinkOperations.cs                  # 审核、事务等可复用业务规则
  Domain/
    Link.cs
    LinkExchange.cs
  Persistence/
    LinkConfiguration.cs
    LinkExchangeConfiguration.cs
```

## 3. 项目精简决策

### 3.1 删除 `StarBlog.Application`

不再保留全局 Application Layer 项目。

- `Services` 移入所属业务模块，按实际职责命名为 `Operations`、`Queries` 或具体用例。
- `ViewModels` 和 `Criteria` 移入模块的 HTTP 契约或查询旁边。
- 后台任务移入所属业务模块或 `Infrastructure/Background`。
- 不再创建全局 `Services`、`ViewModels`、`DTOs`、`Criteria` 目录。
- 不为每个具体实现机械创建接口。

### 3.2 删除 `StarBlog.Data`

不再保留以 Data 为中心的独立项目。

- 实体移动到所属模块的 `Domain` 目录。
- EF Core 映射移动到所属模块的 `Persistence` 目录。
- `StarBlogDbContext` 放在 `Infrastructure/Persistence`。
- 删除 `FreeSqlFactory`、`ConfigureFreeSql` 和 FreeSql Repository 注册。
- 删除所有 `IBaseRepository<T>` 使用。
- 删除现有 migrations 和 ModelSnapshot。

### 3.3 删除 `StarBlog.Content`

当前 `StarBlog.Content` 同时包含 Markdown、文章实体处理、SMTP、哈希、ID 和字符串工具，不是稳定且内聚的架构边界。按实际所有权拆分：

| 现有能力 | 新位置 |
| --- | --- |
| Markdown、ToC、Bootstrap 扩展 | `Modules/Content/Markdown` |
| `PostProcessor` 和文章导入 | `Modules/Content/Importing` |
| SMTP 与邮件发送 | `Infrastructure/Email` |
| 评论/友链通知模板 | `Modules/Notifications` 或具体业务模块 |
| 密码哈希 | `Modules/Identity/Security` |
| Guid/ID 工具 | 优先使用框架原生 `Guid`，无必要则删除 |
| 通用字符串扩展 | 靠近使用位置，避免形成全局 Helpers |

如果 `MarkdownImporter` 在重构后仍有独立运行价值，可以另外保留一个很小的纯 Markdown 类库。该类库不得依赖 EF Core、业务实体、邮件或 ASP.NET Core；这不是默认方案，只有真实复用需求才能证明其存在价值。

### 3.4 删除独立 `StarBlog.Infrastructure`

现有 IP 查询、CLR 统计等少量代码直接移动到 `StarBlog.Api/Infrastructure`。当前规模不足以证明独立基础设施程序集的收益。

### 3.5 删除旧 `StarBlog.Web`

最终只保留纯 API 后端。旧 Razor/MVC 宿主及其重复 Controller、Service、ViewModel、静态编排代码全部删除。前端应用独立存在，不影响后端作为一个模块化单体部署。

## 4. 模块边界与依赖规则

| 模块 | 拥有的数据与能力 | 允许的业务依赖 |
| --- | --- | --- |
| Identity | User、认证、密码、JWT、首次初始化 | Configuration |
| Content | Post、Category、Featured、Translation、Publication | Configuration、Notifications |
| Comments | Comment、AnonymousUser、OTP、审核、回复规则 | Content 的只读契约、Notifications |
| Media | Photo、FeaturedPhoto、缩略图和原图 | Storage |
| Links | Link、LinkExchange、审核规则 | Notifications |
| Analytics | VisitRecord、IP/UA 和统计查询 | 无业务写依赖 |
| Configuration | ConfigItem 和运行时配置 | 无 |
| Notifications | OutboxMessage、通知任务和邮件模板 | Email 基础设施 |
| Site | 首页、搜索、Feed、Sitemap 等聚合读模型 | Content、Media、Links、Configuration 的只读契约 |

必须遵守以下规则：

1. 模块不得直接修改另一个模块拥有的实体。
2. 跨模块调用通过小而明确的公开查询或命令契约完成，禁止引用另一个模块的内部 Operations。
3. `Site` 是允许扇出读取多个模块的组合模块，但不得成为业务写入规则的归宿。
4. `StarBlogDbContext` 是当前规模下允许共享的基础设施，不为形式上的隔离拆分多个 DbContext。
5. 接口只用于真实可替换边界，例如文件存储、邮件发送、时间、外部 LLM 和当前用户上下文。
6. 模块内部的单实现业务协作者默认使用具体类型。
7. 不引入 MediatR、通用 Repository、Unit of Work 包装或简单 CRUD 的 CQRS 仪式。
8. 优先使用 ASP.NET Core 与 EF Core 原生能力，再评估第三方框架。

## 5. EF Core 重建策略

### 5.1 单一持久化模型

最终只保留一个 `StarBlogDbContext`，统一管理：

- 用户与认证数据。
- 文章、分类、精选、翻译和发布记录。
- 评论和匿名用户。
- 图片和精选图片。
- 友链与友链申请。
- 站点配置。
- 访问记录。
- Outbox 消息。

访问记录暂不单独拆库。只有出现经过测量的写入压力、保留周期或运维隔离需求后，才重新评估独立数据库。

### 5.2 映射方式

实体配置使用模块内的 `IEntityTypeConfiguration<T>`，`StarBlogDbContext` 只负责统一发现配置：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    // 自动发现各业务模块中的实体配置，避免 DbContext 演变为全局映射文件。
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(StarBlogDbContext).Assembly);
}
```

### 5.3 Migration 基线

本次重构执行以下数据库历史重置：

1. 删除当前所有 EF Core migration 文件和 ModelSnapshot。
2. 删除 FreeSql 的结构同步逻辑，不使用 `SyncStructure`。
3. 完成全部目标实体、索引、约束和关系设计。
4. 在目标数据库 Provider 确定后生成唯一的新 `InitialCreate`。
5. 从新的 `InitialCreate` 开始正常维护后续 migrations。

不使用 `EnsureCreated` 替代正式 migration。集成测试可以针对临时 SQLite 数据库使用独立的测试初始化机制，但生产启动流程必须使用 migration。

默认数据库 Provider 采用 SQLite，以保持单机部署简单。若目标改为 PostgreSQL，必须在生成新 `InitialCreate` 前确定，避免刚重建就产生 Provider 切换迁移。

### 5.4 Repository 决策

EF Core `DbContext` 已提供当前项目需要的 Unit of Work 和 Repository 能力：

```text
简单用例：Endpoint -> StarBlogDbContext
复杂或复用用例：Endpoint -> Concrete Operations -> StarBlogDbContext
```

不再创建：

```text
IPostRepository -> PostRepository
ILinkService -> LinkService
IUnitOfWork -> UnitOfWork
```

包含业务不变量、多个入口复用、事务或外部副作用的功能应保留具体 Operations。例如友链审核、评论回复通知、文章发布和文件生命周期不应全部复制进 endpoint。

## 6. HTTP 与契约策略

本次重构允许重新设计 API，不继承旧路由和返回包装作为硬性约束。

- 优先使用标准 HTTP 状态码和 `ProblemDetails`。
- 使用明确的请求与响应契约，不直接返回 EF Core 实体。
- 取消全局 `ApiResponse<T>` 包装，除非前端存在经过确认的统一包装需求。
- 分页使用项目内简单的 `PageResult<T>`，不默认保留 X.PagedList。
- DTO 映射优先显式实现，不默认保留 AutoMapper。
- Endpoint 可以使用 Minimal API Route Group；MVC Controller 同样允许，但同一模块应保持一致风格。
- 无论使用哪种 HTTP 技术，endpoint 只负责协议、验证、授权和响应映射。

`Program.cs` 最终只承担组合根职责：

```text
AddWebFramework()
AddInfrastructure()
AddIdentityModule()
AddContentModule()
...

UseWebFramework()
MapIdentityModule()
MapContentModule()
...
```

## 7. 依赖清理范围

最终必须移除：

- `FreeSql`
- `FreeSql.Repository`
- `FreeSql.Provider.Sqlite`
- `FreeSql.Provider.MySql`
- `FreeSql.Provider.PostgreSQL`
- 所有 FreeSql 扩展、Factory 和 `IBaseRepository<T>`

重构期间评估并优先移除：

- `AutoMapper.Extensions.Microsoft.DependencyInjection`
- `X.PagedList` 及其 EF/MVC 扩展
- `Microsoft.EntityFrameworkCore.DynamicLinq`
- 仅用于旧响应包装或旧工具的依赖
- 已无入口的 demo、烟雾测试项目和旧 CLI 工具

继续保留的依赖必须有当前源代码中的实际用途，例如 EF Core、SQLite Provider、Markdig、ImageSharp、MailKit 和认证相关组件。

## 8. 实施阶段

### Phase 0：建立重构基线

1. 在独立重构分支工作。
2. 固化需要保留的产品能力清单，不固化旧内部实现。
3. 确认默认数据库 Provider。
4. 为认证、文章、评论、友链、图片、初始化和 Outbox 等关键行为建立特征测试。
5. 建立只覆盖新后端目标的可重复 build/test 命令。

### Phase 1：建立新骨架

1. 创建 `Hosting`、`Modules`、`Infrastructure` 和 `Shared` 目录。
2. 建立模块注册约定：`Add<Module>()` 与 `Map<Module>()`。
3. 建立 `StarBlogDbContext` 和测试数据库工厂。
4. 增加架构测试，禁止新增全局 `Services`、`DTOs`、`ViewModels`、`Helpers` 和 `Repositories`。

### Phase 2：迁移基础能力

1. Identity 与密码安全。
2. Configuration。
3. Notifications、Outbox 与 Email。
4. Storage、Clock 和 Background Queue。

Outbox 与触发它的业务写入必须使用同一个 EF Core 事务，避免业务状态成功但通知任务丢失。

### Phase 3：验证模块模板

首先迁移 Links：

- 公开友链查询。
- 管理端 CRUD。
- 友链申请。
- 审核通过或拒绝。
- 审核后 Link 状态变化。
- Outbox 邮件去重。

Links 规模适中，能够同时验证 CRUD、业务规则、事务、匿名/管理端权限和外部通知边界。

### Phase 4：迁移主要业务模块

推荐顺序：

1. Media。
2. Content 的 Post、Category 和 Featured。
3. Comments。
4. Content 的 Translation 与 Publication。
5. Analytics。
6. Site 聚合查询、搜索、Feed 和 Sitemap。

每迁移一个模块，应同时完成 endpoint、契约、Operations、实体、EF 配置和模块测试，不保留新旧实现长期并行。

### Phase 5：生成新数据库基线

1. 审查实体字段、必填约束、唯一索引、级联删除和枚举存储。
2. 删除重构过程中产生的临时数据库。
3. 确认不再有 FreeSql 数据访问路径。
4. 生成新的 `InitialCreate`。
5. 从空数据库应用 migration 并运行完整集成测试。

### Phase 6：删除旧架构

1. 删除 `StarBlog.Application`。
2. 删除 `StarBlog.Data`。
3. 删除 `StarBlog.Content`。
4. 删除独立 `StarBlog.Infrastructure`。
5. 删除 `StarBlog.Web`。
6. 删除旧 migrations、重复测试、无效 demo 和无用工具。
7. 清理 solution、项目引用、Docker、部署脚本和文档。
8. 同步更新前端 API 调用。

实施分支可以为了保持可构建而短暂同时包含 FreeSql 与 EF Core，但这只是迁移机制；最终提交和任何可部署版本不得保留 FreeSql 运行路径。

## 9. 风险与控制措施

### 9.1 大规模行为回归

不需要兼容旧数据库不等于不需要保护产品行为。使用模块级特征测试记录登录、发布、审核、文件处理和通知等关键规则。

### 9.2 API 与前端同时变化

所有契约变更必须同步更新 `apps/admin` 和其他调用方。API 端到端测试以新契约为准，不保留无调用方的兼容路由。

### 9.3 文件与数据库不具备天然原子性

图片上传、文章导入等操作涉及数据库与文件系统。必须设计失败补偿、临时文件和清理策略，不能假设 EF Core 事务能够回滚文件写入。

### 9.4 后台任务使用 DbContext 生命周期

HostedService 不得持有 Scoped DbContext。每次任务处理通过 `IServiceScopeFactory` 创建独立 scope，并正确响应取消信号。

### 9.5 模块边界退化

单项目不能提供模块间的编译期隔离，因此必须使用命名空间、`internal` 可见性、代码审查和架构测试共同约束。只有实际出现边界失控时，才考虑把个别模块拆为独立程序集。

## 10. 明确的非目标

本次重构不做以下事情：

- 不拆微服务。
- 不引入分布式消息总线。
- 不为每个模块建立独立数据库或 DbContext。
- 不建立完整 DDD 战术模式体系。
- 不为普通 CRUD 引入 MediatR 或 CQRS。
- 不维护旧数据库升级路径。
- 不保留旧 API 作为永久兼容层。
- 不为了“未来可能复用”预先创建共享抽象或独立类库。

## 11. 完成标准

满足以下条件后，本次重构才算完成：

- 后端只有一个可部署 ASP.NET Core 项目。
- `StarBlog.Web`、`StarBlog.Application`、`StarBlog.Data`、`StarBlog.Content` 和独立 `StarBlog.Infrastructure` 已移除。
- 源代码和项目文件中不存在 FreeSql、`IBaseRepository<T>` 或 `SyncStructure` 运行路径。
- 所有持久化统一使用 `StarBlogDbContext`。
- 数据库从空状态应用新的 `InitialCreate` 后可以启动并完成初始化。
- `Program.cs` 只负责框架、基础设施和业务模块组合。
- Endpoint、契约、规则、实体和映射能够从所属模块目录直接发现。
- 不存在全局业务 `Services`、`DTOs`、`ViewModels`、`Helpers` 或 `Repositories` 堆放目录。
- API 不直接暴露 EF Core 实体。
- 所有跨模块依赖符合本方案中的依赖规则，不存在循环依赖。
- 管理端和其他保留的调用方已经切换到新 API。
- build、单元测试、集成测试和关键端到端流程全部通过。
- 编译器警告和已知依赖漏洞已审查；无法同时处理的项目必须形成独立、明确的后续变更记录。

## 12. 第一项实施任务

正式实施从以下工作开始：

1. 建立新架构目录和模块注册约定。
2. 建立 `StarBlogDbContext` 与测试数据库基线，但暂不生成最终 migration。
3. 为 Links 模块补齐特征测试。
4. 将 Links 完整迁移为第一个 EF Core 垂直模块。
5. 通过 Links 模块复盘目录、命名、事务和测试约定，再推广到其他模块。

架构目标可以彻底，执行过程仍必须保持小步、可验证、可回退。每个阶段只迁移一个明确边界，避免在缺少行为保护的情况下进行不可审查的大规模机械搬运。
