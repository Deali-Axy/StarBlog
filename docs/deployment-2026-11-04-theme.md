# StarBlog 系统主题镜像发布记录：2026-11-04

2026-11-04 17:12（UTC+8）通过 ship 成功部署 `v2026.11.04.3`，公网地址为 <https://blog.deali.cn/>。新访客默认跟随系统主题，已有手动保存的浅色或深色偏好继续优先生效。

本文保留 2026-11-04 的发布验收记录，不作为当前线上版本或后续数据变更的实时状态说明；生产当前版本以服务器 `active-release.json` 为准。

## 发布身份

- 镜像源码：`4ab6754e757ab97f120fd0e3b99f0a69c51c37c3`，构建时工作区干净。
- ship run ID：`6b8d79f0f864f18e`。
- 命令：`python3 deploy/ship-local.py run -v v2026.11.04.3 -y`。
- API、Web、Gateway 的 build、tag、publish、deploy、verify 阶段全部成功。

| 组件 | 运行镜像摘要 |
| --- | --- |
| API | `sha256:e31ac7a20582c9c0da0f01a3e825cf473cc0c2cc31f6ff2fa81a23296953af4a` |
| Web | `sha256:526b52b626e5af6fe36d82640ed91f8b2f85d14166c849a8a8d414c06075e83b` |
| Gateway | `sha256:502d8144a50cc91f7e1689a01bf715b62b1903267259e35cf201d2e9ca42fb53` |

仓库为 `ccr.ccs.tencentyun.com/deali/starblog`。服务器版本记录与容器配置均引用以上摘要，三个容器正常运行，Web 为 healthy。

## 验收与备份

- API、Web、Admin 生产构建成功；Next.js TypeScript 检查通过。
- 部署脚本完成就绪检查、默认首页、后台入口和 SEO 验收；精选分类 9 个、精选照片 12 张，与部署前一致。
- 公网隔离浏览器验证新访客采用系统浅色，以及打开页面后随系统切换至深色、再切回浅色。
- 已保存的手动深色偏好在系统浅色下生效，已保存的手动浅色偏好在系统深色下生效。
- 公网 `/health/ready` 返回 200，状态为 `Healthy`。

切换前数据库目录备份位于 `/home/deali/projects/starblog-v2/backups/images-20261104T091157Z/data`。版本记录为服务器 `active-release.json`，发布配方位于 `image-releases/v2026.11.04.3/`；回退方法见 [镜像发布指南](image-deployment.md)。

公网浏览器验收报告保存在 Git 忽略目录 `backups/image-deployment-v2026.11.04.3/public-verification.json`。
