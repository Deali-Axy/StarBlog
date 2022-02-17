## 博客

支持Markdown导入的博客

## 第三方组件

- ORM：FreeSQL
- markdown解析：[Markdig](https://github.com/xoofx/markdig)


## 代码高亮：

本来是打算拿`Markdig`的几个第三方插件来试验的，测试后勉强能用就以下两个

- [Markdig.Prism](https://github.com/ilich/Markdig.Prism)：前端渲染，但需要服务端组件配合
- [Markdown.ColorCode](https://github.com/wbaldoumas/markdown-colorcode)：服务端渲染

结果效果都不是很理想，今晚（2022-2-17）看阿星plus的博客，看到`Editor.md`这个组件也可以渲染HTML，立即测试了一下，好用！

所以最终我的markdown渲染HTML以及代码高亮由`Editor.md`承包！

（PS：`Markdig`组件先保留吧）