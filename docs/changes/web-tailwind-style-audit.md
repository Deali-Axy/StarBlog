# apps/web Tailwind 样式审计

日期：2026-11-18。业务源码基线：`e46a372`。本机安装及锁定的 Tailwind 为 **4.3.3**。

## 结论

存在项目级的样式迁移遗留和可维护性问题，博客区域最集中。大量 CSS 声明被直接转换为任意属性，普通工具类、任意属性和任意选择器混用；部分独立视图还保留了“完整基础规则 + 高优先级覆盖”的编排方式。

不能把所有方括号都当成违规：任意属性、任意值和任意变体都是 Tailwind 官方支持的语法。真正需要整改的是已有标准工具类却继续使用冗长声明、重复的设计数值与断点、父节点对后代结构的过度依赖，以及优先级叠加。流体字号、运行时变量、精确的不等宽网格通常需要保留任意值。

本次只做扫描、编译核对和报告，不修改业务样式或视觉设计。

## 范围与统计口径

- 业务源文件 **242** 个：`app` 26、`components` 183、`features` 3、`lib` 30。遍历了 `apps/web` 的文件，并核对配置、样式约定和架构检查；依赖、构建产物、测试字符串、脚本和文档不进入业务类名统计。
- CSS 文件全部核对：`app/globals.css`、`components/home/home-design.module.css`、`components/blog/article-native.module.css`。
- 使用 TypeScript AST 提取静态字符串及模板的静态片段，再用已安装的 Tailwind、项目 `globals.css` 和其导入解析、编译候选类名。统计按**源码出现次数**，不是运行时 DOM 数，也不是违规总数。
- 不做跨函数数据流分析；扫描所有业务源码字符串，极少数非样式字符串理论上也可能成为有效候选。动态内联样式、模板、CSS 文件另行核对。未将任意字符串的编译成功等同于浏览器行为正确。
- `className` 长度是 AST 中初始化表达式的源码字符数，包含表达式标点，未展开 `cn()` 或引用变量。

| 指标 | 数量 |
| --- | ---: |
| 含方括号的有效候选类名出现次数 | 1,814 |
| 含方括号的不同完整类名 | 968 |
| 涉及源文件 | 111 |
| 任意属性，如 `[padding:…]` | 496 |
| 工具类任意值，如 `text-[11px]` | 463 |
| 自定义变量声明，如 `[--background:…]` | 111 |
| 工具类本体为普通工具类、方括号位于变体 | 744 |
| 任意 min/max 断点使用次数 | 375 |
| `!` 类名使用次数（含无方括号类名） | 51 |
| 带 `&&` 的选择器使用次数 | 59 |
| `className` 属性 | 1,817 |
| 超过 500 字符的 `className` 表达式 | 19 |
| JSX `style` 属性 | 97 |

前四类互斥，合计 1,814；断点、选择器、important 与这些分类交叉，不能相加。候选类名编译核对中，明确样式上下文里含方括号或 `!` 的静态片段没有发现无法编译的候选；这不覆盖全部 CSS 语义或运行时样式。

## 发现与整改建议

### 1. 普通 CSS 属性直接搬进类名：优先整改

Git 提交 `7074785` 删除 `blog-designs.module.css` 时，将 `.previewDocument { padding: 22px 26px 32px; }` 直接转换成了现在的 `[padding:22px_26px_32px]`。当前见 `components/blog/archive-design.tsx:31`、`:111`。这是有提交证据的迁移遗留。

任意属性出现最多的包括：阴影 34、字距 34、padding 34、网格列 31、background 26、font 23、z-index 23、width 22、border-bottom 21、line-height 20。阴影和特殊布局并非都能消除，简单属性则应优先使用对应工具类。

| 当前写法 | 建议表达 | 保留条件与注意事项 |
| --- | --- | --- |
| `[inset:0]` | `inset-0` | 核对同一元素其他定位规则 |
| `[z-index:50]` | `z-50` | 保留原状态或响应式前缀 |
| `[animation:none]` | `animate-none` | 保留原 `motion-reduce:`、后代范围与 `!`，再单独评估重复覆盖 |
| `[transition:none]` | `transition-none` | 不顺带删除减少动态效果支持 |
| `[scroll-behavior:auto]` | `scroll-auto` | 同上 |
| `[justify-content:flex-end]` | `justify-end` | 标准工具类已覆盖 |
| `[flex:1]` | `flex-1` | 已核对当前版本输出 `flex: 1` |
| `[line-height:2.2]` | `leading-[2.2]` | 保持无单位行高；`leading-2.2` 无法编译，并非等价类名 |
| `[padding:22px_26px_32px]` | `pt-5.5 px-6.5 pb-8` | 默认间距、根字号 16px、横向书写时数值一致；固定像素要求可用 `pt-[22px] px-[26px] pb-[32px]` |
| `[padding-block:24px_32px]` | `pbs-6 pbe-8` | 当前 4.3.3 编译可用；保留逻辑方向。严格固定像素可用 `pbs-[24px] pbe-[32px]` |
| `[grid-template-columns:30px_minmax(0,_1fr)]` | `grid-cols-[30px_minmax(0,1fr)]` | 不等宽列继续使用任意值 |
| `[font:600_29px/1.65_var(--font-heading)]` | `font-heading font-semibold text-[29px]/[1.65]` | 意图更清楚，但 `font` 简写还会重置其他字体属性，需核对继承和主题覆盖 |

示例位置：`archive-design.tsx:29`、`:45`、`:88`、`:116`，`views/archive-expanded-posts.tsx:13`，`components/home/home-design-switcher.tsx:147`。

“换成命名工具类”不自动等于完全相同的 CSS：`background` 与 `background-color`、`border` 简写与分解后的边框属性、逻辑方向与物理方向，都需要保留原语义。

### 2. 断点缺少命名，并残留旧式减一数值：优先统一

| 任意断点 | 出现次数 | 判断 |
| --- | ---: | --- |
| `max-[39.9375rem]` | 234 | 接近默认 `sm`，但并不等价 |
| `min-[70rem]` | 51 | 首页实际布局边界，适合命名 |
| `max-[63.9375rem]` | 46 | 接近默认 `lg`，但并不等价 |
| `max-[74.9375rem]` | 19 | 非默认断点，不能替换为 `max-xl` |
| `max-[23rem]` | 7 | 窄屏例外，评估是否合并 |
| `max-[39.999rem]` | 7 | 与 39.9375rem 并存，设计边界不一致 |
| `min-[56.25rem]` | 5 | 评估是否作为明确设计边界命名 |
| `min-[90rem]` | 2 | 宽屏例外 |
| `max-[359px]` / `min-[400px]` | 各 1 | 与主体 rem 断点混用 |
| `min-[40rem]` | 1 | 默认主题下可表达为 `sm:` |
| `min-[93.75rem]` | 1 | 宽屏例外 |

已对照当前编译器输出：

```css
/* max-[74.9375rem]:… */
@media (width < 74.9375rem) { … }
/* max-lg:… */
@media (width < 64rem) { … }
```

以 16px 为换算基准，74.9375rem 为 1199px，默认 `xl` 是 80rem，即 1280px；误换 `max-xl` 会改变约 81px 的范围。`max-[63.9375rem]` 与 `lg:` 若共同控制显隐，还可能在临界区间缺少覆盖；需要逐组件核对，不能只依赖整数宽度截图。

建议明确默认 `sm` / `lg` 与业务断点的职责；70rem 等反复出现的设计边界放进 `@theme` 的 `--breakpoint-*`。是否将 74.9375rem 规范为 75rem，属于边界调整，必须在整改时验证。所有默认和自定义宽度断点保持同一种单位。

另有 2 处 `[@media(min-width:1120px)_and_(max-height:740px)]`，位于 `components/home/views/numbered-navigation.tsx:10`、`:12`，同时使用 px 宽度与高度条件。高度条件有实际用途，保留它并统一宽度边界，而非改成普通宽度断点后丢失高度约束。

### 3. 字号、行高与圆角尺度未统一：需要设计层决策

`text-*` 任意值 196 次，覆盖 7px、8px、9px、10px、11px、13px、21px、22px、23px、25px、26px、27px，以及 rem 写法和 `clamp()`。其中 `text-[11px]` 32 次、`text-[.6875rem]` 31 次、`text-[.625rem]` 21 次、`text-[10px]` 19 次。11px 与 .6875rem 在根字号 16px 时相同，却采用两种表达。

23 处 `font` 任意简写还混入了字号和字体族。20 处 `[line-height:…]` 与 15 处 `leading-[…]` 并存，涉及 1.55、1.65、1.7、1.8、1.9、1.95、2.1、2.2 等值。

建议把重复且有产品含义的字号/行高组合提炼为少量 `--text-*` token，确有必要的一次性字号仍允许任意值；先统一表达，调整字号密度作为独立设计变更。不能把所有 13px 机械四舍五入到 `text-sm`。

圆角也不能机械归一：本项目覆写了 `--radius-*`，且支持主题。`rounded-[.5rem]` 不能直接假设等价于 `rounded-lg`。字号工具类还可能附带默认行高，替换时必须检查最终行高。

### 4. 子元素选择器、优先级覆盖和旧规则堆叠：重点结构问题

59 处类名使用 `&&`，51 处类名使用 `!`。它们合法，但有一部分是旧基础样式叠加新布局的结果。

- `components/blog/views/index-compact-posts.tsx:12`、`:13`：组件已经是独立紧凑视图，却先设置包含摘要列的网格，再通过 `[&&]`、`[&&_>_a]` 改列并隐藏摘要，继续保留基础视图各断点规则。
- `components/blog/views/archive-compact-posts.tsx:13`：保留展开视图的摘要排版和截断声明，随后用 `[&&_p]:hidden` 覆盖。
- `components/blog/views/archive-parts.tsx:87`：通过 `[&&_>_nav_>_div:first-child]` 等规则控制嵌套分页 DOM。分页内部层级变化会影响父组件布局。
- 多套页面、弹层及根容器重复给全部后代施加减少动态效果规则；不能直接删除，但可以明确负责该行为的作用域，减少重复声明。

建议让独立视图直接表达最终布局，把字体、间距和显隐规则尽量写到拥有对应节点的 JSX；只有渲染器输出、伪元素或确需整体协调的内容保留后代选择器。不要重新引入集中 classes 配置或用风格标识切换共享页面结构，这会违背现有架构约定。

重构时必须核对优先级：`&&` 规则的优先级高于单 `&` 规则，响应式媒体查询本身不会提高选择器优先级；把类名换成更短的形式，不能解决已有覆盖关系。

### 5. 工具类合并不能兜底任意属性冲突

`lib/utils.ts:4` 的 `cn()` 使用 `tailwind-merge`。已用当前安装版本验证：

```text
twMerge("[padding:22px_26px_32px] px-5")
=> "[padding:22px_26px_32px] px-5"

twMerge("[line-height:2.2] leading-normal")
=> "[line-height:2.2] leading-normal"

twMerge("px-4 px-5")
=> "px-5"
```

因此往末尾追加普通类不保证覆盖旧任意属性，需删除对应旧声明并核对生成 CSS。还要注意 `DialogOverlay` 已提供 `fixed inset-0 z-50 bg-black/45 backdrop-blur-[5px]`（`components/ui/dialog.tsx:18`），业务弹层有不少重复声明和任意属性覆盖，宜逐个明确控件默认与页面差异。

### 6. 交互变体简化存在行为差异

`[&:hover]` 出现 39 次。当前编译器将它直接生成 `:hover`；而 `hover:` 包在 `@media (hover: hover)` 中。替换会影响触屏设备的 hover 行为，应按产品期望核对，不能标记成无条件等价替换。

`[&[aria-busy=true]]`、`[&[data-selected=true]]` 等简单状态通常可以表达为 `aria-busy:`、`data-[selected=true]:`，但应保留准确的匹配值、后代范围和优先级。

### 7. 合理任意值和内联样式：保留并说明职责

- `components/blog/archive-design.tsx:88` 的 `min-h-[calc(100dvh_-_var(--blog-header-height))]`：高度依赖实际 DOM 测量；`blog-layout.tsx:28` 使用测量结果更新变量。它符合 Tailwind 用法，直接改成 `min-h-dvh` 会丢掉页头扣除。
- `grid-cols-[30px_minmax(0,1fr)]`：不等宽列确有必要；换成 `grid-cols-2` 会改变列宽。即使是 `1fr 1fr`，换成工具类的 `minmax(0,1fr)` 也可能改变最小尺寸与溢出行为。
- `clamp()`、安全区 `env()`、密度变量计算、原生伪元素和复杂的一次性阴影有保留理由。需要命名的是反复出现的设计意图，不是每个数字。
- 111 处自定义变量声明主要用于博客、首页和摄影主题，不应为了消除方括号而硬编码颜色。
- 97 处 JSX 内联样式中，74 处是 `zoom: var(--home-scale)`；其余主要是摄影布局测量、图片缩放旋转、Motion 值、分类层级缩进、主题变量和 Markdown 表格对齐。它们大多符合 README 的运行时样式约定。
- 另发现 `components/home/views/tag-interests.tsx:16` 的 `fontSize: \`${0.8125}rem\`` 实际是常量，可移入类名或字号 token；这不是运行时值。
- 唯一发现的 className 模板是 `home-design-context.tsx:88`，用于拼接 CSS Module 已生成的类名及固定 Tailwind 类；未发现这处模板动态构造 Tailwind 工具类名称。此结论不代表进行了全程序动态值分析。

## 配置与现有约束

`README.md:92` 已要求字号、间距和圆角优先使用标准工具类，现有代码没有完全落实。`eslint.config.mjs` 管理导入和展示层边界，`tests/ui-architecture.test.mjs` 管理布局选择与残留 CSS 的职责；两者都没有约束任意属性、重复断点或类名规范化。

全部 3 个 CSS 文件目前分别承载 Tailwind/token/基础层、首页主题与效果、浏览器原生伪元素，没有发现需要把大量页面布局从它们重新迁出的情况。问题主要已经在 JSX 中。只要求“不要写 CSS 文件”不能解决本次发现。

## 建议实施顺序与验证

1. **表达整理**：按属性清理显然已有工具类的声明，保留响应式、状态和可访问性行为；检查 `cn()` 调用处的冲突。先处理博客列表、导航、弹层，再处理首页及其他页面。
2. **局部结构整理**：独立紧凑视图直接定义最终布局；把依赖嵌套 DOM 的规则移到对应节点，逐步减少 `&&`、冗余 `!` 和失效基础规则。保留独立视图架构。
3. **设计尺度统一**：明确默认/业务断点、字号、行高及 token 命名，单独评审会改变数值和边界的调整。
4. **持续约束**：在明确允许清单后增加样式约定检查；可评估 Tailwind 官方工具的规范化建议，但不能自动接受所有 canonical 结果，也不建议全面禁用任意值。当前扫描脚本只输出清单，不充当 CI 失败门禁。

真正实施业务样式修改时，运行类型、lint、现有 home/blog/photography/utility/architecture 测试和生产构建；浏览器重点验证三套博客、首页布局、明暗/主题/缩放、弹层、减少动态效果、触屏 hover，以及断点两侧和临界小数宽度。仅通过构建不能证明清理前后视觉和交互等价。

本次审计脚本已实际执行并核对统计，另使用当前编译器验证了表中的行高、逻辑 padding、字体、网格、动画和断点示例，并验证 `tailwind-merge` 冲突行为。所有命中的源码行位置和统计加总均已校验，脚本通过 `node --check`。

全项目 ESLint 以 `node node_modules/eslint/bin/eslint.js .` 执行，结果为 0 错误、2 条现存警告，均为 `components/blog/article-cover.tsx:46`、`:49` 的图片 alt 属性检查。`pnpm lint` 因包管理器存储锁位于只读路径而未能启动，直接调用同一已安装 ESLint 完成了检查。没有修改业务代码，因此没有启动全套应用测试或浏览器视觉回归。

## 复现与完整位置清单

```sh
node apps/web/scripts/audit-tailwind.mjs > /tmp/starblog-web-tailwind-audit.json
```

脚本只读取项目并向标准输出写 JSON，不修改样式。JSON 包含每处命中的文件、行列、完整类名、任意属性/值、断点、选择器、important，以及长 className、内联样式、模板和待核对编译候选。脚本使用 Tailwind 的不稳定分析 API，升级 Tailwind 时应核对 API 和统计结果。`canonical` 是编译器建议，不是经过视觉验证的自动替换方案。

本次完整 JSON：`/tmp/starblog-web-tailwind-audit.json`。临时文件不受 Git 管理，可以用以上命令重新生成。

## 官方依据

- [任意值、属性和变体](https://tailwindcss.com/docs/adding-custom-styles)
- [响应式断点与 max 变体](https://tailwindcss.com/docs/responsive-design)
- [行高：无单位任意值与间距行高](https://tailwindcss.com/docs/line-height)
- [网格列：固定列与任意布局](https://tailwindcss.com/docs/grid-template-columns)
- [主题变量及设计 token](https://tailwindcss.com/docs/theme)

## 附录：按功能的分布

下表均为含方括号的有效类名出现次数，包含合理的任意值、主题变量和变体，不能直接视为违规数量。

| 功能 | 次数 |
| --- | ---: |
| `components/blog` | 1217 |
| `components/home` | 325 |
| `lib` | 118 |
| `components/ui` | 49 |
| `components/search` | 27 |
| `components/photography` | 26 |
| `components/links` | 13 |
| `components/about` | 11 |
| `components/rss` | 11 |
| `app` | 6 |
| `components/brand` | 6 |
| `components/layout` | 4 |
| `components/theme-toggle.tsx` | 1 |

## 附录：全部受影响源文件

路径相对于 `apps/web`；每处具体行列见 JSON 的 `occurrences`。

| 文件 | 次数 |
| --- | ---: |
| `components/blog/views/archive-parts.tsx` | 90 |
| `lib/blog-theme.ts` | 77 |
| `components/blog/views/index-compact-posts.tsx` | 74 |
| `components/blog/views/index-chrome-parts.tsx` | 69 |
| `components/home/home-design-switcher.tsx` | 67 |
| `components/blog/views/index-parts.tsx` | 66 |
| `components/blog/views/index-expanded-posts.tsx` | 60 |
| `components/blog/archive-design.tsx` | 58 |
| `components/blog/index-design.tsx` | 56 |
| `components/blog/views/folio-chrome-parts.tsx` | 55 |
| `components/blog/views/folio-parts.tsx` | 55 |
| `components/blog/index-chrome.tsx` | 50 |
| `components/blog/views/archive-chrome-parts.tsx` | 48 |
| `components/blog/views/archive-mobile-nav.tsx` | 47 |
| `components/blog/views/index-mobile-nav.tsx` | 44 |
| `lib/styles.ts` | 41 |
| `components/blog/views/archive-compact-posts.tsx` | 33 |
| `components/home/views/numbered-navigation.tsx` | 33 |
| `components/blog/folio-chrome.tsx` | 32 |
| `components/blog/views/archive-expanded-posts.tsx` | 31 |
| `components/blog/views/index-control-panel.tsx` | 31 |
| `components/ui/select.tsx` | 31 |
| `components/blog/archive-chrome.tsx` | 27 |
| `components/blog/views/folio-expanded-posts.tsx` | 27 |
| `components/blog/views/archive-control-panel.tsx` | 26 |
| `components/blog/views/folio-control-panel.tsx` | 26 |
| `components/home/views/cover-hero.tsx` | 25 |
| `components/blog/views/archive-preview.tsx` | 23 |
| `components/blog/views/folio-preview.tsx` | 23 |
| `components/blog/views/index-preview.tsx` | 21 |
| `components/search/views/editorial-search.tsx` | 21 |
| `components/blog/views/folio-compact-posts.tsx` | 19 |
| `components/blog/markdown.tsx` | 18 |
| `components/home/views/compact-navigation.tsx` | 16 |
| `components/home/views/editorial-featured-posts.tsx` | 15 |
| `components/home/views/notes-featured-posts.tsx` | 14 |
| `components/home/views/sidebar-tools.tsx` | 14 |
| `components/about/about-profile.tsx` | 11 |
| `components/home/views/contact-footer.tsx` | 11 |
| `components/home/views/prints-photos.tsx` | 11 |
| `components/blog/article-interactions.tsx` | 10 |
| `components/blog/views/editorial-article.tsx` | 10 |
| `components/home/views/photograph-hero.tsx` | 10 |
| `components/home/views/signature-hero.tsx` | 10 |
| `components/home/views/sidebar-frame.tsx` | 9 |
| `components/photography/photo-detail.tsx` | 9 |
| `components/blog/views/editorial-desktop-toc.tsx` | 8 |
| `components/blog/views/library-article.tsx` | 8 |
| `components/home/views/contact-sheet-photos.tsx` | 8 |
| `components/home/views/ledger-post-row.tsx` | 8 |
| `components/home/views/timeline-post-row.tsx` | 8 |
| `components/links/views/editorial-links.tsx` | 8 |
| `components/rss/views/editorial-rss.tsx` | 8 |
| `components/blog/article-cover.tsx` | 7 |
| `components/blog/folio-design.tsx` | 7 |
| `components/blog/views/editorial-mobile-toc.tsx` | 7 |
| `components/home/views/card-links.tsx` | 7 |
| `components/photography/photography-chrome.tsx` | 7 |
| `components/ui/button.tsx` | 7 |
| `components/blog/views/studio-article.tsx` | 6 |
| `components/brand/site-logo.tsx` | 6 |
| `components/ui/marquee.tsx` | 6 |
| `components/blog/views/library-desktop-toc.tsx` | 5 |
| `components/blog/views/studio-desktop-toc.tsx` | 5 |
| `components/home/featured-categories/catalog.tsx` | 5 |
| `components/home/featured-categories/deck.tsx` | 5 |
| `components/home/views/directory-links.tsx` | 5 |
| `components/home/views/timeline-latest-posts.tsx` | 5 |
| `components/links/views/flat-links.tsx` | 5 |
| `components/photography/photo-studio.tsx` | 5 |
| `components/blog/article-image-viewer.tsx` | 4 |
| `components/blog/blog-layout.tsx` | 4 |
| `components/blog/views/archive-pagination.tsx` | 4 |
| `components/blog/views/folio-category-tree.tsx` | 4 |
| `components/blog/views/index-category-tree.tsx` | 4 |
| `components/blog/views/library-mobile-toc.tsx` | 4 |
| `components/blog/views/studio-mobile-toc.tsx` | 4 |
| `components/home/featured-categories/tilt.tsx` | 4 |
| `components/home/section-heading.tsx` | 4 |
| `components/home/views/cloud-interests.tsx` | 4 |
| `components/home/views/colophon-footer.tsx` | 4 |
| `components/home/views/tag-interests.tsx` | 4 |
| `components/layout/utility-design-switcher.tsx` | 4 |
| `components/photography/photo-tile.tsx` | 4 |
| `components/search/views/flat-search.tsx` | 4 |
| `components/blog/post-extras.tsx` | 3 |
| `components/blog/views/archive-category-tree.tsx` | 3 |
| `components/home/featured-categories/accordion.tsx` | 3 |
| `components/home/featured-categories/index.tsx` | 3 |
| `components/home/views/topbar-frame.tsx` | 3 |
| `components/rss/views/flat-rss.tsx` | 3 |
| `components/ui/sonner.tsx` | 3 |
| `app/page.tsx` | 2 |
| `components/home/home-brand.tsx` | 2 |
| `components/home/views/full-width-hero-photo.tsx` | 2 |
| `components/home/views/ledger-latest-posts.tsx` | 2 |
| `app/error.tsx` | 1 |
| `app/not-found.tsx` | 1 |
| `app/photography/catalog/page.tsx` | 1 |
| `app/photography/not-found.tsx` | 1 |
| `components/blog/comment-section.tsx` | 1 |
| `components/home/featured-categories/rail.tsx` | 1 |
| `components/home/home-layout.tsx` | 1 |
| `components/home/views/cover-hero-photo.tsx` | 1 |
| `components/home/views/topbar-tools.tsx` | 1 |
| `components/photography/photo-lightbox-image.tsx` | 1 |
| `components/search/search-form.tsx` | 1 |
| `components/search/search-input.tsx` | 1 |
| `components/theme-toggle.tsx` | 1 |
| `components/ui/badge.tsx` | 1 |
| `components/ui/dialog.tsx` | 1 |

## 附录：超过 500 字符的 className 表达式

字符数包含初始化表达式标点，不展开变量。

| 文件与行 | 字符数 |
| --- | ---: |
| `components/blog/views/index-compact-posts.tsx:13` | 1282 |
| `components/blog/views/index-expanded-posts.tsx:13` | 1008 |
| `components/blog/views/index-compact-posts.tsx:12` | 948 |
| `components/ui/select.tsx:47` | 868 |
| `components/blog/views/archive-compact-posts.tsx:13` | 829 |
| `components/blog/views/archive-expanded-posts.tsx:13` | 805 |
| `components/ui/select.tsx:73` | 803 |
| `components/home/home-design-switcher.tsx:111` | 747 |
| `components/blog/views/index-expanded-posts.tsx:12` | 737 |
| `components/blog/index-design.tsx:25` | 612 |
| `components/blog/views/folio-chrome-parts.tsx:25` | 577 |
| `components/home/views/cover-hero.tsx:18` | 571 |
| `components/blog/views/index-chrome-parts.tsx:26` | 554 |
| `components/blog/views/folio-parts.tsx:46` | 527 |
| `components/blog/views/archive-parts.tsx:53` | 515 |
| `components/blog/views/editorial-desktop-toc.tsx:17` | 512 |
| `components/blog/views/editorial-mobile-toc.tsx:16` | 512 |
| `components/blog/views/index-parts.tsx:46` | 511 |
| `components/blog/views/archive-parts.tsx:87` | 503 |
