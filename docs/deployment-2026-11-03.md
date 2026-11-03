# StarBlog 首次迁移上线记录

2026-11-03 23:52（UTC+8）已完成公网切换，ship 的 build、publish、deploy、verify 四阶段均成功。

| 项目 | 实际结果 |
| --- | --- |
| 前台 | https://blog.deali.cn/ |
| 后台 | https://blog.deali.cn/admin/ |
| 就绪检查 | https://blog.deali.cn/health/ready，200 / Healthy |
| 发布版本 | `first-rollout-20261103-3` |
| ship run ID | `710a8c28ef7136fa` |
| 源码基线 | `79d15ab1eec02953ba929c3c1d4cdda59a9150ac` 加本次工作区部署修复；尚未替用户创建 Git 提交 |
| 发布包 SHA-256 | `86195847072504e9212f9ec33c81eb4a71a783dec08586a9e5c6b168bd38899a` |
| 服务器目录 | `/home/deali/projects/starblog-v2` |
| 当前运行目录 | `/home/deali/projects/starblog-v2/releases/first-rollout-20261103-3` |
| 备份目录 | `/home/deali/projects/starblog-v2/backups/20261103T155038Z` |
| 原站目录 | `/home/deali/projects/starblog`，数据库、媒体和访问日志保留 |

## 实际部署方式

按用户要求，本次通过 `/home/deali/code/ship/ship` 执行 command → SCP → SSH → HTTP verify。

```sh
python3 deploy/ship-local.py run -v first-rollout-20261103-3 -y
```

`ship-local.py` 仅为该 ship 进程准备工作区内的 SSH/SCP 配置，保留严格主机密钥校验，避免本机系统 SSH 配置权限错误。不修改系统 SSH 配置。

API、Next.js standalone 与 `/admin/` 静态后台通过发布目录挂载在三个 Docker 容器中；运行时镜像已记录摘要：

- API：`mcr.microsoft.com/dotnet/aspnet@sha256:de0ee358cdd672f7017a003bed21e9ac9ed48a27c922a59b18175b6e82fa7fee`
- Web：`node@sha256:c86ecead9e55a5210ac1727de38a4ca840bf03dcaf34dc9749addbf383b1920c`
- Gateway：`nginx@sha256:0e52bd054126cdc040b1276c0af347aeb02a41b9cfe305d6094767ac30e97949`

这只是首次迁移的过渡方案。`install-release.py` 会拒绝用另一版本执行常规升级；稳定后的发布必须按 [todo.md](todo.md) 改成专用应用镜像 build/push registry，服务器 pull 后更新 Compose。不要继续扩展这套临时发布包流程来替代镜像部署。

## 数据与媒体

- 最终线上内容快照迁入 759 篇文章（681 已发布、78 草稿）、93 分类、35 照片、19 评论、11 匿名用户、8 友链和 3 申请。
- 合并本地 275 篇英文译文，逐行比对完全一致；旧站最新文章与本地译文共同保留。
- `514395100768e83a`《在 Windows 10 上实现免密码 SSH 登录》的原文有变化，英文译文完整保留，仍需人工复核。
- 2,515 个媒体文件全部在服务器内复制，并与旧媒体逐文件比较内容，未下载到本地；35 张摄影原件 HTTP 验证全部通过。
- 数据库 quick_check 为 `ok`、foreign_key_check 为空；旧访问记录未导入。旧 `app.log.db` 保留在原目录，未手动截断、压缩或删除；停止旧应用时 SQLite 正常关闭并收尾 WAL。
- 现有本地新版管理员被保留，后台沿用该账号密码；未公开空管理员初始化入口。新 JWT 密钥独立生成，邮件配置与词库独立挂载，不进入发布包。

## 验收与修复

- API Release 构建 0 警告、0 错误；EasyImageSharp 1.2.0 未出现原 ImageSharp 的许可阻塞。
- API 测试 79 项、迁移测试 12 项、首次发布包安全测试 4 项、文章渲染测试 6 项通过。
- 首页、文章列表、摄影、RSS 页面、后台入口与深层路由、后台 JS/CSS、Feed、robots、Sitemap 公网 HTTPS 返回 200。
- 英文文章详情、译文匹配、匿名草稿 404、35 张原件与代表性缩略图通过服务端部署验收。
- 旧 `/Blog/Post/{id}`、`/Blog/List`、`/Rss/Feed`、`/Rss` 与 `/admin` 跳转返回 301，保留查询参数，不泄漏内部 8080 端口，`/rss` 不循环跳转。
- 浏览器访问宿主不可用，未完成浏览器交互或截图验收；没有把 HTTP 验证表述为视觉验收。
- 首次尝试因只读 `/app` 内缺少嵌套挂载点失败，自动恢复旧站并验证 200；补齐空挂载点后使用新版本发布包重试成功。
- 旧容器 `starblog` 已停止，重启策略改为 `no`，避免服务器重启后与新版同时启动；旧镜像和旧目录均保留。

本地验收报告位于 Git 忽略的 `backups/production-releases/first-rollout-20261103-3/`；ship 状态位于 `.ship/runs/710a8c28ef7136fa/run.json`。

## 运行与回退边界

服务器日常只读检查：

```sh
cd /home/deali/projects/starblog-v2
docker compose --env-file .env -f compose.yaml ps
```

原 SWAG 配置、旧容器检查记录和最终旧业务快照已保存在上述备份目录，迁移报告为 `migration-report.json`。不要输出 `.env` 或 `legacy-container.json` 的内容到公开日志，它们可能含私密配置。

新版已开放公网写入，回退前必须冻结写入并保全新版数据库与媒体，核对新增文章、评论和申请；不能直接恢复旧站来丢弃增量。完成增量保全后，可停新版服务、恢复备份的 SWAG 站点配置、启动旧容器并执行 `nginx -t` / reload。旧容器原重启策略为 `always`，只有确定回退运行时才恢复该策略。

下一阶段依照 `docs/todo.md` 完成镜像发布流程与文章图片地址规范化，并安排定期异机备份和恢复验证。
