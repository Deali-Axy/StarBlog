# StarBlog.Web API 端点清单

本文档用于梳理当前 `apps/web-legacy` 项目内已存在的 API 端点，按用途分为「公开」「后台」「站点资源」，并给出 Next.js 前端改造阶段需要调用的最小集合。

## 约定与说明

- API 端点主要位于：`apps/web-legacy/Apis/**`
- 站点 MVC 页面主要位于：`apps/web-legacy/Controllers/**`（不属于 WebAPI，但其中部分是“资源输出型端点”，见「站点资源」）
- API 路由以特性路由为主，多数使用 `Api/[controller]` 前缀
- 鉴权方式：JWT Bearer（`[Authorize]` / `[AllowAnonymous]`）
- 返回风格：项目注册了全局 `ResponseWrapperFilter`，多数返回会被包装成统一结构；但仍存在少量 Action “裸返回”的情况（需要前端注意或后续统一）

## 公开（前台可匿名调用）

### 博客

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 获取置顶文章 | GET | `Api/Blog/Top` | 匿名 | 裸返回 `Post?` |
| 获取精选文章列表 | GET | `Api/Blog/Featured` | 匿名 | 裸返回 `List<Post>` |
| 获取博客概览 | GET | `Api/Blog/Overview` | 匿名 | 裸返回 `BlogOverview` |
| 获取状态列表 | GET | `Api/Blog/GetStatusList` | 匿名 | 裸返回 `List<string?>` |
| 文章分页列表 | GET | `Api/BlogPost` | 匿名（Action 标注） | 返回 `ApiResponsePaged<Post>` |
| 文章详情（按 id） | GET | `Api/BlogPost/{id}` | 匿名（Action 标注） | 返回 `ApiResponse<Post>` |
| 文章详情（按 slug） | GET | `Api/BlogPost/slug/{slug}` | 匿名（Action 标注） | 返回 `ApiResponse<Post>` |

### 分类

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 分类树（节点） | GET | `Api/Category/Nodes` | 匿名（Action 标注） | 裸返回 `List<CategoryNode>?` |
| 全量分类 | GET | `Api/Category/All` | 匿名（Action 标注） | 裸返回 `List<Category>` |
| 分类分页列表 | GET | `Api/Category?page=1&pageSize=10` | 匿名（Action 标注） | 返回 `ApiResponsePaged<Category>` |
| 分类详情 | GET | `Api/Category/{id:int}` | 匿名（Action 标注） | 返回 `ApiResponse<Category>` |
| 分类词云 | GET | `Api/Category/WordCloud` | 匿名（Action 标注） | 裸返回 `List<object>` |

### 摄影/照片

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 照片分页列表 | GET | `Api/Photo?page=1&pageSize=10` | 匿名（Action 标注） | 返回 `ApiResponsePaged<Photo>` |
| 照片详情 | GET | `Api/Photo/{id}` | 匿名（Action 标注） | 返回 `ApiResponse<Photo>` |
| 获取缩略图 | GET | `Api/Photo/{id}/Thumb?width=300&quality=85` | 匿名（Action 标注） | 返回图片流（`image/jpeg`） |

### 评论

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 评论分页列表 | GET | `Api/Comment` | 匿名 | 返回 `ApiResponsePaged<Comment>` |
| 获取文章评论列表 | GET | `Api/Comment/GetAll?postId=...` | 匿名 | 裸返回 `List<Comment>?` |
| 发送邮箱 OTP | GET | `Api/Comment/GetEmailOtp?email=...` | 匿名 | 返回 `ApiResponse` |
| 获取匿名用户（邮箱+OTP） | GET | `Api/Comment/GetAnonymousUser?email=...&otp=...` | 匿名 | 返回 `ApiResponse` |
| 新增评论 | POST | `Api/Comment` | 匿名 | 返回 `ApiResponse<Comment>` |

### 友情链接（展示用）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 友情链接列表 | GET | `Api/Link` | 需要登录（控制器 `[Authorize]`） | 当前实现为后台口径；如要前台展示建议后续拆分公开只读接口 |

### 主题

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 主题列表 | GET | `Api/Theme` | 匿名 | 裸返回 `List<Theme>` |

## 后台（需要登录）

### 认证/身份

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 登录 | POST | `Api/Auth`（或 `Api/Auth/Login`） | 匿名 | 返回 `ApiResponse<LoginToken>` |
| 获取当前用户 | GET | `Api/Auth`（或 `Api/Auth/GetUser`） | 需要登录 | 返回 `ApiResponse<User>` |

### 配置（Config）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 获取全部配置 | GET | `Api/Config` | 需要登录 | 裸返回 `List<ConfigItem>` |
| 按 key 获取配置 | GET | `Api/Config/{key}` | 需要登录 | 返回 `ApiResponse<ConfigItem>` |
| 新增配置 | POST | `Api/Config` | 需要登录 | 返回 `ApiResponse<ConfigItem>` |
| 更新配置 | PUT | `Api/Config/{key}` | 需要登录 | 返回 `ApiResponse<ConfigItem>` |
| 删除配置 | DELETE | `Api/Config/{key}` | 需要登录 | 返回 `ApiResponse` |

### 文章（BlogPost）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 新增文章 | POST | `Api/BlogPost` | 需要登录（控制器 `[Authorize]`） | 返回 `ApiResponse<Post>` |
| 更新文章 | PUT | `Api/BlogPost/{id}` | 需要登录 | 返回 `ApiResponse<Post>` |
| 删除文章 | DELETE | `Api/BlogPost/{id}` | 需要登录 | 返回 `ApiResponse` |
| 上传文章图片 | POST | `Api/BlogPost/{id}/UploadImage` | 需要登录 | 返回 `ApiResponse` |
| 获取文章图片列表 | GET | `Api/BlogPost/{id}/Images` | 需要登录 | 返回 `ApiResponse<List<string>>` |
| 设置/取消精选 | POST | `Api/BlogPost/{id}/SetFeatured` / `Api/BlogPost/{id}/CancelFeatured` | 需要登录 | 返回 `ApiResponse` 或 `ApiResponse<FeaturedPost>` |
| 设置置顶 | POST | `Api/BlogPost/{id}/SetTop` | 需要登录 | 返回 `ApiResponse<TopPost>` |

### 分类（Category）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 新增分类 | POST | `Api/Category` | 需要登录（控制器 `[Authorize]`） | 返回 `ApiResponse<Category>` |
| 更新分类 | PUT | `Api/Category/{id:int}` | 需要登录 | 返回 `ApiResponse<Category>` |
| 删除分类 | DELETE | `Api/Category/{id:int}` | 需要登录 | 返回 `ApiResponse` |
| 设置/取消精选 | POST | `Api/Category/{id:int}/SetFeatured` / `Api/Category/{id:int}/CancelFeatured` | 需要登录 |  |
| 设置可见/不可见 | POST | `Api/Category/{id:int}/SetVisible` / `Api/Category/{id:int}/SetInvisible` | 需要登录 |  |

### 精选（Featured*）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 精选文章列表/详情 | GET | `Api/FeaturedPost` / `Api/FeaturedPost/{id:int}` | 匿名（Action 标注） | 管理口径但开放读取 |
| 新增/删除精选文章 | POST/DELETE | `Api/FeaturedPost?postId=...` / `Api/FeaturedPost/{id:int}` | 需要登录 |  |
| 精选分类列表/详情 | GET | `Api/FeaturedCategory` / `Api/FeaturedCategory/{id:int}` | 匿名（Action 标注） |  |
| 新增/删除精选分类 | POST/DELETE | `Api/FeaturedCategory` / `Api/FeaturedCategory/{id:int}` | 需要登录 |  |
| 精选照片列表/详情 | GET | `Api/FeaturedPhoto` / `Api/FeaturedPhoto/{id:int}` | 匿名（Action 标注） |  |
| 新增/删除精选照片 | POST/DELETE | `Api/FeaturedPhoto` / `Api/FeaturedPhoto/{id:int}` | 需要登录 |  |

### 摄影/照片（管理）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 新增照片 | POST | `Api/Photo` | 需要登录（控制器 `[Authorize]`） | `multipart/form-data` |
| 更新照片 | PUT | `Api/Photo/{id}` | 需要登录 |  |
| 删除照片 | DELETE | `Api/Photo/{id}` | 需要登录 |  |
| 设置/取消精选 | POST | `Api/Photo/{id}/SetFeatured` / `Api/Photo/{id}/CancelFeatured` | 需要登录 |  |
| 重建数据 | POST | `Api/Photo/ReBuildData` | 需要登录 |  |
| 批量导入 | POST | `Api/Photo/BatchImport` | 需要登录 |  |

### 友情链接（管理）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 获取列表/详情 | GET | `Api/Link` / `Api/Link/{id:int}` | 需要登录（控制器 `[Authorize]`） |  |
| 新增 | POST | `Api/Link` | 需要登录 | 当前 Action 裸返回 `Link` |
| 更新/删除 | PUT/DELETE | `Api/Link/{id:int}` | 需要登录 |  |

### 友链申请（审核）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 列表/详情 | GET | `Api/LinkExchange` / `Api/LinkExchange/{id:int}` | 需要登录（控制器 `[Authorize]`） |  |
| 通过/拒绝 | POST | `Api/LinkExchange/{id:int}/Accept` / `Api/LinkExchange/{id:int}/Reject` | 需要登录 |  |
| 删除 | DELETE | `Api/LinkExchange/{id:int}` | 需要登录 |  |

### 访问统计（VisitRecord）

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| 访问记录分页/详情 | GET | `Api/VisitRecord` / `Api/VisitRecord/{id:int}` | 需要登录 |  |
| 全量/概览/趋势/分布/Top 等 | GET | `Api/VisitRecord/*` | 需要登录 | 端点较多，集中在一个控制器内 |

### 运行时仪表盘

| 功能 | 方法 | 路径 | 鉴权 | 备注 |
|---|---:|---|---|---|
| CLR 运行时统计 | GET | `Api/Dashboard/ClrStats` | 需要登录 |  |

## 站点资源（非 JSON API，但对外属于“接口式资源”）

这些端点目前位于 MVC Controllers 中，返回 XML 或文件流；如果后续前端全面迁移到 Next.js，建议由 Next.js 生成（或由 API 项目专门承载）。

| 功能 | 方法 | 路径 | 当前位置 | 备注 |
|---|---:|---|---|---|
| RSS Feed | GET | `/feed` | `Controllers/RssController` | `application/xml` |
| Sitemap Index | GET | `/sitemap-index.xml` | `Controllers/SitemapController` | `application/xml` |
| Sitemap（文章等） | GET | `/sitemap.xml` | `Controllers/SitemapController` | `application/xml` |
| 图片 Sitemap | GET | `/sitemap-images.xml` | `Controllers/SitemapController` | `application/xml` |

## Next.js 需要调用的最小集合（建议）

以“先把 Next.js 跑起来 + 覆盖主要页面”为目标，优先保证这些接口稳定可用：

### 文章与首页

- `GET Api/BlogPost`：首页/列表页分页
- `GET Api/BlogPost/slug/{slug}`：文章详情页（推荐用 slug 做路由）
- `GET Api/Blog/Top`：首页置顶（可选）
- `GET Api/Blog/Featured`：首页精选（可选）
- `GET Api/Blog/Overview`：站点概览（可选）

### 分类与导航

- `GET Api/Category/Nodes`：导航/分类树（或侧边栏）
- `GET Api/Category/All`：分类集合（可选）

### 评论

- `GET Api/Comment/GetAll?postId=...`：文章详情页评论列表
- `POST Api/Comment`：提交评论
- `GET Api/Comment/GetEmailOtp` + `GET Api/Comment/GetAnonymousUser`：匿名评论身份（如果 Next.js 前端继续沿用当前逻辑）

### 摄影

- `GET Api/Photo`：摄影列表
- `GET Api/Photo/{id}`：摄影详情
- `GET Api/Photo/{id}/Thumb`：列表缩略图

### 主题/外观

- `GET Api/Theme`：主题列表（如果前端需要）

### SEO（建议迁移到 Next.js 生成）

- `/feed`、`/sitemap*.xml`

## 已知需要后续收敛的问题（可选）

- CORS：此前 `WithOrigins(...)` 多次调用可能导致只剩最后一个 origin 生效；已在 `Program.cs` 合并为单次调用
- 返回包装：存在少量“裸返回”与 `ApiResponse` 混用，Next.js 调用时需注意（建议后续统一 DTO 与返回模型）
