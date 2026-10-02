# Next.js 前台对齐 web-legacy 计划

日期：2026-10-02
状态：已完成
前置：`apps/web` 第一版已覆盖首页聚合、文章列表/分类、搜索、文章 Markdown 详情。

## 1. 目标

把 [`apps/web-legacy`](../../apps/web-legacy) 上仍在使用的公开站能力迁到 [`apps/web`](../../apps/web)，使独立 Next.js 前台可以替换 Razor 宿主。后台继续只走 [`apps/admin`](../../apps/admin)；本批次不移植 `/Admin`，也不做 Bootswatch 整站换肤。

对齐以**用户可完成的公开流程**为准，而不是逐文件复刻 Bootstrap 布局。视觉继续用现有 shadcn / Magic UI 语言。

完成标准：

- 旧站导航里的公开入口在新站都有对应路由（SeoTest、Init、Temp 除外）。
- 摄影、评论、友链申请、关于页、RSS 说明页可在浏览器走通。
- 文章详情具备评论、相关文章、版权/永久链接、基础 SEO（Open Graph + JSON-LD）。
- API 侧补齐本批次必需的只读缺口；sitemap / robots / feed 的 URL 指向 Next 路由。
- 不删除 `apps/web-legacy`（删除放到再下一批，确认流量切走之后）。

## 2. 第一版已有 / 明确不做

### 已有

| 能力 | 现状 |
| --- | --- |
| 首页聚合 | `GET /api/v1/site/home`：置顶、精选文章/分类/图片、友链展示、随机图 |
| 文章列表 | `/blog`：分类树、标题搜索、分页、网格/列表 |
| 文章详情 | `/blog/[idOrSlug]`：Markdown、API `toc` |
| 搜索 | `/search?keyword=` → `GET /api/v1/site/search` |
| 主题 | `next-themes` 浅色/深色 |
| Feed 入口 | 顶栏链接到 API `/feed`（Next rewrite） |

### 本批次不做

- 管理端任何页面。
- Bootswatch 主题列表（`GET /api/v1/theme` 可保留给以后，不接入 UI）。
- 旧站 `/Home/Init`（已由 `POST /api/v1/site/initialization` + Admin 承接）。
- `/seo-test`、`/Blog/Temp`、`Product` 占位页。
- 旧 PicLib 随机封面服务；列表卡继续用渐变占位或正文首图。
- Umami / Clarity 注入（等有公开配置契约再做）。
- 删除 `apps/web-legacy`。

## 3. 路由对照

| 旧站 | 新站 | 备注 |
| --- | --- | --- |
| `/` | `/` | 补图表、随机文章、友链申请入口 |
| `/Blog/List` | `/blog` | 已有；补服务端排序若 API 支持 |
| `/Blog/Post/{id}`、`/p/{slug}` | `/blog/[idOrSlug]`，另加 `/p/[slug]` 兼容跳转 | 旧外链大量走 `/p/{slug}` |
| `/Blog/RandomPost` | `/blog/random` | 需 API 或列表侧随机 |
| `/Search/Blog?keyword=` | `/search` | 已有 |
| `/Photography` | `/photography` | 本批次新增 |
| `/Photography/Photo/{id}` | `/photography/[id]` | |
| `/Photography/Next\|Previous\|RandomPhoto` | 详情页内上一张/下一张/随机 | API 已有 |
| `/About` | `/about` | 静态内容 + 环境变量 |
| `/LinkExchange/Add` | `/links` | 列表 + 申请表单 |
| `/Rss` | `/rss` | 说明页；`/feed` 仍代理 API |
| `/sitemap*.xml`、`/robots.txt` | Next rewrite 到 API | 并修正 sitemap 里的前台路径 |

## 4. 分批落地

建议按可独立验收的切片推进，每片合并后前台仍可发布。

### 4.1 摄影

旧站：[`PhotographyController.cs`](../../apps/web-legacy/Controllers/PhotographyController.cs)、[`Views/Photography`](../../apps/web-legacy/Views/Photography)。

- `/photography`：`GET /api/v1/photos` 分页，CSS 多列瀑布流，缩略图走 `/api/v1/photos/{id}/thumbnail`。**已完成**
- `/photography/[id]`：原图 `/media/photography/{filePath}`，标题/地点/时间。**已完成**
- 上一张 / 下一张 / 随机：`GET /api/v1/site/photos/{id}/adjacent/*`、`GET /api/v1/site/photos/random`。**已完成**
- 首页精选图可点进详情（第一版只展示）。**已完成**

API：已具备，无需新契约。

### 4.2 评论

旧站仅后端渲染页挂了 [`Widgets/Comment.cshtml`](../../apps/web-legacy/Views/Blog/Widgets/Comment.cshtml) + `comment.js`。新站用 React Query 接到现有评论 API。

- 文章详情底部：树形列表 `GET /api/v1/comments/by-post/{postId}`。**已完成**
- 发表：邮箱 + `POST /api/v1/comments/email-otp` + `POST /api/v1/comments`（`userName`、`email`、`url?`、`emailOtp`、`parentId?`）。**已完成**
- 审核中评论按 API 的 `visible` / `isNeedAudit` 显示「待审核」而不是当成失败。**已完成**
- 不移植 `GetAnonymousUser` 和 PicLib 头像；头像用昵称首字母。**已完成**

API：读写已有。若缺「按邮箱回填昵称」可在前端用 localStorage 记住上次昵称/网址。

### 4.3 友链

- `/links`：`GET /api/v1/links` 展示可见友链。**已完成**
- 申请表单：`POST /api/v1/site/link-exchanges`（Name、Description、Url、WebMaster、Email），成功用 sonner 提示，错误解析 ProblemDetails。**已完成**
- 首页友链区加「申请友链」按钮。**已完成**

API：已有。

### 4.4 关于页与 RSS 说明

- `/about`：从旧 [`Views/About/Index.cshtml`](../../apps/web-legacy/Views/About/Index.cshtml) 迁文案与外链；站长名、社交链接放入 `NEXT_PUBLIC_*` 或 `lib/site.ts`，二维码等静态资源放到 `apps/web/public`。**已完成**
- `/rss`：说明 Atom 地址，链到 `/feed`。**已完成**
- 顶栏补「摄影 / 关于 / 友链」。**已完成**

无需新 API。

### 4.5 文章详情补全

对照 [`BlogController.Post`](../../apps/web-legacy/Controllers/BlogController.cs) 与 `PostHeader` / `PostBottom` / `RelatedPosts`。

- 相关文章：同分类随机补齐。优先加只读 API（见第 5 节）；短期也可用当前分类的 `GET /api/v1/posts?categoryId=` 在服务端排除本文后取几篇。**已完成**（`GET /api/v1/posts/{id}/related`）
- 随机一篇：`/blog/random` 跳转到详情。**已完成**
- 版权声明、永久链接、二维码（可选，可用当前 URL 的客户端 QR）。**已完成**（版权与永久链接；二维码未做）
- 兼容路由：`/p/[slug]` → 内部 `redirect` 到 `/blog/{slug}`，避免旧外链 404。**已完成**
- 译文：旧站 `?lang=` 读 `PostTranslation`。公开读接口目前没有，列入 API 缺口；本批次若来不及做接口，详情页先不展示语言切换。**已完成**（`?lang=` + 详情语言切换）

### 4.6 首页补全

对照 [`Views/Home/Index.cshtml`](../../apps/web-legacy/Views/Home/Index.cshtml)。

- 分类词云 / 图表：旧站 `GET /Api/Category/WordCloud` + Chart.js。新 API 无此资源，见第 5 节。没有接口前不要用假数据。**已完成**（`GET /api/v1/categories/word-cloud`，受 `chartVisible` 控制）
- 技能标签：链到 `/search?keyword=`。**已完成**
- 随机文章、随机摄影 CTA。**已完成**
- 移动端可隐藏大图（CSS/`use-mobile`），不必复刻 User-Agent 判断。**已完成**

### 4.7 SEO 与站点协议

旧站：`SeoMeta`、`StructuredDataService`、`SitemapService`、`RssController`。

Next 侧：

- 根布局与文章/摄影详情补 `openGraph`、`twitter`、canonical。**已完成**
- 文章页 JSON-LD：`BlogPosting` + `BreadcrumbList`；首页 `WebSite`（`SearchAction` 指向 `/search?keyword=`，不要写成旧的 `/Search?q=`）。**已完成**
- `next.config.ts` rewrite 增加 `/robots.txt`、`/sitemap.xml`、`/sitemap-index.xml`、`/sitemap-images.xml`。**已完成**
- **API 生成的 sitemap / robots 必须改为 Next 路径**（`/blog`、`/blog/{slug}`、`/photography` 等），否则切流后搜索引擎仍指向 Razor 路由。**已完成**

### 4.8 列表与搜索体验（小项）

- 分类筛选含子分类：旧站 `IncludeSubCategory`。若 `GET /api/v1/posts?categoryId=` 只匹配自身，需 API 支持 `includeDescendants` 或前端展开子 id 多次请求。**已完成**（`includeDescendants=true`）
- 服务端排序：旧站 `CreationTime` / `LastUpdateTime` / `Title`。现在标题排序在客户端，翻页后不正确；应对齐到 `GET /api/v1/posts` 的 `sortBy`（若没有则补查询参数）。**已完成**
- 搜索页可保留高亮；grid/list 切换非必须。**已有**

## 5. API 缺口（本批次建议补）

只补公开只读或已有模块上的小扩展，不改管理契约。

| 缺口 | 建议 | 阻塞的前台 | 完成 |
| --- | --- | --- | --- |
| 相关文章 | `GET /api/v1/posts/{id}/related?limit=4` | 详情「相关」 | 已完成 |
| 随机文章 | `GET /api/v1/posts/random`（仅已发布） | `/blog/random`、首页 CTA | 已完成 |
| 分类词云 | `GET /api/v1/categories/word-cloud` | 首页图表 | 已完成 |
| 公开译文 | `GET /api/v1/posts/{id}?lang=`、`GET /api/v1/posts/{id}/translations`、`GET /api/v1/posts/{id}/translations/{lang}` | 详情语言切换 | 已完成 |
| 列表排序 / 子分类 | `sortBy`、`includeDescendants` | 列表与旧站一致 | 已完成 |
| Sitemap 路径 | `SiteResourceOperations` 使用 `/blog`、`/photography` 等前台路由 | SEO | 已完成 |
| 未发布 `GET /posts/{id}` | 匿名 404 | 安全加固，前台已当 404 | 已完成 |

评论、友链、摄影、分类树、搜索、首页聚合：**不必为功能本身新增端点**。

## 6. 实现约定

- 数据：列表/详情继续 RSC + `revalidate`；评论提交、OTP、友链表单用 TanStack Query。
- 错误：统一解析 ProblemDetails。
- 媒体：继续相对路径 + rewrite，不要写死旧 PicLib 域名。
- 路由集中在 [`apps/web/lib/routes.ts`](../../apps/web/lib/routes.ts)。
- 不引入 Fumadocs；不把 Admin JWT 带进公开页。

## 7. 验收清单

- [x] 摄影列表、详情、上一张/下一张/随机
- [x] 文章下评论列表、OTP 发送、发表、回复
- [x] 友链列表与申请成功/校验失败
- [x] 关于页、RSS 说明页、顶栏入口
- [x] `/p/{slug}` 能打开对应文章
- [x] 详情有相关文章与版权/永久链接
- [x] 首页有友链申请入口；词云在 API 就绪后出现
- [x] 文章与摄影详情有 OG；`/robots.txt` 与 sitemap 经 Next 可访问且 loc 为新路由
- [x] 未发布文章详情仍为 404（API 匿名 `GET /api/v1/posts/{id}` 返回 404；前台按未发布处理）

## 8. 再下一批（不在本次）

- 从默认构建与文档中移除 `apps/web-legacy`（确认无未备份的 `wwwroot/media`）。
- 部署：compose / 反代把 `blog.deali.cn` 指到 Next，`/api` 与 `/media` 回源 API。
- 公开站点设置（站点名、统计脚本）若要从 Admin 配置读取，再增加只读 settings 契约。
