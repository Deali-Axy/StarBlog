# Links、RSS 与 Search 的两套 UI

`/links`、`/rss` 默认使用简洁 flat 布局。与 blog 页一致，右下角的调色板浮动按钮打开“页面风格”菜单，可以切换为此前的刊物布局，对应 `?uv=editorial`。切回简洁会删除 `uv`；未知值回退到简洁。地址可分享、刷新，也支持浏览器前进和后退，canonical 仍是原始页面地址。菜单支持键盘选择、Escape 关闭和关闭后焦点返回按钮，并避开屏幕安全区域。

两套展示分别位于 `components/links/views`、`components/rss/views`，由 `LinksDesign`、`RssDesign` 的类型化组件映射选择。展示只接收内容和插槽，不读取布局选择状态。后续增加方案时，扩展 `UtilityVariant` 和组件映射即可。

`/search` 同样默认简洁版并使用浮动菜单。简洁版空白页展示居中的搜索框，结果页采用标题、网址和摘要列表；原有主题推荐、近期文章、侧栏与插图保留在刊物版。页面外层复用 layout 容器，结果文本限制阅读宽度。搜索页隐藏页头重复的搜索框。

搜索路由继续在服务端获取结果、主题和近期文章。`SearchDesign` 选择独立的页面和表单展示；`useArticleSearch` 保存输入草稿、快捷键和提交状态，风格切换不重置输入。新关键词提交重置页码，分页、重试和新搜索保留当前 `uv`。高亮仅将字面量 `<mark>` 识别为元素，其余 API 文本由 React 转义。

`useLinkApplication` 位于展示选择层，切换风格时保留草稿、提交中状态和请求结果。表单验证、API、重置和提交逻辑共用；RSS 地址复制及手动复制提示同样共用。不增加持久化配置或 API 契约。

基础验证：`pnpm typecheck`、`pnpm test:architecture`、`pnpm test:utility`。浏览器验证切换、历史返回、草稿保留、重置、复制、中英文、明暗和手机布局。
