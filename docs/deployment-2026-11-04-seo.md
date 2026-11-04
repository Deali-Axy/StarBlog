# StarBlog SEO 镜像发布记录：2026-11-04

2026-11-04（UTC+8）通过 ship 成功部署 `v2026.11.04.2`，公网地址为 <https://blog.deali.cn/>。本次上线整站 SEO、摄影目录和错误恢复改进；公网验收于 16:30 开始，最终结果全部通过。

## 发布身份

- 镜像源码：`65f3d2b8a30c9d0364f8c0f788e7b5c513384e0a`，工作区干净。
- ship run ID：`75e59b503bb28f2c`。
- 命令：`python3 deploy/ship-local.py run -v v2026.11.04.2 -y`。
- API、Web、Gateway 的 build、tag、publish、deploy、verify 阶段全部成功。

| 组件 | 运行镜像摘要 |
| --- | --- |
| API | `sha256:bf5f4d6d86f28a852bc33c25b19ff2ebfb780c2da1fb0ba19e3dbf6486542fd1` |
| Web | `sha256:a5b34a53c5abaed222a50c8d587e06074bbc78d4c1a8bbe735ebd0e5528b5b33` |
| Gateway | `sha256:502d8144a50cc91f7e1689a01bf715b62b1903267259e35cf201d2e9ca42fb53` |

仓库为 `ccr.ccs.tencentyun.com/deali/starblog`。已逐一核对容器配置引用和实际 image ID 与对应摘要一致，Web 为 healthy。Gateway 镜像内容未改变，新的 Nginx 配置通过既有只读挂载更新。

## 部署前修正

生产网关原来把 `robots.txt` 和 sitemap 交给 API，修正为转发到 Web，确保新 SEO 路由在公网生效。部署验收增加首页标题、各栏目 canonical、Web sitemap 索引和分片检查，发现旧网关路由或 canonical 回归时触发既有自动恢复流程。镜像发布与失败恢复测试共 9 项通过。

## 公网验收

- 首页标题为「技术、摄影与生活手记 · StarBlog」。文章列表、分页、摄影、摄影目录、关于、友链、RSS、搜索、文章及照片详情共 12 个页面返回 200，标题、描述、canonical 和分享信息符合页面内容。
- 英文文章 `/blog/unboxing-two-type-c-hubs?lang=en` 的 canonical 保留语言参数，并输出对应 `hreflang`；不存在的页面输出 `noindex`。
- 真实照片详情的初始 HTML 包含作品标题；搜索结果不参与收录。
- `/robots.txt`、三个 sitemap 入口和全部分片均可访问。页面分片 100 条、公开文章分片 681 条、图片分片 35 条，URL 均使用公开域名。
- 就绪检查、后台入口与深层路由、Feed、默认 PNG 分享封面均正常。
- 公网执行 2 项只读浏览器冒烟检查，首页标题与品牌、robots 检查全部通过。未在生产环境运行依赖测试数据或可能写入业务内容的完整浏览器套件。
- 验收中一次公网 TCP 连接超时，有限重试后全部请求通过；ship 健康检查首次即成功。

## 数据、备份与恢复

部署前后公开文章仍为 681 篇。总文章 759 篇、分类 93 个、照片 35 张、精选文章 6 篇、精选分类 9 个、精选照片 12 张，与基线一致；精选分类和照片 ID 集合也一致。

切换前 SQLite 目录备份：`/home/deali/projects/starblog-v2/backups/images-20261104T082748Z/data`。备份 `quick_check=ok`，含 759 篇文章、275 篇译文和 35 张照片。未重新导入数据库、媒体或私密配置，API 继续使用原持久化挂载。

服务器版本记录为 `/home/deali/projects/starblog-v2/active-release.json`，发布配方位于 `image-releases/v2026.11.04.2/`。备份目录还保留上一版环境变量、Compose、网关配置和发布记录；恢复方法见 [镜像发布指南](image-deployment.md)，不要覆盖当前数据库以免丢失后续内容。

公网验收报告保存在 Git 忽略目录 `backups/image-deployment-v2026.11.04.2/public-verification.json`。
