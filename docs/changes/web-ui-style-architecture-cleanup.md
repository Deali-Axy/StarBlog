# Web 可切换 UI 架构清理

日期：2026-11-01。对应 [实施前审计](web-ui-style-architecture-audit.md)。

## 组件边界

路由负责数据与服务端 Markdown；稳定的控制层负责交互状态；选择层通过 `satisfies Record<Variant, ComponentType<Props>>` 映射组件；展示层拥有自己的 JSX、固定 Tailwind 编排和原语组合。只挂载当前方案。

| 功能 | 独立展示 |
| --- | --- |
| 文章 | Index / Folio / Archive 文章布局，各自的桌面及移动目录 |
| 首页框架 | Sidebar / Topbar，Numbered / Compact 导航与各自工具栏 |
| 首页 Hero | Cover / Signature / Photograph，封面与全幅摄影各自展示 |
| 首页内容 | 两套精选文章、最近文章与条目、兴趣分类、摄影与相纸、友链和页脚；保留五套精选分类 |
| 博客列表 | 三套页面，各自 Expanded / Compact 集合、工具栏、筛选区、结果容器和分页 |
| 博客导航 | 三套分类树与目录容器，两套移动导航；三套风格预览工具 |
| 摄影墙 | Masonry / Justified，独立调用对应算法和编排图片 |

展示实现位于各功能的 `views` 目录，首页选择器位于 `slots`。旧路由使用的入口组件继续充当选择器或导出门面。各方案不接收风格标识、布局布尔值或 classes 配置；组件 props 可以包含数据、受控交互值、回调与内容插槽。预览工具的当前选项仅用于控件选中状态。

共享叶子组件仅承担一致的结构和行为。基础控件的 `variant` / `size`、明暗、主题配色、字体、密度、字号、数据决定的内容编排和交互状态仍可参数化；它们不负责替换另一套布局。

## 状态生命周期

- `CommentSessionProvider` 在文章布局之外，以文章 ID 为 key。评论查询、表单、OTP、回复、倒计时和待审核评论随文章会话保留。
- `ArticleReadingProvider` 统一观察正文。布局替换后重新绑定节点；桌面与移动目录共享定位和进度，不重复注册观察器。
- `HomeInteractionProvider` 位于框架外，按功能与数据 ID 保存摘要展开、分类选择、当前摄影、加载与失败状态。离开首页后会话结束，不增加持久化格式。
- 首页自定义面板位于框架之外，预览保留当前标签和编辑会话；提交、撤销、取消沿用既有语义。
- 博客导航与分类控制层不随页面风格替换。`BlogInteractionProvider` 保存未提交搜索与工作台选择，临时目录和预览弹层在所属视图卸载时关闭。
- 摄影墙的测量与自然尺寸缓存在选择层，分页、灯箱缓存、焦点和滚动恢复继续由 `PhotoStudio` 管理。

## 样式和原语

删除 `article-designs.ts`、`blog-designs.module.css`、`blog-chrome.module.css`、`blog-preview.module.css`。布局类直接归属于各自展示组件。首页 CSS 仅保留主题 token、字体映射、纹理、边框、阴影及焦点效果；原生文章 CSS 仅处理浏览器伪元素。

业务层统一组合 `components/ui` 中的源码原语。沿用现有 Radix 依赖与 `radix-vega`，不增加状态库。Dialog 与 Sheet 保留显式 Portal / Overlay / Content 组合，原有焦点约束、关闭行为和主题作用域由调用者组合。触发器在 hydration 完成后启用，避免首次点击发生在事件绑定之前。

固定页头高度由实际 DOM 测量写入滚动偏移变量，不再用父级风格选择器改变偏移。`data-*` 保留语义、测试定位与交互状态用途，不用来选择不同页面构图。

## 验证

Web 检查：`pnpm typecheck`、`pnpm lint`、`pnpm test:home`、`pnpm test:blog`、`pnpm test:photography`、`pnpm test:architecture`、`pnpm build`。

浏览器继续使用现有 .NET Playwright 项目与隔离数据库夹具。新增 `UiCompositionTests` 覆盖评论草稿和编码目录目标、搜索草稿与工作台选择、首页全部区块及框架替换、两种摄影布局与三档密度。现有测试覆盖 URL / Cookie / 历史、翻译与旧地址、响应式构图、Portal 主题、焦点、图片失败和灯箱缓存。

ESLint 约束原语与展示层导入边界；`ui-architecture.test.mjs` 检查布局选择标识不能进入展示样式表达式，以及 CSS 不得重新承载布局切换。

摄影父布局在取数前显式等待请求边界，避免默认 Turbopack 预渲染阶段等待 API；运行时仍使用原有查询与缓存契约。

### 本次验收结果

- `pnpm typecheck`、`pnpm lint`、`pnpm test:home`、`pnpm test:blog`、`pnpm test:photography`、`pnpm test:architecture` 全部通过，共 33 项基础及架构检查。
- 隔离数据库上的完整 .NET Playwright 回归 100 / 100 通过；摄影请求边界调整后，摄影与组件状态相关的 33 项补跑全部通过。
- 项目默认 `pnpm build`（Turbopack）通过，另验证了 Webpack 生产构建。
- 响应式与明暗截图覆盖首页预设 / Hero、博客三套页头页脚和文章布局；浏览器用例覆盖长标题、空结果、图片失败、翻译、焦点、历史及 Portal 主题。

## 兼容

API、路由、`bv`、`view`、`hc`、旧 `hv/fc`、Cookie、摄影偏好和首页 v2 配置均保持原有契约，无数据迁移。canonical 与文章永久链接仍不携带预览风格。新增方案必须创建独立展示文件，再扩充类型化组件映射。
