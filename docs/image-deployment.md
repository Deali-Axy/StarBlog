# StarBlog 镜像发布

首次迁移记录保留在 [2026-11-03 上线记录](deployment-2026-11-03.md)。首次迁移安装器、发布包构建与本地数据上传脚本已移除，后续更新不会重新导入旧站或本地数据库。

最新成功发布：2026-11-04 的 `v2026.11.04.1`，镜像摘要、备份与验收结果见 [发布记录](deployment-2026-11-04.md)。

## 发布

```sh
python3 deploy/ship-local.py plan -v v2026.11.04.1
python3 deploy/ship-local.py doctor -v v2026.11.04.1
python3 deploy/ship-local.py run -v v2026.11.04.1 -y
```

在提交代码且工作区干净后发布，每次选择新的版本。当前 `version.source=env` 使用该提交的工作目录，ship manifest 记录源码 commit；不要用旧版本名覆盖镜像。构建依赖本地已安装的 .NET、Node、前后台依赖以及 Docker。`deploy/build.env` 只含公开构建参数。

ship 通过 api、web、gateway 三个 profile 构建 `linux/amd64` 镜像并推送到 `ccr.ccs.tencentyun.com/deali/starblog`，分别使用 `<version>-api`、`<version>-web`、`<version>-gateway` 标签。不会自动发布 latest。基础镜像均固定摘要；本机已缓存且镜像加速器返回 429 时，可以按 ship 指南使用 `--pull=false`，不改变配置默认拉取策略。

构建上下文只包含发布后的程序、Next standalone 与后台静态资源。不会上传数据库、媒体、SSH 密钥或私密配置；API 镜像不含开发用 JWT key。

publish 后读取 `.ship/releases/<version>.json`，确认三个 registry 摘要完整，再仅上传 Compose、网关配置、部署脚本和摘要清单。服务器先 pull 全部镜像，然后停止 API，把完整 SQLite 目录备份到 `backups/images-<UTC时间>/data`，保留 WAL。随后更新三组容器并检查就绪状态、后台入口、默认首页方案及精选内容。

服务器工作目录仍为 `/home/deali/projects/starblog-v2`，保留原来的 `data`、`media`、`cache`、`config` 目录和 JWT key。Compose 不再挂载应用发布目录。站点入口和 SWAG 路由不变。

## 检查与回退

```sh
ssh deali.cn 'cd /home/deali/projects/starblog-v2 && docker compose --env-file .env -f compose.yaml ps'
```

当前镜像摘要、源码与对应备份记录在服务器 `active-release.json`；每次发布配方位于 `image-releases/<version>/`。`.env` 和备份中的私密配置不得输出到公开日志。

部署或验收失败时，脚本恢复上一份 Compose、环境变量和网关配置，重新创建前一组容器，继续使用当前持久化数据。首次镜像切换的备份仍可恢复为旧目录挂载模式，因此不要清理旧发布目录。

人工回退时先确认目标程序兼容当前数据库 schema，再恢复指定备份中的 `.env`、`compose.yaml`、`gateway.conf` 和 `active-release.json`，执行 `docker compose up -d --force-recreate --wait` 并检查站点。不能直接覆盖数据库快照，否则会丢失发布后的新增内容。跨 schema 回退需要维护窗口、完整增量备份与人工恢复方案。ship 的通用 rollback 暂未启用，因为这里需要同步切换三个镜像。

## 默认首页

新访客默认使用“点阵手记”（journal）：刊物侧栏和封面，点阵纹理、虚线边框、便笺文章、散落相纸、友链名片。原刊物与书桌方案仍可选择。已保存的偏好和分享链接继续优先于站点默认值。
