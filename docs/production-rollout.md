# StarBlog 新版上线方案

盘点日期：2026-11-03（Asia/Hong_Kong）。当前结论：数据迁移路径已验证，暂不切流；先解决容量、部署资产和旧内容兼容问题。

### 恢复本地开发后的状态

用户已接受停机更新，并要求使用 `/home/deali/code/ship`；云端交接已取消，继续本地准备。尚未执行停机或发布。下一步优先完成以下事项：

1. 解决 ImageSharp 3.2.0 的正式构建许可要求：本次 Debug 构建通过，但 Release 构建已实际失败，错误为 `No Six Labors license found`。不能将这次依赖升级视为可发布状态。
2. 补齐 `deploy/install-release.py`，并让迁移输入准备支持失败重试；目前 ship 配置与构建脚本引用了不存在的安装脚本，不能执行正式发布。
3. 完成三个应用的正式构建、图片/旧链接/Feed 兼容验证，再执行 ship doctor 与产物检查。当前 Web TypeScript 检查和 ship plan 已通过；部署方案采用 command → SCP → SSH。
4. 恢复 SSH 连通性：最新只读检查仍返回 `Connection closed`。发布前重新确认线上状态、网络、目录权限和最终数据快照；资源余量不再作为拒绝停机部署的理由。

## 线上实况

通过 `ssh deali.cn` 检查 `/home/deali/projects/starblog`，未修改生产应用、反向代理或业务数据。

| 项目 | 实测 |
| --- | --- |
| 系统 | Debian，x86_64，2 核 |
| 内存 | 1.9 GiB 总量，约 328 MiB 可用；Swap 已用约 4.4 GiB |
| 系统盘 | 首次检查已用 99%、约 822 MiB 可用；结束复查已用 94%、约 3.1 GiB 可用 |
| 旧应用 | Docker 容器 `starblog`，镜像 `starblog`，宿主机 9874 → 容器 80 |
| 持久化 | 整个旧项目目录绑定到 `/app` |
| HTTPS 入口 | `swag` 容器，外部 Docker 网络 `swag`（172.18.0.0/16） |
| 现有路由 | `/config/nginx/proxy-confs/blog.subdomain.conf`，`blog.*` → `starblog:80` |
| 代理配置目录 | 宿主机 `/home/deali/apps-docker/swag/config` |
| Nginx 配置检查 | 通过；已有多处 http2 等弃用警告，未改动 |
| 业务库 | `app.db`，6,066,176 字节，SQLite quick_check 通过 |
| 访问日志库 | `app.log.db` 约 3.22 GiB，另有 WAL/SHM；保留旧处，后续独立分析 |
| 媒体 | 2,515 个文件，298,418,400 字节（约 284.6 MiB；du 约 292M），无符号链接 |
| 照片 | 35 条，35 个原始文件全部存在 |
| 旧备份 | 3 个压缩包，共约 747 MiB，最近文件日期为 2026-05-10；未做恢复验证 |
| 服务器迁移工具 | Python 3.11.2、sqlite3；未发现 rsync 或宿主机 dotnet |

Docker 报告镜像可回收约 6.36 GB、构建缓存约 2.26 GB。这只是容量线索，不代表可以直接删除，也不要将不同口径简单相加。未运行 prune，未删除旧备份或日志。

检查期间磁盘可用量发生变化，本次没有执行清理，变化原因未确认；容量准备应以正式部署前复查为准。结束时旧容器仍为运行状态、启动时间未变，旧站 HTTP 返回 200。

## 两个数据源的合并结果

旧站业务数据与本地新版译文需要共同保留。旧 FreeSql 库不能直接挂给新版 EF Core API，也不能靠应用启动自动升级它；必须先建立新版空库，再按字段导入。

| 内容 | 线上 | 本地原有新版库 | 合并演练库 |
| --- | ---: | ---: | ---: |
| 文章 | 759（发布 681、草稿 78） | 709 | 759 |
| 分类 | 93 | 87 | 93 |
| 照片 | 35 | 35 | 35 |
| 英文译文 | 无译文表 | 275 | 275 |
| 评论 | 19 | — | 19 |
| 匿名用户 | 11 | — | 11 |
| 友情链接 / 申请 | 8 / 3 | — | 8 / 3 |

本地 709 篇文章 ID 全部存在于线上，线上额外有 50 篇。275 篇译文全部找到对应原文；274 篇对应的原文标题、正文完全一致，1 篇正文有差异：

- 文章 `514395100768e83a`：《在 Windows 10 上实现免密码 SSH 登录》。本地正文 1,939 字符，线上 2,058 字符。合并时使用线上原文、完整保留现有英文译文，发布前人工复核差异。

合并工具不自动选择或覆盖冲突译文；译文的 ID、post_id、language、title、summary、content、creation_time、last_update_time 均原样保留。若目标已有不同译文，或本地译文对应的文章不在线上，预检失败，整次导入退出。

不导入旧管理员密码、旧访问记录或旧待发邮件。新管理员在隔离环境中初始化；站点地址、邮件、翻译服务配置分别核对。当前开发库的其他配置和管理员保存在完整副本中，本次没有自动合并到演练库。

### 本地已生成的文件

所有真实数据和报告均在 Git 忽略的 `backups/production-2026-11-03/` 下：

- `source/app.db`：使用 SQLite Backup API 通过 SSH 流式取得的线上一致性快照。SHA-256：`08ae5908bac2c85a1b30ff5dba21e1ec8dc464030a7e730f08da6d56e5c40ecc`。
- `local-before-merge.db`：本地原有新版数据库的一致性完整备份，含原有管理员与配置，按敏感数据保管。
- `translation-source.db`：只含译文及对应原文、迁移版本信息的精简副本，约 4.36 MB，不含管理员、邮件配置；适合上传服务器参与合并。
- `merged/app.db`：已合并的新版演练库，含 759 篇文章和全部 275 篇译文。未覆盖 `apps/api/src/StarBlog.Api/app.db`。
- `merged/validation.json`、`merged/api-validation.json`：合并与英文 API 验证报告。
- `server-combined-validation.json`：服务器端完整媒体预检、两源数据合并和数据库校验报告。
- `article-media-audit.json`：历史 Markdown 图片引用初步扫描，需结合解析器继续确认。

### 已验证的边界

- 本地合并库 quick_check 为 `ok`，foreign_key_check 为空，275 条译文与本地备份逐行一致。
- 独立 API 实例验证了健康检查、文章/分类/照片列表、已发布文章、草稿匿名访问 404、Feed、Sitemap；合并后英文文章详情与译文列表返回 200。
- 在服务器临时目录流式上传精简译文副本与空库，完成“线上内容 + 本地译文 + 现有媒体”组合演练，校验通过，临时目录已自动移除。
- 对独立目标媒体目录的预检确认需要复制 2,515 个文件；实际临时导入复用已有媒体，报告 2,515 个相同文件、0 次媒体写入。这证明数据库合并与服务器文件核对可行，尚未执行正式媒体复制或容器部署。
- 没有下载媒体或访问日志库，没有切换生产流量。

## 推荐部署结构

延续现有 Docker + SWAG，镜像在开发机或 CI 构建，服务器只拉取运行。保留旧容器名 `starblog`，新版使用不同名称和 Compose project，避免冲突。

| 入口 / 路径 | 目标 |
| --- | --- |
| `blog.deali.cn/` | 新 Next.js standalone 服务 `starblog-web-v2` |
| `blog.deali.cn/api/*`、`/media/*` | 新 API `starblog-api-v2:8080` |
| `/feed`、robots、各 Sitemap | 新 API |
| 建议的 `blog-admin.deali.cn/` | Vite 构建的静态后台，由 Nginx 提供，带 SPA fallback |
| 管理域名 `/api/*`、必要的 `/media/*` | 同一新 API，浏览器使用同源相对路径 |

管理域名尚未创建或验证 DNS/证书。独立域名适合当前 BrowserRouter 的根路径部署，避免直接将后台放 `/admin/` 后遇到资源和路由基址问题。API 不单独开放公网端口；预览用回环地址端口和 SSH 隧道。

建议服务器目录：

```text
/home/deali/projects/starblog-v2/
  compose.yaml
  .env                         # 镜像版本、生产密钥，权限 600
  data/app.db                  # 新 EF Core 数据库，持久挂载整个 data 目录（含 WAL/SHM）
  media/                       # 服务器内复制得到，挂载 /app/wwwroot/media
  migration/translations.db    # 精简译文快照，权限 600
  config/                      # 邮件、敏感词等文件，按需只读挂载
  backups/                     # 新库快照；另安排异机备份
```

媒体无需经本地中转。正式迁移让导入脚本直接从旧服务器目录复制到新目录，保留 `/media/blog/{id}/...` 与 `/media/photography/...` 的结构。预览可只读挂载旧媒体目录；正式运行使用独立副本，不让新旧应用共写媒体，也不使用硬链接。图片原件完整保留，ImageSharp 缓存可重建。

## 切流前必须补齐

1. **容量与运行预算。** 先审查不再使用的镜像、构建缓存或扩容，保留旧镜像用于回退。建议预留至少 5 GiB 磁盘，并实测新版 API + Next 的峰值内存；当前可用约 328 MiB 不足以放心并行运行。更稳妥的容量选择是至少 4 GiB 内存，或将前台放到另一台机器。若维持现有规格，采用短维护窗口停止旧应用后再启动新版，并验证整机其他服务的余量。
2. **完整部署资产。** 现有 `deploy/compose.prod.yaml` 仅 API、且复用旧容器名 `starblog`，没有媒体卷、前台或后台，不能直接用于本次切换。需补齐两个前端的构建资产、独立项目名、持久化挂载、服务网络、资源与日志上限、就绪检查。SWAG 只连接必要的入口服务；后端网络用于内部调用。
3. **镜像构建上下文。** 当前 API Dockerfile 使用 `COPY . .`，根 `.dockerignore` 尚未排除真实数据库、媒体、backups 和部分私密配置。构建前补齐排除规则，并确认发布镜像不含开发库、译文快照、邮件/翻译密钥或批量媒体。
4. **旧正文图片兼容。** 旧版 `PostService.MdImageLinkConvert` 会给相对图片加 `/media/blog/{postId}/` 前缀；新版 API 原样返回 Markdown，前台也直接使用图片 src。目前迁入的多数图片仍是相对文件名。需在渲染层兼容，并同时覆盖原文和译文，避免改写数据库中的译文内容。另有历史 `.assets`、Windows 反斜杠和外站图片；初步扫描有 102 处未按简单规则匹配，包含可能的代码示例，不能直接等同于 102 张图片缺失，需用 Markdown AST 与文件清单复核。
5. **旧链接与 SEO。** 明确实现 `/Blog/Post/{id}` → `/blog/{id}`、`/Blog/List` → `/blog`（保留分类/分页参数）、`/Rss/Feed` → `/feed` 的永久跳转；盘点其他旧入口。新版 `/p/{slug}` 已有跳转，但仍需验收 slug、查询参数、语言参数、canonical 和 Sitemap 的最终地址。
6. **生产配置。** 覆盖仓库内默认 JWT 密钥；先隔离初始化管理员再开放公网；将 `host` 设为 `https://blog.deali.cn`。邮件配置、翻译服务、敏感词文件分别迁移/核验。管理端保持同源 API，若改成跨域则补准确 CORS origin。
7. **代理链。** 当前 API 仅启用转发头类型，未配置 Docker 上游信任范围。配置准确的代理 IP/专用网络与跳数，验证 HTTPS scheme 和真实访问 IP，不能直接信任全网转发头。SWAG → API 与 Next SSR → API 的链路分别验收。
8. **构建时环境与依赖。** Next 的 rewrite 目的地址与公开环境变量在构建时确定，需与运行时 `INTERNAL_API_BASE_URL` 一致；后台 `VITE_API_URL` 同样是构建参数。本次 API 构建成功，但 NuGet 报告 ImageSharp 3.1.12、System.Security.Cryptography.Pkcs 10.0.0 的已知漏洞警告，上线构建前升级并验证。

## 执行顺序

### 1. 准备与预览

- 完成上述修复；固定 API、Web、Admin 同一发布版本，记录镜像摘要和 Git commit，在 CI/本地构建。
- 留存旧镜像的实际 image ID/digest，导出当前 SWAG 站点配置。容量处理后再拉新镜像。
- 上传迁移脚本和最新的精简译文快照。开发翻译任务也需在最终快照前完成或暂停，避免遗漏本次盘点后的新增译文。
- 建立隔离新版环境，首次启动生成当前版本 EF schema，并在仅本机可访问的环境初始化管理员；随后停新 API，确保导入期间没有业务进程写目标库。
- 使用线上内容快照、译文副本进行完整预检；检查新旧目录权限。预览只读复用旧媒体，或在空间足够时直接复制独立媒体目录。
- 通过 SSH 隧道验证首页、分类、搜索、摄影、图片原件/缩略图、中英文切换、后台与上传；预览禁止公开评论和其他写入，避免最终导入时产生分叉。

### 2. 最终数据迁移（服务器内）

安排短维护窗口，暂停旧站后台发布、评论、友链申请及所有外部 Publisher 写入。最明确的做法是让 SWAG 展示维护页并停止旧应用；旧日志库及其 WAL/SHM 全部保留原处。

再次取得最终业务快照，确认新目标库仍是空内容库（允许已有管理员和初始化配置）。以下为服务器上的模板命令，`--source` 应指向最终一致性快照；本次未执行正式写入：

```sh
python3 /home/deali/projects/starblog-v2/migration/Import-LegacyBlog.py \
  --source /home/deali/projects/starblog-v2/migration/legacy-final.db \
  --target /home/deali/projects/starblog-v2/data/app.db \
  --source-media /home/deali/projects/starblog/wwwroot/media \
  --target-media /home/deali/projects/starblog-v2/media \
  --translations-from /home/deali/projects/starblog-v2/migration/translations.db \
  --backup-dir /home/deali/projects/starblog-v2/backups \
  --dry-run
```

预检通过后，移除 `--dry-run` 执行。正式迁移不能带 `--database-only`。脚本会备份目标库、复制媒体并合并译文；同名不同内容文件或译文冲突会拒绝覆盖。当前工具仅支持空内容库，不支持给已经导入的预览库做增量同步：最终迁移需另建空目标库、保留预览副本后执行。

### 3. 验收与切流

- 数据量以最终快照重新核对，不能永久固定本次 759/275 的计数；核对草稿、分类关系、评论、照片、译文逐行一致、外键与 quick_check。
- 单独复核 `514395100768e83a` 的英文译文与最新原文；保留旧译文备份。
- 新 API ready、Web 页面、中英文详情、全部 35 张摄影原件及代表性文章图片通过后，才更换 SWAG upstream；先 `nginx -t`，再 reload。
- 验证外网 HTTPS、旧链接永久跳转、Feed/Sitemap、真实访问 IP、后台登录与上传；确认后解除维护，恢复写入。清理/重建 Next 数据缓存，避免预览阶段内容残留。
- 观察 5xx、内存、Swap、磁盘增长、SQLite 锁等待和上传错误。旧站与日志库先保留，暂不删除旧镜像、旧备份或旧媒体。

### 4. 回退与后续

开放新版写入前失败：恢复 SWAG 原配置、启动旧容器即可，新库与旧库独立。开放写入后不能承诺无损一键回退；应先冻结写入、备份新库和媒体，保全新评论/文章/申请，再决定继续修复还是合并增量后回退，不能直接丢弃新数据。

新版上线后设置业务库与媒体的定期异机备份，并实际做一次恢复验证。新版访问记录与业务表共用数据库，还需设置保留期/归档，避免复现旧日志库持续增长；健康探测访问也需纳入日志预算。旧 `app.log.db` 单独保留用于后续分析，不接入新版运行库，也不在此次迁移中 VACUUM、截断或删除。

## 参考

- [SQLite Online Backup API](https://www.sqlite.org/backup.html)：一致性快照与在线备份。
- [Next.js 自托管](https://nextjs.org/docs/app/guides/self-hosting)：反向代理、构建/运行环境变量与缓存。
- [Docker bind mounts](https://docs.docker.com/engine/storage/bind-mounts/)：持久化目录与只读预览挂载。
- [ASP.NET Core 代理配置](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)：转发头与可信代理。
