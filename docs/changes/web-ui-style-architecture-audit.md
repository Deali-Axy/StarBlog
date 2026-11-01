# Web 可切换 UI 风格实现梳理

日期：2026-11-01。代码基线：`2d230d2`，包含文章详情 Tailwind 迁移。

## 范围与结论

检查 `apps/web` 的路由、风格配置、切换入口、Context、页面组件、Tailwind 条件类及 CSS Module 的父级 / `data-*` 选择器。本次为静态代码审计，记录当前结构与后续拆分建议，不包含布局重构或视觉重新设计。

目前有三组与页面布局有关的切换：

| 入口 | 选项 / 状态 | 实现入口 |
| --- | --- | --- |
| 首页 `/` 自定义面板 | “刊物内页 / 个人书桌”预设，以及可独立覆盖的框架、导航、Hero、页脚、六个内容区块和主题参数 | `components/home/home-design-switcher.tsx`、`home-design-context.tsx`、`lib/home-variant.ts` |
| 博客列表和文章详情 | `bv=library/editorial/studio`；列表另有 `view=grid/list` | `components/blog/blog-variant-switcher.tsx`、`blog-layout.tsx`、`lib/blog-variant.ts` |
| 摄影列表 `/photography` | 瀑布流 / 等高行，三档密度 | `components/photography/photo-studio.tsx`、`lib/photography/layout.ts`、`justify.ts` |

**最集中的问题是文章详情，以及首页五个内容区块和页脚。** 博客分类目录弹层、移动导航也存在共享结构配合样式切换的情况。博客列表和博客页头 / 页脚已有独立组件，但内部共享 UI 仍受父级风格样式影响。

这里的“共享结构切换样式”包括条件 Tailwind classes、集中 classes 配置、CSS Module 父级选择器、`data-*` 选择器和条件内联样式，不仅限于显式的 `className={variant ? ... : ...}`。

拆分优先级依据布局结构和维护耦合度确定。共享数据、交互状态、基础控件、主题颜色本身不等于页面布局被合并；无需为了独立构图复制它们。

## 明确存在共享结构切换样式的界面

下表路径均相对于 `apps/web`。

| 界面 / 范围 | 可切换方案 | 当前实现与证据 | 拆分建议 |
| --- | --- | --- | --- |
| 文章详情 `/blog/[id]`，包含标题、署名、正文区域、桌面 / 手机目录、右侧文档信息 | 活字索引 / 折页手记 / 档案工作台 | `components/blog/blog-post-renderer.tsx:35` 读取 `variant`，使用同一棵 JSX；`article-designs.ts` 为二十多个位置配置 classes，改变栅格、显隐、字体及间距；`sidebar-toc.tsx:12` 读取同一配置。路由统一渲染 `BlogPostRenderer` | **优先**拆成三个文章布局，分别组合标题区、阅读区和导航；共享服务端 Markdown、标题 ID、目录定位、分享、评论状态。目录呈现由布局选择，定位逻辑继续共享 |
| 首页精选文章 `FeaturedPosts` | 刊物 `editorial` / 便笺 `notes` | `components/home/featured-sections.tsx:40` 用 `desk` 为同一组 article / Link 切换跨栏、字号、卡片边框、倾斜、摘要行数等 | **优先**拆成刊物文章编排和便笺文章编排两个展示组件，共用文章数据与链接能力 |
| 首页最近文章 `LatestPosts` | 时间索引 `timeline` / 手记 `ledger` | `featured-sections.tsx:17` 共用年份分组与列表，条件 classes 改变年份位置、列数、字号；`latest-post-row.tsx:18` 共用行结构，`ledger` 主要增加虚线边框 | 拆分两套年份 / 条目排版；将分组和摘要展开状态保留在共享数据 / 状态层 |
| 首页兴趣分类 `CategoryWordCloud` | 词云 `cloud` / 标签 `tags` | `components/home/category-word-cloud.tsx:12` 共用 flex 和分类链接，切换边框、胶囊背景和内联字号；词云按权重计算字号，标签为固定字号 | 拆成词云和标签集合两个展示组件；分类权重和分类导航继续共享 |
| 首页精选摄影 `FeaturedPhotos`、`PhotoPrint` | 接触印样 `contactSheet` / 散落相纸 `prints` | `components/home/featured-photos.tsx:16`、`:37` 共用图片循环和 figure；条件 classes 切换网格、背景、相纸边框、旋转、图片高度及说明间距 | **优先**拆分接触印样与散落相纸的编排；保留图片加载失败处理和媒体地址等公共能力 |
| 首页友链 `FriendLinks` | 目录 `directory` / 名片 `cards` | `components/home/friend-links.tsx:16` 共用 ul / li / a，通过条件 classes 改变边框、背景、间距、首字母显隐和域名位置 | 拆分目录条目与名片展示，保留域名提取和链接数据 |
| 首页页脚 `HomeFooter` | 刊物落款 `colophon` / 联系名片 `contact` | `components/home/home-layout.tsx:80` 共用 footer / div，`contact` 切换卡片外观、标题和简介，并条件插入社交链接 | 拆成两个页脚组合组件，分别拥有布局；共用作者资料与回到顶部能力 |
| 博客分类目录弹层 `BlogDirectory` | 活字宽目录 / 折页目录页 / 工作台抽屉 | `components/blog/blog-design-shared.tsx:46` 共用 Dialog / CategoryTree；`:58` 设置 `data-directory-layout=wide/sheet/drawer`；`blog-designs.module.css:8` 起通过 CSS 改成三列、两列或左侧抽屉 | **优先**拆分三套目录容器和触发器组合；分类搜索、展开状态、导航链接继续共享。工作台桌面常驻目录也应由其布局拥有 |
| 博客移动导航 `BlogMobileNav` | 活字索引菜单 / 档案工作台侧栏 | `components/blog/blog-chrome-shared.tsx:56` 共用 Dialog 和链接列表；`data-blog-menu` 配合 `blog-chrome.module.css:84` 将浮层改为右侧抽屉，并改变字号 / 间距 | 拆分索引菜单和工作台抽屉的展示层。折页页头调用 `BlogHeaderTools menu={false}`，目前没有第三套移动菜单；不能把它算成三套菜单 |

## 部分拆分、仍需明确边界的界面

| 界面 | 当前结构 | 后续处理 |
| --- | --- | --- |
| 首页整体框架 / 导航 | `HomeLayout` 只有一个 frame，通过 `styles.sidebar` 改变外层栅格；内部根据映射选择独立的 `NumberedNavigation` / `CompactNavigation`。`Tools` 还有 `sidebar` 条件 classes | 属于混合实现。需要更独立的页面框架时，可拆成 Sidebar / Topbar 布局，再按配置组合导航；无须把已经分开的导航重复拆分 |
| 首页 Hero | `CoverHero` 已独立；极简签名和全幅摄影共用 `SignatureHero`，摄影通过 children 插入 `HeroPhoto fullWidth`，失败时回退签名。`home-hero.tsx:152` 负责选择 | 共用签名段并非只换 class，但“全幅摄影”还没有独立 Hero 组合组件。可以新增 PhotographHero，明确其组合与回退边界 |
| 博客搜索 / 排序 / 筛选 / 结果与分页 | `ControlPanel`、`BlogFilters`、`BlogResults`、`PaginationNav` 共用结构。`blog-designs.module.css` 的 `.folio .tools`、`.archiveCenter .tools/.results/.resultBody` 等父级选择器会改变排列、滚动区域和分页布局；`CategoryTree` 也直接读取 `variant` 改变外观 | 三套列表页面已分开，但这些部件仍有展示耦合。后续让各列表布局拥有工具栏、结果区和分页组合，公共层保留查询 / 分页 / 分类行为；仅改变字体、圆角的小控件无需全部复制 |
| 博客导航链接 / Wordmark 等子组件 | `BlogSiteNav` 接受 `className` / `numbered`；`BlogWordmark`、`BlogFooterNav` 等接受布局传来的 class，CSS 在三套 chrome 下定制 | 这是独立页头 / 页脚内部的小型展示复用，不能据此把整个页头 / 页脚判定为共享页面。只在具体结构产生明显分歧时拆分 |

## 其他可切换视图与工具

| 项目 | 当前实现 | 与本次问题的关系 |
| --- | --- | --- |
| 博客展开 / 紧凑视图 `view=grid/list` | `index-design.tsx:18` 使用 `indexCompact`，`folio-design.tsx:28` 使用 `folioList`，`archive-design.tsx:86` 使用 `archiveCompact`；各自保留同一文章循环，CSS 改列数、排版或隐藏摘要 | **确实也是共享结构切换 class**，但它是每套设计内部的显示模式，和三套品牌风格不同。若目标也要求这两种视图独立演进，应拆出各自的 Expanded / Compact 展示组件；目前列为次级梳理项 |
| 摄影瀑布流 / 等高行 | `photo-studio.tsx:260` 的 `PhotoWall` 共用图片节点；`:289` 在 `layoutMasonry()` / `layoutJustified()` 之间切换，坐标和尺寸来自不同算法；密度改变列数或目标行高 | **共用渲染器，但不是仅更换 classes**。可以独立算法 / 展示模块，优先级低于前述页面；无需复制图片、分页、灯箱和滚动恢复逻辑 |
| 博客风格预览按钮 / 菜单 | `blog-variant-switcher.tsx` 共用 Radix 菜单，`blog-preview.module.css` 根据 `data-variant` 改圆角、背景、阴影 | 属于共用工具的外观适配，已记录；一般不需要为每套风格复制同一个设置工具 |
| 首页自定义面板 | `HomeDesignSwitcher` 共用设置界面，使用当前主题变量预览颜色和字体；控制项切换的是首页各布局配置 | 本身没有多套独立布局方案。保留统一编辑器即可 |

## 已有独立组件的部分

- 博客三套列表：`IndexDesign`、`FolioDesign`、`ArchiveDesign`，由 `blog-explorer.tsx:56` 选择。工作台还有自己的选中文章、键盘导航和预览交互，不只是同一列表换皮。
- 博客三套页头 / 页脚：`IndexHeader/Footer`、`FolioHeader/Footer`、`ArchiveHeader/Footer`，由 `blog-layout.tsx:15` 的映射选择。公共 CSS 文件并不意味着共用同一页面结构。
- 首页两套导航：`NumberedNavigation`、`CompactNavigation`，在 `home-layout.tsx:51` / `:71` 定义并通过映射选择。
- 首页大字封面 `CoverHero` 已有独立结构。
- 首页精选分类的五个布局：`DeckVariant`、`RailVariant`、`CatalogVariant`、`AccordionVariant`、`TiltVariant`。`featured-categories/index.tsx:18` 通过组件映射选择；它们是独立文件与交互。虽然旧 `FEATURED_CATEGORIES_PREVIEW` 常量为 false，目前统一首页自定义面板仍注册了全部五个选项，不能按旧开关判定不可切换。

## 路由与共享能力的边界

首页路由统一获取数据并渲染同一组区块；`HOME_PRESETS` 是配置组合，不是两套独立的首页页面组件。首页 `hc`（兼容 `hv` / `fc`）、Cookie 和预览草稿由 `HomeDesignProvider` 管理，区块通过 `config` 选择展示。

博客路由统一获取文章并在服务端解析 Markdown；`bv` 由客户端 `BlogLayout` 解析并通过 Context 下发，切换器更新原生 history。独立组件选择发生在展示层，当前没有按风格切换 route 的 import。

后续拆分应保留现有 URL、Cookie、翻译、锚点和浏览器历史语义。尤其要把评论草稿、分类展开 / 搜索、文章摘要展开等需跨风格保留的状态放在稳定的上层或共享状态容器，避免切换组件类型导致状态重置。公共数据、交互 hook、基础控件可继续共享；大型布局及其主要编排 classes 应由各自展示组件拥有。

`/p/[slug]` 是保留博客风格的旧地址重定向，没有另一套详情实现；`/blog/random`、`/photography/random` 也不是独立风格页面。`/about`、`/links`、`/rss`、`/search` 和摄影详情未发现类似 `bv` / 首页预设的多风格页面入口。通用明暗主题、摄影固定主题、Button / Badge 的常规 variants、按文章数据决定的 featured 卡片不计为可切换页面风格。

## 建议顺序

1. 文章详情三套布局及目录呈现。
2. 首页精选文章、最近文章、兴趣分类、摄影、友链、页脚；保留现有逐区块覆盖能力。
3. 博客分类目录弹层、移动菜单，以及工具栏 / 分页的展示耦合。
4. 首页 frame、摄影 Hero 的组合边界；按实际需要决定展开 / 紧凑视图和摄影渲染器是否进一步独立。

本次只完成分类提交与结构梳理。以上拆分均为后续建议，尚未实施。
