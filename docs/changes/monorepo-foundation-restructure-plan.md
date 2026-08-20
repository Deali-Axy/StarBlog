# StarBlog Monorepo 基础结构调整方案

日期：2026-08-20
状态：已完成

## 1. 目标

在模块化单体与 EF Core 全量重建之前，先把 StarBlog 整理为多语言 monorepo，使 .NET、TypeScript、运维工具、部署资产和文档都拥有稳定、可解释的仓库位置。

本次调整只改变仓库结构与编排方式，不开始业务模块重构，不迁移 FreeSql，不重建数据库，也不执行任何测试。测试项目的归并、迁移和运行放到下一批工作。

## 2. 顶层目录语义

| 目录 | 职责 |
| --- | --- |
| `apps/` | 可独立运行或部署的产品应用，不区分实现语言 |
| `tools/` | 数据处理、导入、备份等开发或运维工具 |
| `tests/` | 当前测试项目；下一批再向应用内归并，仅长期保留跨应用 E2E |
| `demo/` | 当前实验代码；后续删除、转测试或迁入 `playground/` |
| `infra/` | 将来的本地网关、缓存、可观测性等运行基础设施，按需创建 |
| `deploy/` | 生产部署与容器编排配置 |
| `scripts/` | 轻量、可审查的仓库自动化脚本 |
| `docs/` | 架构、变更与使用文档 |

`backups/`、`dist/`、`TestResults/`、`temp/` 和 `output/` 是本地产物，不属于仓库架构，应保持未跟踪。

## 3. 本批次落地结构

```text
StarBlog/
  apps/
    api/
      src/
        StarBlog.Api/
        StarBlog.Application/       # 过渡项目，模块化重构时删除
        StarBlog.Content/           # 过渡项目，模块化重构时删除
        StarBlog.Data/              # 过渡项目，模块化重构时删除
        StarBlog.Infrastructure/    # 过渡项目，模块化重构时删除
      Taskfile.yml

    admin/
      src/
      package.json
      Taskfile.yml

    web-legacy/
      StarBlog.Web.csproj
      ...                           # 旧 Razor/MVC 宿主，后续删除

  tools/
    BlogImageOptimizer/
    DataProc/
    MarkdownImporter/
    StarBlogBackup/
    Taskfile.yml

  demo/                             # 本批次不整理、不加入默认构建
  tests/                            # 本批次不移动、不运行
  deploy/
  docs/
  scripts/

  Taskfile.yml
  StarBlog.slnx
  global.json
  Directory.Build.props
```

顶层 `src/` 在本批次后消失。当前四个横向类库暂时位于 `apps/api/src`，只是为了在大规模重构前维持现有引用关系；它们不是新的长期分层，也不会被包装为正式 `packages`。

## 4. 最终目标结构

完成后续模块化单体重构后，API 工作区将收敛为：

```text
apps/api/
  src/StarBlog.Api/
    Modules/
    Infrastructure/
    Hosting/
    Shared/
    StarBlog.Api.csproj
  tests/StarBlog.Api.Tests/
  Taskfile.yml
```

`StarBlog.Application`、`StarBlog.Content`、`StarBlog.Data`、`StarBlog.Infrastructure` 和 `apps/web-legacy` 最终全部删除。只有出现被多个应用真实复用的纯代码时才创建 `packages/`，不为了表达技术层预先创建包。

## 5. 测试与 Demo 决策

本批次不执行任何测试，也不修改测试内容。

下一批计划：

- 将 API 单元测试、集成测试和测试基础设施归并到 `apps/api/tests`。
- 根 `tests/` 最终只保留跨 API、Admin/Web 的黑盒 E2E。
- `CommentReplyNotifySmokeTest` 和 `OutboxSmokeTest` 转为正式集成测试。
- `MarkdownParseTest` 转为 Content 模块单元测试或删除。
- `FileParseTest`、`GenerateRandomCoordinates`、`ParallelTest`、`RegTest` 删除或迁入不参与构建的 `playground/`。
- `demo/` 不加入根 `StarBlog.slnx`、默认 `task build` 或 CI。

## 6. 工具目录决策

`tools/` 是合法的 monorepo 顶层目录，不属于产品应用，因此不强行迁入 `apps/`。

当前工具暂时保持独立项目。等新的 EF Core 数据模型稳定后，再评估合并为单一 `tools/starblog-cli`：

```text
starblog import
starblog backup
starblog restore
starblog optimize-images
starblog enrich-visits
starblog generate-slugs
starblog generate-summaries
```

工具不得成为迫使主后端保留旧 Data/Application 分层的理由。长期应通过 HTTP 契约、文件契约或非常小的纯共享包复用能力。

## 7. 统一任务编排

仓库使用 Taskfile 作为多语言统一入口：

```text
Taskfile.yml
apps/api/Taskfile.yml
apps/admin/Taskfile.yml
tools/Taskfile.yml
```

根 Taskfile 只组合子项目任务：

```text
task install
task dev
task build
task format
task clean

task api:restore
task api:dev
task api:build
task api:format
task api:db:add -- MigrationName
task api:db:update

task admin:install
task admin:dev
task admin:build

task tools:build
```

测试任务在下一批补齐；本批次不定义会被根任务自动执行的测试依赖。

Taskfile 负责编排 .NET、npm 和工具项目；`.slnx`、MSBuild 与 NuGet 继续负责 .NET 内部构建。暂不引入 NUKE 或 .NET Aspire，避免在只有 API 与 Vite Admin 的阶段增加额外编排项目。

## 8. .NET 工作区

- 使用一个根 `StarBlog.slnx` 代替 `StarBlog.sln`、`StarBlog.Tools.sln` 和 `StarBlog.Demo.sln`。
- `StarBlog.slnx` 收录当前维护的 API、过渡类库、旧 Web、测试和工具项目，但不收录 demo。
- 使用根 `global.json` 固定 .NET 10 SDK 系列。
- 使用 `Directory.Build.props` 统一确定性构建、语言版本和 CI 属性。
- Central Package Management 延后到 FreeSql 清理批次，避免本次纯路径迁移同时改变所有 NuGet 依赖解析。

## 9. 路径调整范围

移动项目后必须同步更新：

- 所有 `.csproj` 的 `ProjectReference`。
- Dockerfile 的构建上下文和项目路径。
- `ship.toml`。
- GitHub Actions 工作流。
- 管理端 README 与环境变量注释。
- PowerShell 管理脚本。
- MarkdownImporter 和 BackupTool 的仓库定位及默认路径。
- 根 README 与现有架构文档中的有效路径。
- `.gitignore` 中旧 Web 的产物路径。

历史评审文档可以保留其当时语境，但链接必须继续能够定位移动后的文件，或明确标注为历史路径。

## 10. 本批次完成标准

- 顶层不再存在 `src/`。
- .NET 产品应用位于 `apps/`。
- `apps/api` 和 `apps/admin` 各自拥有 Taskfile。
- 根 Taskfile 可以列出并解析所有已定义任务。
- 根目录只保留一个 `.slnx` 作为 .NET 工作区入口。
- 所有项目引用都指向存在的文件。
- Docker、部署、脚本和主要文档不再引用旧 `src/StarBlog.*` 路径。
- `demo` 不参与默认构建。
- 本批次不运行测试；测试结果不作为本批次完成条件。
