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

首页的服务端入口负责取数，再由 `HomeDesignProvider` 包住整个 `HomeLayout`，统一协调导航、Hero、内容、页脚和自定义面板。其他页面继续使用各自外壳。右下角「自定义首页」按整体方案、外观、页面布局、首尾、内容模块分组；选择即时预览，应用后统一保存，取消、关闭或 Escape 丢弃草稿。「撤销本次修改」恢复本次打开时的配置。每项支持恢复方案默认并标记自定义值；切换预设清空旧覆盖。

「刊物内页」延续编号侧栏、大字封面与刊物排版；「个人书桌」使用顶部紧凑导航、极简签名、联系名片和散落相纸。框架与导航按注册表联动调整，避免不兼容组合。Hero 另提供全幅摄影；当前摄影不可用或加载失败时暂以签名展示，所选配置保持不变。推荐文章、最近文章、兴趣词云、分类、照片和友链均可独立选择变体。推荐文章与照片完整呈现 API 返回的数据；最新文章继续展示前八篇，标题进入全文，旁边按钮独立展开摘要。

四套首页配色为暖纸、冷灰、森林、墨色，各有浅深版本。字体、密度、字号、内容宽度、纹理、边框与阴影均可调整；高级调色分别保存浅深模式的主色、背景色、文字色，并派生其余语义颜色。切换配色清除手动颜色覆盖。首页主题仅作用于 `/` 及其面板 Portal；昼夜模式沿用 `next-themes` 的全站共享设置，面板中先局部预览，应用后才更新全站模式。预设和分享不改变接收者的昼夜模式。

偏好采用 `{ version: 2, preset, overrides }`，只保存不同于方案默认值的覆盖。`lib/home-variant.ts` 管理类型、注册表、预设、白名单解析、关联调整和序列化；`lib/home-theme.ts` 管理配色与运行时变量，首页 CSS Module 管理作用域与框架。新增变体时注册中英文名称、默认值与兼容关系，再接入组件映射或共享 JSX。主题模式不重复存入首页配置。

新的 `?hc=v2.<Base64URL JSON>` 携带独立配置，优先于本机偏好且不混入接收者 Cookie。旧 `?hv=editorial|desk`、`?fc=deck|rail|catalog|accordion|tilt` 和旧 Cookie 继续可读；旧 desk 迁移到升级后的完整书桌方案。损坏、未知版本、非法字段与颜色安全回退。应用后保存到 `starblog.home-design` Cookie 一年；每次首页配置应用最多新增一条原生 history 记录，保留其他查询参数、锚点与文章摘要状态，不重新获取首页数据。刷新、跨页面返回和切换语言均保留已应用的配置。

分享按钮复制当前预览，不自动保存；接收者可直接应用保存。剪贴板不可用时提供可选中的链接文本。配置、模式和每次打开面板的草稿分别管理，前进后退恢复对应历史中的首页设计。

`pnpm test:home` 覆盖配置迁移、覆盖与恢复、注册选项往返、框架导航兼容、非法配置、配色派生、分享与 URL、三种 Hero 及推荐数据完整渲染。浏览器回归见 `HomeCustomizationTests`，启动 API 后通过 `INTERNAL_API_BASE_URL` 将 Web 指向该实例，再设置 `E2E_BASE_URL`、`E2E_CHROMIUM_EXECUTABLE_PATH` 运行测试；可用 `E2E_HOME_SCREENSHOT_DIR` 保存响应式截图。

## 博客列表风格预览

`/blog` 提供活字索引、折页手记、档案工作台三套独立构图，分别使用 `?bv=library`、`?bv=editorial`、`?bv=studio` 直接预览。右下角悬浮按钮打开风格预览菜单，不占用页头空间；切换会保留查询、分页及显示设置，URL 支持刷新与前进后退。

默认风格与预览开关集中在 `lib/blog-variant.ts`。选定后修改 `BLOG_VARIANT` 并将 `BLOG_PREVIEW` 设为 `false`；此时隐藏切换器，固定使用配置值并忽略 URL 中的 `bv`。三版拥有各自的导航、详情页与页脚，覆盖 `/blog` 列表、文章详情与 `/p` 旧链接。主题仅应用于博客外壳及其弹层，离开博客后恢复对应站点区域的布局。

活字索引采用全宽文字条目和展开式分栏目录；折页手记采用不对称跨栏排版和独立目录页；档案工作台采用目录、文章索引和摘要预览三栏，小屏选择文章后打开预览对话框。档案索引支持 ↑ / ↓ / Home / End 键选择，预览不会改变 URL 或文章顺序。

`SiteShell` 按路由接入 `BlogLayout`，三套 Header/Footer 分别位于 `index-chrome.tsx`、`folio-chrome.tsx`、`archive-chrome.tsx`。风格上下文与主内容保持稳定，切换外壳不会重置分类展开状态。博客内部文章、相关文章、语言链接和随机文章导航会保留当前 `bv`，canonical 与文章永久链接仍使用原始地址。

保留既有 `bv` 标识以兼容已保存的地址。`view=grid` 表示各设计的展开阅读布局，`view=list` 表示紧凑索引；不强制三版使用同一种卡片网格。

分类默认折叠，直达子分类会展开祖先路径。分类名称搜索临时展示匹配路径，清空后恢复原来的展开状态；前两版在所有尺寸通过目录弹层访问，工作台仅在桌面常驻目录。纯查询与分类逻辑检查使用 `pnpm test:blog`，浏览器回归说明见仓库 `tests/StarBlog.E2ETests/README.md`。

## 文章阅读页

正文 Markdown 由 Next.js 服务端的 `renderArticleMarkdown()` 解析，使用现有 `react-markdown`、`remark-gfm`、`rehype-slug` 和 `rehype-highlight`。同一次 rehype 处理在生成标题 ID 后提取目录，包含实际标题层级、中文、行内代码和重复标题的唯一 ID；纯标点标题补充无冲突的 ID。正文 React 节点和目录数据传入客户端布局，原始 Markdown、高亮和解析器不进入文章页客户端包。模块用 `server-only` 防止意外跨入客户端。

Web 读取文章时携带 `includeToc=false`，API 返回原始 Markdown 并跳过 Markdig 目录生成。API 的开关默认仍为 `true`，兼容其他客户端；后端发布渠道使用的 HTML 渲染继续保留。译文目录直接来自最终选中的译文正文。

详情页构图集中在 `components/blog/article-designs.module.css`：活字索引使用全宽大标题、左侧编号目录和右侧正文；折页手记使用居中刊物封面、衬线正文与右侧页边目录；档案工作台使用左侧章节树、中间文档纸面和右侧文档信息。小屏改为可展开的目录，选择章节后自动收起。共享文章树保持稳定，预览风格切换不重置评论草稿。

目录使用真实标题 ID 的原生锚点链接，支持中文编码、直接打开锚点、刷新与浏览器前进后退。点击后滚动并移动焦点至标题，偏移跟随各风格固定页头的高度。滚动高亮涵盖标题间的长段落和最后一节；图片载入或窗口尺寸改变后重新计算。目录同时显示正文阅读进度，尊重减少动态效果偏好。代码和表格在局部横向滚动，避免撑破手机页面。

`pnpm test:blog` 还会执行 `tests/article-reading.test.mjs`，覆盖实际 Markdown 渲染后的目录与标题 ID 一致性、重复和空 ID、标题层级、译文、GFM、脚注、代码高亮、安全链接及滚动定位逻辑。浏览器用的 `prepare_blog_fixture.py` 另提供 `/blog/article-reading-fixture`，包含十一种标题、长正文和英文译文，供检查布局、目录跳转与语言切换。

## 搜索页

`/search` 使用站点通用明暗主题，以搜索主视觉、真实分类入口、最近更新和随机阅读组成发现页。输入关键词后显示标题与正文的匹配摘要，保留 API 的相关度排序，每页 12 篇；分页地址保留 `keyword`，新查询回到第一页。搜索接口未提供总数，因此仅显示本页数量，满页时提供下一页，越界页可返回第一页。

搜索框支持 `/` 聚焦、`Escape` 清空、回车提交及加载反馈。分类和最近文章独立降级，搜索失败显示重试入口；所有新增文案纳入中英文词典。摘要仅将 API 的 `<mark>` 分隔符渲染为高亮，其余内容作为文本处理，并移除常见 Markdown 链接和标题符号。

## 样式约定

组件样式使用 Tailwind CSS v4，字号、间距和圆角优先使用标准工具类。博客三版的独立构图与响应式排版集中在 `components/blog/blog-designs.module.css`，样式通过 CSS Modules 限定作用域。复杂布局、流体字号和主题变量可使用任意值；条件样式通过 `cn()` 组合完整类名，不动态拼接工具类名称。自定义响应式断点与 Tailwind 默认断点统一使用 `rem`，避免混用 `px` 导致生成顺序与预期不符；例如首页桌面断点使用 `min-[70rem]`。

`app/globals.css` 仅保留 Tailwind/shadcn 导入、明暗主题变量、字体及圆角映射、基础层和 Marquee 动画注册。公共容器、标题、摄影主题等工具类组合集中在 `lib/styles.ts`；摄影主题必须同时应用于页面容器和 Portal 弹出层。Markdown 在服务端入口引入 highlight.js 官方主题，正文排版使用 `markdown.module.css`，各阅读方案通过局部 CSS Module 调整字体与版式。

内联 `style` 仅用于运行时值，例如摄影布局坐标、Motion 动画、分类权重字号及层级缩进。Markdown 表格的对齐由正文数据决定，也保留为运行时样式。浏览器测试使用语义或 `data-*` 定位，不依赖样式类。
