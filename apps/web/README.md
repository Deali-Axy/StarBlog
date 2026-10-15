# StarBlog Web

公开博客前台，Next.js 16 App Router，调用 `apps/api` 的 `/api/v1` 契约。

## 本地开发

先启动 API（默认 `http://localhost:5039`），再启动本应用：

```bash
task api:dev
task web:dev
```

或在仓库根目录执行 `task dev`，会并行启动 API、Admin 和 Web。

Web 开发服务器：<http://localhost:3001>

复制 [`.env.example`](.env.example) 为 `.env.local` 可覆盖站点名和后端地址。Next 会把 `/api`、`/media`、`/feed` 代理到 API。

## 语言

页头的 `EN / 中文` 按钮切换全站中英文。默认中文，偏好保存在 `starblog.locale` Cookie 中一年；切换时重新加载当前页，保留筛选条件与锚点。服务端和客户端使用同一偏好，页面标题、导航、表单和提示文案随之切换。

英文模式下，首页推荐、文章列表、搜索和相关文章优先展示已有英文译文，详情页同步显示译文标题、摘要、正文和目录。缺少译文时回退原文。详情页的 `?lang=en` 可单独指定文章语言，`?lang=original` 强制查看原文；两者都不修改全站偏好，再次切换全站语言时清除单篇覆盖。分类名称、照片说明、评论和友链等没有译文的数据保留原内容，Atom 源仍提供原文。

界面词典集中在 [`lib/i18n.ts`](lib/i18n.ts)，英文词典由 TypeScript 检查是否与中文保持一致。服务端调用 `getI18n()`，客户端调用现有 `AppProviders` 导出的 `useI18n()`；无需额外依赖或语言路由。Cookie 使页面按请求渲染，API 数据仍按语言分别缓存 5 分钟。

自定义站点介绍可配置 `NEXT_PUBLIC_SITE_DESCRIPTION_EN`、`NEXT_PUBLIC_AUTHOR_TITLE_EN`、`NEXT_PUBLIC_AUTHOR_BIO_EN`、`NEXT_PUBLIC_LAB_NAME_EN` 和 `NEXT_PUBLIC_LAB_DESCRIPTION_EN`。未配置英文时保留对应的自定义原文；没有自定义值时使用内置中英文介绍。

语言切换的浏览器回归测试位于 `tests/StarBlog.E2ETests/LanguageNavigationTests.cs`，启动 Web 后设置 `E2E_BASE_URL` 再运行。可用 `E2E_CHROMIUM_EXECUTABLE_PATH` 指定已安装的 Chromium；未设置时使用 Playwright 自带的浏览器。
