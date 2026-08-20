# StarBlog API 路由说明

`apps/api/src/StarBlog.Api` 是唯一对外 HTTP 入口。契约使用标准 HTTP 状态码和 `ProblemDetails`，成功响应直接返回 JSON，不再包装 `ApiResponse<T>`。旧的 `/Api/*` 路由已经移除。

分页统一为：

```json
{ "items": [], "page": 1, "pageSize": 10, "totalCount": 0, "totalPages": 0 }
```

## 认证与初始化

| 方法 | 路径 | 鉴权 | 说明 |
| --- | --- | --- | --- |
| POST | `/api/v1/auth/tokens` | 匿名 | 登录，返回 `{ token, expires }` |
| GET | `/api/v1/users/me` | JWT | 当前用户 |
| GET | `/api/v1/site/initialization` | 匿名 | 是否已创建管理员 |
| POST | `/api/v1/site/initialization` | 匿名，仅一次 | 创建首个管理员 |

## 内容

| 方法 | 路径 | 鉴权 | 说明 |
| --- | --- | --- | --- |
| GET | `/api/v1/posts` | 匿名；登录后可见草稿 | 文章分页 |
| GET | `/api/v1/posts/{id}` | 匿名 | 文章详情，含 ToC |
| POST | `/api/v1/posts` | JWT | 创建文章 |
| PUT | `/api/v1/posts/{id}` | JWT | 更新文章 |
| DELETE | `/api/v1/posts/{id}` | JWT | 删除文章 |
| POST | `/api/v1/posts/imports` | JWT | 上传 Markdown zip，按文件夹建分类 |
| POST | `/api/v1/posts/{id}/images` | JWT | 上传文章图片，返回 `{ url }` |
| POST | `/api/v1/posts/{id}/featured-post` | JWT | 设为精选 |
| PUT | `/api/v1/posts/{id}/top-placement` | JWT | 设为唯一置顶 |
| POST | `/api/v1/posts/{id}/translations` | JWT | 触发翻译 |
| GET | `/api/v1/categories` | 匿名 | 分类分页 |
| GET | `/api/v1/categories/tree` | 匿名 | 可见分类树 |
| POST/PUT/DELETE | `/api/v1/categories` | JWT | 分类写入 |
| POST | `/api/v1/categories/{id}/featured-category` | JWT | 推荐分类 |

## 站点聚合

| 方法 | 路径 | 鉴权 | 说明 |
| --- | --- | --- | --- |
| GET | `/api/v1/site/home` | 匿名 | 首页精选、友链、随机图开关 |
| GET | `/api/v1/site/overview` | 匿名 | 文章/分类/图片数量 |
| GET | `/api/v1/site/search` | 匿名 | `keyword` 必填 |
| GET | `/api/v1/site/photos/random` | 匿名 | 随机图片 |
| GET | `/api/v1/site/photos/{id}/adjacent/next` | 匿名 | 下一张图片 |
| GET | `/api/v1/site/photos/{id}/adjacent/previous` | 匿名 | 上一张图片 |
| POST | `/api/v1/site/link-exchanges` | 匿名 | 提交友链申请 |
| GET | `/api/v1/theme` | 匿名 | 主题列表 |
| GET | `/feed` | 匿名 | Atom |
| GET | `/robots.txt` | 匿名 | robots |
| GET | `/sitemap.xml`、`/sitemap-index.xml`、`/sitemap-images.xml` | 匿名 | Sitemap |

## 媒体、友链、评论、配置

| 资源 | 公开 | 管理 |
| --- | --- | --- |
| 图片 | `GET /api/v1/photos`、`/{id}`、`/{id}/thumbnail`、`/api/v1/featured-photos` | `POST/PUT/DELETE /api/v1/photos`，精选 `/{id}/featured-photo` |
| 友链 | `GET /api/v1/links` | `/api/v1/admin/links` |
| 友链申请 | `POST /api/v1/site/link-exchanges` | `/api/v1/admin/link-exchange-requests`，`PATCH .../approval` 或 `.../rejection` |
| 评论 | `/api/v1/comments`、`/by-post/{postId}`、`POST /email-otp` | `/api/v1/admin/comments`，`PATCH .../approval` 或 `.../rejection` |
| 配置 | 无 | `/api/v1/admin/settings` |
| 发布中继 | 无 | `/api/v1/admin/publication-channels`、`/api/v1/admin/posts/{postId}/publications`、`/api/v1/admin/publications/{id}/deliveries` |
| 访问统计 | 无 | `/api/v1/admin/visit-records`、`/reports/overview`、`/reports/daily-trends`、`/api/v1/admin/runtime/statistics` |

管理接口除特别标注外均需要 JWT Bearer。发布渠道响应包含 `hasSecret`，永不回显 `appSecret`。
