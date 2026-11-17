# StarBlog 博客版式与图片预览发布记录：2026-11-17

2026-11-17 22:36（UTC+8）通过 ship 成功部署 `v2026.11.17.1`，公网地址为 <https://blog.deali.cn/>。

## 发布内容与身份

- 折页手记列表减少分隔线，以留白、浅纸色与文字层级组织内容，调整搜索工具区、展开与紧凑列表。
- 三套文章主题接入图片预览，提供前后切图、缩放、旋转、全屏及新窗口打开原图；已有图片链接保留原跳转。
- 镜像源码：`d4e15a7670ae2d06d83911cc66932b492f2c6996`，构建时工作区干净。
- ship run ID：`fe446787d97bd969`。
- 发布命令：`python3 deploy/ship-local.py run -v v2026.11.17.1 -y`。
- API、Web、Gateway 的 build、tag、publish、deploy、verify 阶段全部成功。

| 组件 | 运行镜像摘要 |
| --- | --- |
| API | `sha256:5c9b02131d293adc3c37a068cfad535922ee99c7022cf37df84a3d1b5f0742a6` |
| Web | `sha256:12f144bd6fceb694c95d5db7ac939e8546ee1d625334f7c2eb143b02c78ca16f` |
| Gateway | `sha256:502d8144a50cc91f7e1689a01bf715b62b1903267259e35cf201d2e9ca42fb53` |

镜像仓库为 `ccr.ccs.tencentyun.com/deali/starblog`。服务器 `active-release.json` 的版本、源码及镜像摘要已与本次发布核对一致。

## 验收与数据

- API、Web、Admin 生产构建成功，Next.js TypeScript 检查通过。
- 三个容器均通过 Compose 就绪检查，公网 `/health/ready` 返回 200 且状态为 `Healthy`。
- 部署脚本完成首页、后台和 SEO 验收；精选分类 9 个、精选照片 12 张，与部署前一致。
- 公网 `/blog?bv=editorial` 返回 200，包含新版折页手记布局。
- 含图片的文章 `/blog/ai-drawing-stablediffusion` 在 `library`、`editorial`、`studio` 三套主题下均返回 200，服务器渲染的正文中均包含 6 个图片预览链接。
- 公网 `/admin/` 返回 200。
- 部署后以 SQLite 只读查询生产库，已发布文章仍为 687 篇，其中 682 篇有英文译文，与部署前一致。

以上网页检查为 HTTP 和服务端 HTML 验证，未进行浏览器中的真实点击、缩放或全屏交互验收。公网验收报告位于 Git 忽略目录 `backups/image-deployment-v2026.11.17.1/public-verification.json`。

## 备份与回退

切换前的备份目录为 `/home/deali/projects/starblog-v2/backups/images-20261117T143624Z`，其中 `data/` 保存数据库目录；环境变量、Compose、网关配置及上一份发布记录也已备份。此次更新保留服务器上的数据库、媒体与私密配置。

本次发布配方位于服务器 `image-releases/v2026.11.17.1/`，前一版本为 `v2026.11.04.3`。回退方法见 [镜像发布指南](image-deployment.md)；回退程序时应继续保留当前数据库，不能用旧数据库快照覆盖上线后的新增内容。
