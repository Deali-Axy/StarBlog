# 档案工作台发布记录：2026-11-18

2026-11-18 11:10（UTC+8）部署 `v2026.11.18.2`，站点为 https://blog.deali.cn/。

## 内容与来源

- `8d892dc`：工作台封面、精简工具栏与侧栏、全宽页面框架；仅 studio 列表页移除网站 footer。
- `369fde9`：文章详情页全宽布局，返回入口移到目录栏，分类链接并入文章信息。
- `f6a7bf6`：文档信息布局调整及中英文摘要字段；保留手动调整。
- 构建源码：`f6a7bf699766aaefb1f470c2d2b1de98c8ce1f57`，工作区干净。
- ship run：`3180ba5a3e9cf82e`，命令为 `python3 deploy/ship-local.py run -v v2026.11.18.2 -y`；故障处理后使用同一 run 的 `--resume` 完成发布。

| 组件 | 镜像摘要 |
| --- | --- |
| API | `sha256:32a49aa871a2ecb74bdb6cf822b2f1f9cb104f00280225dd3292adca610d97bf` |
| Web | `sha256:181930665a64a465e298039467e1f7c224992250b9d88fefe1f4b3f5965a9087` |
| Gateway | `sha256:d5713ee942a9fa1576451f56e737e198452bc1d43acd69da76cfd048271d7ccb` |

## 验收

- TypeScript 及 18 项博客导航、文章阅读、文章交互与架构测试通过；三个应用生产构建成功。
- Compose 就绪检查及公网 `/health/ready` 通过，服务器 active-release 的版本和源码一致。
- 公网 `/blog?bv=studio` 包含封面且无网站 footer；library、editorial 列表页保留 footer。
- `/blog/feaulle-lian3-hifi-wired-earphones?bv=studio` 包含文章摘要和详情页 footer。
- 部署流程通过首页、后台与 SEO 验收，精选分类 9 个、精选照片 12 张保持一致。
- 本次为 HTTP 与服务端 HTML 验收，未做浏览器视觉与点击验收。

## 备份及磁盘故障

首次部署在复制数据库备份时遇到磁盘耗尽，API 停止后自动回退也因空间不足失败。删除本次生成的不完整备份副本 `backups/images-20261118T030800Z/data/app.db` 后重启原 API，健康检查恢复。构建缓存清理未释放空间；移除六个可从仓库重新拉取的 StarBlog 历史镜像缓存后恢复发布，未删除生产数据库、媒体、卷或已有完整备份。

成功发布的完整备份：`/home/deali/projects/starblog-v2/backups/images-20261118T031015Z`。上一版本为 `v2026.11.18.1`，回退遵循 [镜像发布指南](image-deployment.md)，不得覆盖发布后的生产数据库。

发布后根分区余量约 923MB（使用率 99%），后续发布前需要安排磁盘容量处理。
