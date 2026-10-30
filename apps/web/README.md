# StarBlog Web

公开博客前台，Next.js 16 App Router，调用 `apps/api` 的 `/api/v1` 契约。

## 本地开发

先启动 API（默认 `http://localhost:5039`），再启动本应用：

```bash
task api:dev
task web:dev
```

或在仓库根目录执行 `task dev`，会通过 goreman 同时启动 API、Admin 和 Web。

Web 开发服务器：<http://localhost:3001>

复制 [`.env.example`](.env.example) 为 `.env.local` 可覆盖站点名和后端地址。Next 会把 `/api`、`/media`、`/feed` 代理到 API。

## 语言

页头的 `EN / 中文` 按钮切换全站中英文。默认中文，偏好保存在 `starblog.locale` Cookie 中一年；切换时重新加载当前页，保留筛选条件与锚点。服务端和客户端使用同一偏好，页面标题、导航、表单和提示文案随之切换。

英文模式下，首页推荐、文章列表、搜索和相关文章优先展示已有英文译文，详情页同步显示译文标题、摘要、正文和目录。缺少译文时回退原文。详情页的 `?lang=en` 可单独指定文章语言，`?lang=original` 强制查看原文；两者都不修改全站偏好，再次切换全站语言时清除单篇覆盖。分类名称、照片说明、评论和友链等没有译文的数据保留原内容，Atom 源仍提供原文。

界面词典集中在 [`lib/i18n.ts`](lib/i18n.ts)，英文词典由 TypeScript 检查是否与中文保持一致。服务端调用 `getI18n()`，客户端调用现有 `AppProviders` 导出的 `useI18n()`；无需额外依赖或语言路由。Cookie 使页面按请求渲染，API 数据仍按语言分别缓存 5 分钟。

自定义站点介绍可配置 `NEXT_PUBLIC_SITE_DESCRIPTION_EN`、`NEXT_PUBLIC_AUTHOR_TITLE_EN`、`NEXT_PUBLIC_AUTHOR_BIO_EN`、`NEXT_PUBLIC_LAB_NAME_EN` 和 `NEXT_PUBLIC_LAB_DESCRIPTION_EN`。未配置英文时保留对应的自定义原文；没有自定义值时使用内置中英文介绍。

语言切换的浏览器回归测试位于 `tests/StarBlog.E2ETests/LanguageNavigationTests.cs`，启动 Web 后设置 `E2E_BASE_URL` 再运行。可用 `E2E_CHROMIUM_EXECUTABLE_PATH` 指定已安装的 Chromium；未设置时使用 Playwright 自带的浏览器。

## 博客列表风格预览

`/blog` 提供活字索引、折页手记、档案工作台三套独立构图，分别使用 `?bv=library`、`?bv=editorial`、`?bv=studio` 直接预览。右下角悬浮按钮打开风格预览菜单，不占用页头空间；切换会保留查询、分页及显示设置，URL 支持刷新与前进后退。

默认风格与预览开关集中在 `lib/blog-variant.ts`。选定后修改 `BLOG_VARIANT` 并将 `BLOG_PREVIEW` 设为 `false`；此时隐藏切换器，固定使用配置值并忽略 URL 中的 `bv`。三版拥有各自的导航与页脚，覆盖 `/blog` 列表、文章详情与 `/p` 旧链接；详情正文排版保持原有结构。主题仅应用于博客外壳及其弹层，离开博客后恢复对应站点区域的布局。

活字索引采用全宽文字条目和展开式分栏目录；折页手记采用不对称跨栏排版和独立目录页；档案工作台采用目录、文章索引和摘要预览三栏，小屏选择文章后打开预览对话框。档案索引支持 ↑ / ↓ / Home / End 键选择，预览不会改变 URL 或文章顺序。

`SiteShell` 按路由接入 `BlogLayout`，三套 Header/Footer 分别位于 `index-chrome.tsx`、`folio-chrome.tsx`、`archive-chrome.tsx`。风格上下文与主内容保持稳定，切换外壳不会重置分类展开状态。博客内部文章、相关文章、语言链接和随机文章导航会保留当前 `bv`，canonical 与文章永久链接仍使用原始地址。

保留既有 `bv` 标识以兼容已保存的地址。`view=grid` 表示各设计的展开阅读布局，`view=list` 表示紧凑索引；不强制三版使用同一种卡片网格。

分类默认折叠，直达子分类会展开祖先路径。分类名称搜索临时展示匹配路径，清空后恢复原来的展开状态；前两版在所有尺寸通过目录弹层访问，工作台仅在桌面常驻目录。纯查询与分类逻辑检查使用 `pnpm test:blog`，浏览器回归说明见仓库 `tests/StarBlog.E2ETests/README.md`。

## 样式约定

组件样式使用 Tailwind CSS v4，字号、间距和圆角优先使用标准工具类。博客三版的独立构图与响应式排版集中在 `components/blog/blog-designs.module.css`，样式通过 CSS Modules 限定作用域。复杂布局、流体字号和主题变量可使用任意值；条件样式通过 `cn()` 组合完整类名，不动态拼接工具类名称。

`app/globals.css` 仅保留 Tailwind/shadcn 导入、明暗主题变量、字体及圆角映射、基础层和 Marquee 动画注册。公共容器、标题、摄影主题等工具类组合集中在 `lib/styles.ts`；摄影主题必须同时应用于页面容器和 Portal 弹出层。Markdown 在组件入口引入 highlight.js 官方主题，其排版使用局部 Tailwind 类。

内联 `style` 仅用于运行时值，例如摄影布局坐标、Motion 动画、分类权重字号及层级缩进。Markdown 表格的对齐由正文数据决定，也保留为运行时样式。浏览器测试使用语义或 `data-*` 定位，不依赖样式类。
