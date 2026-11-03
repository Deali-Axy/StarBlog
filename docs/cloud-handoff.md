# StarBlog 部署交接（2026-11-03）

更新：用户电脑已充电，已取消云端接续，恢复本地开发。此文件保留为历史交接记录；最新准备状态见 `production-rollout.md`。

最终更新：2026-11-03 已在本地通过 ship 完成上线，见 [实际部署记录](deployment-2026-11-03.md)。下文的缺失脚本、许可阻塞和 SSH 故障均为历史状态。

用户已授权接受停机，停止旧 StarBlog 并替换为新版；必须使用 `~/code/ship` 工具。用户随后要求电脑断电后改由 cloud agent 接续。

## 当前准确状态

- **旧站未停止，尚未运行 ship 发布、上传发布包、创建新版容器或更改 SWAG。**
- 本机调用 `list_cloud_environments` 返回空数组，没有可运行的已保存云端环境，尚未创建云端任务。不能宣称已交接。
- 已生成本地私有交接包 `backups/cloud-handoff-2026-11-03/starblog-cloud-handoff.tar.gz`（约 12 MB），包含工作区改动、最新本地数据库、精简译文、ship 二进制和技能说明，不含 SSH 私钥。尝试上传服务器时 SSH/SCP 均返回 `Connection closed`，重试仍失败，**没有确认服务器收到交接包**；不能把服务器 handoff 目录视为可用来源。
- 当前工作区 `/home/deali/code/starblog/starblog`，分支 `codex/monorepo-foundation`；未提交的改动需要随交接包恢复，不能只检出远程分支。
- `docs/production-rollout.md` 记录完整线上检查、迁移策略和回退边界。用户接受停机后，资源不足不再作为拒绝部署的理由。
- 第一阶段迁移工具验证完成：线上 759 篇文章（681 发布、78 草稿）、93 分类、35 照片、19 评论；本地 275 篇英文译文全部合并且逐行一致。本地原有开发库保留。
- **正式发布脚本还未写完。** 新 `ship.toml`、`deploy/build-release.py` 引用了尚不存在的 `deploy/install-release.py`。不得直接运行 ship deploy/run；先完成、审查和测试安装脚本。

## 必须保留的数据

- 旧目录 `/home/deali/projects/starblog`，访问日志 `app.log.db` 及 WAL/SHM 原样留存，后续分析，不导入新版。
- 媒体在旧目录 `wwwroot/media`，2,515 文件，约 292M。直接在服务器复制到新媒体目录，不下载到本地，不使用硬链接，不让新旧应用共写。
- 本地新版库有 275 英文译文。按 post_id + language 合并，保留译文 ID、正文和时间；冲突或缺失原文必须停止导入，不能丢弃或重译。
- 文章 `514395100768e83a`《在 Windows 10 上实现免密码 SSH 登录》的线上原文与本地版本正文有差异，译文已保留并提示复核；其他 274 篇原文标题/正文一致。
- 本地管理员用户名为 `admin`，新版 EF 用户表使用 PBKDF2。已开始设计保留本地管理员哈希的流程，避免公开空库初始化入口；不复制旧版管理员哈希，不输出密码/哈希。

## 线上拓扑

- SSH 别名 `deali.cn`；本机通过 `ssh -F /home/deali/.ssh/config deali.cn` 访问。默认 ssh 会报 `/etc/ssh/ssh_config.d/20-systemd-ssh-proxy.conf` 权限错误，显式 `-F` 可绕开。
- 云端需独立配置经授权的 SSH/网络访问。**本次不把本机 SSH 私钥放进交接包。**
- 旧容器 `starblog`，主机 9874 → 容器 80，镜像 ID `sha256:30f3bc494546f420509131243d159ca6fa853128cad42f2d42f22f6c381ab95f`。
- SWAG 容器 `swag`，网络 `swag`，子网 172.18.0.0/16；配置 `/config/nginx/proxy-confs/blog.subdomain.conf`，目前 upstream `starblog:80`。
- SWAG 宿主卷 `/home/deali/apps-docker/swag/config`。只改 StarBlog 的站点配置，保留其余服务。
- 服务器 2 核 / 2GB，Python 3.11.2、Docker、sqlite3 可用，没有 rsync 或宿主 dotnet。末次磁盘剩 3.1G；用户接受停机。不要为了容量删除访问日志或旧备份。

## 已写入但尚待完整验证的部署改动

1. `ship.toml` 改为 command build → SCP → SSH，版本来源 env，以包含当前未提交的部署修复；原 HEAD 中是旧版 Docker/registry/compose 配置。ship 也支持该驱动组合。任何可能提示确认的 ship 命令必须加 `-y`。
2. `deploy/build-release.py` 本地 publish API、构建 Next standalone 和 `/admin/` 静态后台，打 runtime-only tar.gz；`deploy/prepare-migration.py` 一致性备份本地库、导出精简译文和管理员哈希后上传。两个脚本尚未完整执行。
3. `deploy/compose.server.yaml` 草案：新容器 api/web/gateway，独立 `starblog-v2` 项目；私有子网草案 172.30.84.0/24，gateway .2/API .3/web .4，须部署前确认不冲突。只有 gateway 加入 swag，主机验证端口 127.0.0.1:15080。
4. 管理端改为支持 Vite BASE_URL / BrowserRouter basename，生产 `/admin/`，避免新增 DNS/证书。默认本地根路径仍保留。
5. 前台 Markdown 增加相对图片 `/media/blog/{postId}/...` 渲染映射，原文、译文共享同一个原文 ID，不改数据库正文。旧 `.assets` 目录和反斜杠仍需结合文件清单核对；Feed 的相对图片兼容尚未实现。
6. Next 增加 `/health/live`。gateway 草案包含 `/Blog/Post`、`/Blog/List`、旧 RSS 等 301 跳转，待 Nginx 与真实请求验收。
7. API 增加显式 KnownProxies 配置，草案只信任私有 gateway；须核对 SWAG → gateway 真实 IP 和 scheme 转发。
8. API ImageSharp 升到 3.2.0，显式 Pkcs 10.0.13；dotnet restore 成功，尚未完成新版本 build/test/图片解码验证。
9. csproj 排除本地媒体和私密配置的 publish；根 .dockerignore 排除数据库、媒体、backups、.ship、.env 等。构建结束仍必须检查产物不含真实数据或密钥。

## 已运行的检查

- `python3 -m unittest discover -s tests -p test_legacy_import.py`：12 项通过（两源合并、冲突拒绝、媒体失败回滚等）。
- `node tests/article-reading.test.mjs`（apps/web）：6 项通过，含新增相对图片渲染测试。
- `git diff --check` 通过。
- `pnpm typecheck` 尚未运行成功：本机 pnpm 引擎管理器对外部缓存写入被 sandbox 拒绝，不是 TypeScript 检查结果。可在合适环境使用现有 `node_modules/typescript/bin/tsc` 直接检查，或修复包管理器缓存路径。
- 第一阶段旧依赖 API 构建和独立 API 真实数据/英文端点验证通过；不能当成后来依赖升级和部署修复的最终验收。

## 安装脚本应实现的顺序

1. 在 `/home/deali/projects/starblog-v2` 准备独立 release、data、media、config、cache、backups、migration；目录与秘密文件权限明确限制。
2. 拉取并记录运行时镜像摘要（.NET 10、Node 24 Debian、已有 nginx），先确保文件、镜像和迁移输入齐备，再停旧站。
3. 保存旧 SWAG 配置、旧镜像 ID、容器状态；启用维护页并 stop 旧 `starblog`。旧数据、媒体、日志均不删除。
4. 新 API 私有启动建立 EF schema，随后停止，不能让业务进程在导入时写目标库。
5. 从停止后的旧业务库取最终一致性快照；运行 Import-LegacyBlog 的 dry-run 后正式导入，使用明确 source/target 媒体目录、精简译文副本。**正式迁移不能带 database-only。**
6. 在新库保留经核验的本地新版管理员，设置正式 host、is_init 等配置；复制旧邮件和词库到受保护运行目录，不输出其内容；生成新 JWT 密钥。
7. 启动新版服务，只通过回环验证健康、前台/后台、草稿保护、中英文、照片原件与缩略图、旧链接、Feed/Sitemap、转发头。
8. `nginx -t` 后将 SWAG 旧 upstream 切为新 gateway；reload 后验收公网，再解除维护。
9. 任一步失败，保留新版数据和错误报告，按是否开放写入的边界处理回退。不要在公网初始化空管理员，不要丢失新评论来强行回退。

## ship 本地工具

原工具 `/home/deali/code/ship/ship`（version `dev`）。技能 `/home/deali/code/ship/cmd/skills/SKILL.md`、REFERENCE.md、EXAMPLES.md 应随交接读取。可将工具放云端工作区自有目录运行，不必修改系统路径。

当前未跑 ship plan/doctor/build/run。云端需先确认其 command/scp 驱动和模板语法可接受。默认 ssh 配置权限问题可用仅作用于 ship 进程 PATH 的 ssh/scp 包装器加 `-F`，不要改系统 SSH 配置。没有部署安装脚本时禁止启动发布。
