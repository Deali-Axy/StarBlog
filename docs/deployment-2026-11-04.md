# StarBlog 镜像发布记录：2026-11-04

2026-11-04 10:32（UTC+8）通过 `/home/deali/code/ship/ship` 成功部署 `v2026.11.04.1`，公网 https://blog.deali.cn/ 已使用新默认首页“点阵手记”。

## 发布身份

- UI 提交：`773bc04`；镜像源码提交：`f7f67c99bc58609d4439e9086f13f2adcf1ff7de`。
- 构建与推送 run ID：`9a215994a604d876`。
- 部署成功 run ID：`537049dd86f0df90`；部署命令：`python3 deploy/ship-local.py deploy -v v2026.11.04.1 -y`。
- 仓库：`ccr.ccs.tencentyun.com/deali/starblog`；运行时按下列摘要固定镜像。

| 组件 | 镜像摘要 |
| --- | --- |
| API | `sha256:42a72105e1a281f92039c938acc83605afa0be4c8f18558dc77599eb2957d031` |
| Web | `sha256:f6b6e60af7c1a592a68e463eb9deca9d6ac13a68bdaba7b1ec2090f9399bf154` |
| Gateway | `sha256:502d8144a50cc91f7e1689a01bf715b62b1903267259e35cf201d2e9ca42fb53` |

## 验收

- 无偏好 cookie 的公网首页 HTML 确认为 `journal`、`dots`、`dashed`，文章、摄影、友链模块分别为 `notes`、`prints`、`cards`。
- 公网首页、就绪检查、后台入口与文章管理深层路由、文章列表、摄影列表、Feed 均返回 200；代表性精选照片缩略图返回有效 JPEG。
- 公开文章仍为 681 篇，9 个精选分类和 12 张精选照片与发布前完全一致。
- 三个线上容器实际使用上述 registry 摘要，Web 健康；应用代码不再通过宿主发布目录挂载。API 继续挂载原来的数据库、媒体、缓存、邮件配置和词库。
- 切换前完整 SQLite 目录已备份到 `/home/deali/projects/starblog-v2/backups/images-20261104T023158Z/data`。备份 `quick_check=ok`，含 759 篇文章、275 篇译文、35 张照片、9 个精选分类和 12 张精选照片。更新未执行数据重新导入。
- 浏览器访问宿主不可用，未执行视觉或浏览器交互验收；以上 UI 结果基于实际公网响应中的渲染标记。

## 运行与回退

当前版本记录在服务器 `/home/deali/projects/starblog-v2/active-release.json`，配方位于 `image-releases/v2026.11.04.1/`。上述备份目录也包含上一版本的 Compose、环境变量和网关配置。私密配置不可输出到公开日志。

回退步骤和 schema 兼容边界见 [镜像发布指南](image-deployment.md)。首次迁移的旧发布目录仍保留，回退不能覆盖当前数据库以免丢失新增内容。

本地公网验收报告位于 Git 忽略的 `backups/image-deployment-v2026.11.04.1/public-verification.json`。既有测试与构建结果：14 项首页测试、2 项 UI 架构测试、5 项镜像发布与失败恢复测试、TypeScript 检查以及三组件生产构建通过。
