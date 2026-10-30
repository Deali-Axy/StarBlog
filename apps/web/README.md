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

## 首页布局

首页单独使用 `components/home/home-layout.tsx` 与 Tailwind 工具类：桌面端以固定在视口内的侧栏展示编号索引、搜索及主题和语言工具，小屏直接展开导航入口。独立的暖纸色 / 深色主题与页脚只作用于 `/`，保留现有首屏、推荐内容和分类展示；`links`、`about`、搜索和 RSS 等页面继续使用通用 `SiteHeader` / `SiteFooter`，博客与摄影仍使用各自布局。

右下角「自定义首页」菜单独立管理两组选项：内容区的「刊物内页 / 个人书桌」，以及精选分类的五种布局。分类区内不再展示切换条；默认分类继续读取 `FEATURED_CATEGORIES_VARIANT`（当前为 `accordion`），旧 `FEATURED_CATEGORIES_PREVIEW` 开关不影响新的自定义菜单。Hero、侧栏和 footer 不随内容风格切换。

`?hv=editorial` 与 `?hv=desk` 直接指定内容风格，`?fc=deck|rail|catalog|accordion|tilt` 指定分类风格；有效 URL 参数优先于已保存偏好，非法值回退到偏好或配置默认值。用户通过菜单选择后，两个值一起写入 URL，并保存到 `starblog.home-design` Cookie 一年；刷新、离开首页再返回、切换语言均保留偏好。切换使用原生 history，不重新请求首页数据，保留其他查询参数、锚点与已展开的文章摘要，支持浏览器前进后退。

内容组件复用真实文章、照片与友链数据，通过 `useHomeDesign().variant` 选择组件中的 Tailwind 类组合来区分构图。刊物版采用主次选文、年份索引、接触印样和往来簿；书桌版采用选题卡、撕页手记、相纸和地址卡。最新文章标题直接进入全文，旁边按钮独立展开摘要；摄影保留原图画幅并提供加载失败提示。分类布局组件独立注册于 `featured-categories/index.tsx`，内容风格切换不会重置当前分类布局。

推荐文章展示 API 返回的全部推荐文章，不在前端限制为三篇。精选摄影展示 API 返回的全部推荐照片，前端不再截取前四张。两种风格均按可用宽度自动排列：刊物版卡片最小宽度约 160px，书桌版约 176px；宽屏增加列数，窄屏自然换行，单张照片保留较大的展示尺寸。缩略图使用懒加载与响应式尺寸选择。展示总数取决于后台设置的推荐照片数量。

新增内容风格时，在 `lib/home-variant.ts` 的 `HOME_VARIANT_OPTIONS` 注册 ID 和中英文名称，并在首页 section 组件中增加对应 Tailwind 类组合；默认值由 `HOME_VARIANT` 控制，类型、菜单及 URL 解析会跟随注册表更新。若新风格需要不同的内容结构，可在现有 section 组件中按 `useHomeDesign().variant` 选择结构。`pnpm test:home` 检查偏好优先级、非法参数回退、独立分类选择、URL 保留、年份分组，以及两种风格下完整渲染推荐文章和照片。

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
