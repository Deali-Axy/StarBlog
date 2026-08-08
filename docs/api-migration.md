# StarBlog 纯 API 路由说明

`src/StarBlog.Api` 已承接旧 Razor/MVC 项目的对外能力。旧的 `Api/*` 地址继续可用，方便渐进式替换前端；新的管理端统一提供 `/Api/Admin/*` 别名。

## 前台站点接口

| 路径 | 用途 |
| --- | --- |
| `GET /Api/Site/home` | 首页聚合数据：精选文章、照片、分类和可见友链 |
| `GET /Api/Site/search?keyword=...` | 已发布文章搜索，返回高亮标题和片段 |
| `GET /Api/Site/photos/{id}/next` | 下一张照片 |
| `GET /Api/Site/photos/{id}/previous` | 上一张照片 |
| `GET /Api/Site/photos/random` | 随机照片 |
| `POST /Api/Site/link-exchanges` | 提交友链申请 |
| `GET/POST /Api/Site/initialization` | 查询或执行首次安装初始化 |
| `GET /feed` | Atom feed（兼容旧链接） |
| `GET /sitemap-index.xml`、`/sitemap.xml`、`/sitemap-images.xml` | 搜索引擎资源（兼容旧链接） |

文章翻译仍位于文章资源下：`POST /Api/BlogPost/{id}/Translate`、`GET /Api/BlogPost/{id}/GetTranslation`、`GET /Api/BlogPost/{id}/AvailableTranslations`、`DELETE /Api/BlogPost/{id}/DeleteTranslation/{translationId}`。需在配置中提供 `Translation:LLM:Key` 后才可发起翻译请求。

## 管理端接口

除登录以外，以下地址均使用 JWT Bearer 鉴权；它们是旧路由的等价管理端入口。

| 资源 | 管理端基路径 |
| --- | --- |
| 登录与当前用户 | `/Api/Admin/Auth` |
| 文章、上传与翻译 | `/Api/Admin/Posts`、`/Api/Admin/Blog` |
| 分类与精选内容 | `/Api/Admin/Categories`、`/Api/Admin/Featured/{Posts|Categories|Photos}` |
| 图片库 | `/Api/Admin/Photos` |
| 评论审核 | `/Api/Admin/Comments` |
| 友链与申请审核 | `/Api/Admin/Links`、`/Api/Admin/LinkExchanges` |
| 配置 | `/Api/Admin/Config` |
| 运行时面板与访问分析 | `/Api/Admin/Dashboard`、`/Api/Admin/Analytics` |

`/Api/Admin/Comments` 是独立管理控制器：`GET` 返回包含待审核评论的列表，`POST /{id}/accept` 和 `POST /{id}/reject` 完成审核。其余管理端别名复用既有动作路径（例如 `PUT /Api/Admin/Posts/{id}`）。
