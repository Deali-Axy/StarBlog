# StarBlog

![](./docs/images/logo.jpg)


<p align="center">
  <a href="https://dotnet.microsoft.com/download/dotnet/6.0"><img src="https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 6"></a>
  <a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Vue3-4FC08D?style=flat-square&logo=vue.js" alt="Vue 3"></a>
  <a href="https://blog.deali.cn"><img src="https://img.shields.io/badge/Demo-Online-brightgreen?style=flat-square" alt="Online Demo"></a>
  <a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Repository-181717?style=flat-square&logo=github" alt="Admin Repository"></a>
</p>

## 📝 简介

StarBlog 是一个基于 .NET 6 和 ASP.NET Core 开发的现代博客系统，支持 Markdown 文章导入，遵循 RESTful 接口规范。前端基于 Vue + ElementUI 开发，可作为 .NET Core 入门学习项目，同时配套了一系列开发笔记，记录了从零开始构建这个博客系统的全过程，可以帮助学习理解 .Net Core 项目的开发流程。

**在线演示**：[https://blog.deali.cn](https://blog.deali.cn)

**管理后台项目**：[https://github.com/Deali-Axy/StarBlog-Admin](https://github.com/Deali-Axy/StarBlog-Admin)

**AI文章发布工具**：[https://github.com/star-blog/starblog-publisher](https://github.com/star-blog/starblog-publisher)

## 📫 项目动机

在线博客平台那么多，**为什么要自己开发博客？**

笔者折腾博客时间不短了，从一开始用WordPress，再到后来用hexo、hugo之类的静态博客生成放github托管，一直在折腾。

折腾是为了更好解决问题，但最终还是打算自己花时间做一个，具体的想法可以查看 [开发笔记](https://www.cnblogs.com/deali/p/16104454.html)

- 找不到一个让我满意的在线博客
- 在线写博客体验不如在本地用typora写
- 写公众号推文的话注意力会分散一部分到如何写得吸引读者而不是文章本身
- 自己的网站才有完整的控制权，不会被垃圾平台添加不良信息污染

## ✨ 特性

- **Markdown 支持**：(**核心功能💡**)本地 Markdown 文章批量导入，支持解析多级分类嵌套和图片自动导入与内容转换
- **文章管理**：支持单篇文章（含图片附件）打包上传和自动导入
- **摄影作品**：支持本地摄影作品批量上传，自动读取 EXIF 信息
- **自定义主页**：可配置的博客主页，支持首页图表/随机图片展示、置顶文章/图片/分类配置
- **主题切换**：博客前台支持多种主题风格切换
- **友情链接**：完整的友情链接管理功能，支持用户自行提交友链申请
- **访问统计**：高性能的访问记录、统计和数据可视化展示
- **随机图片 API**：提供一套随机图片 API，文章封面默认使用随机图片
- **评论系统**：内置用户/游客评论功能，以及评论自动过滤、审核功能
- **RSS 订阅**：支持 RSS 订阅

## 🌟 StarBlog 生态

StarBlog 不仅仅是一个博客系统，它正在发展成为一个完整的博客生态系统，包含多个配套工具和项目：

### StarBlog Publisher

[StarBlog Publisher](https://github.com/star-blog/starblog-publisher) 是一款专为 StarBlog 博客系统设计的专业文章发布工具，提供比传统打包上传更便捷的文章发布方式。

- **Markdown 支持**：支持 Markdown 格式文章的预览和发布
- **直观界面**：提供用户友好的界面，轻松管理和发布博客内容
- **AI 辅助**：集成多种领先的 AI 大模型（包括 OpenAI 的 ChatGPT、Anthropic 的 Claude 和 DeepSeek 等）
- **跨平台**：基于 C# 和 .NET 8.0 构建，可在 Windows、macOS 和 Linux 上运行

### StarBlog Admin

[StarBlog Admin](https://github.com/Deali-Axy/StarBlog-Admin) 是 StarBlog 的管理后台项目，基于 Vue + ElementUI 开发，提供完整的博客内容管理功能。

### StarBlogHub

实现一个去中心化的博客聚合平台，不同的个人博客都可以接入，共享流量。

## 🏗️ 架构

### 技术栈

#### 后端

##### 核心框架与基础设施
- **Web框架**：ASP.NET Core - 跨平台、高性能的 .NET Web 应用框架
- **监控调试**：Rin - ASP.NET Core 应用实时检查工具

##### 数据访问与处理
- **主要ORM**：FreeSql - 高性能、支持多种数据库的 ORM 框架
- **辅助ORM**：Entity Framework Core - 微软官方 ORM 框架
- **对象映射**：AutoMapper - 简化对象-对象映射的工具库

##### API与认证
- **API文档**：Swagger/OpenAPI (Swashbuckle.AspNetCore) - RESTful API 自动文档生成工具
- **认证机制**：JWT (JSON Web Token) - 安全的跨域身份验证解决方案
- **搜索引擎支持**：RobotsTxtCore - 管理搜索引擎爬虫访问策略

##### 内容处理
- **Markdown引擎**：[Markdig](https://github.com/xoofx/markdig) - 高性能 Markdown 处理器
- **图像处理**：ImageSharp - 跨平台图像处理库
- **分页组件**：X.PagedList - 高效的数据分页解决方案
- **RSS支持**：System.ServiceModel.Syndication - RSS 内容聚合工具

##### 通信与工具
- **邮件服务**：MailKit - 跨平台邮件客户端库

#### 前端
- **博客前台**：Bootstrap + Vue + ElementUI + editor.md + bootswatch
- **管理后台**：Vue + Vuex + Vue Router + ElementUI + SCSS

### 项目结构

```
StarBlog/
├── StarBlog.Contrib/       # 通用工具和扩展
├── StarBlog.Data/          # 数据模型和数据访问层
├── StarBlog.Migrate/       # 博客文章导入 CLI 工具
├── StarBlog.Share/         # 共享组件和工具类
└── StarBlog.Web/           # 主 Web 应用项目
```

## 🚀 快速开始

### 环境要求

- **.NET 6 SDK**
- **Node.js v18** 以上版本 和 **npm/yarn**（用于前端资源管理）

### 构建步骤

1. **克隆仓库**

```bash
git clone https://github.com/Deali-Axy/StarBlog.git
cd StarBlog
```

2. **前端资源准备**

本项目使用 NPM + Gulp 管理前端静态文件，需要使用 Nodejs 18 以上版本，详情可查看: [AspNetCore开发笔记：使用NPM和gulp管理前端静态文件](https://www.cnblogs.com/deali/p/15905760.html)。

```bash
cd StarBlog.Web
npm i -g bower
npm install  # 或 yarn
npm install --global gulp-cli
gulp move
```

**注意**：本项目依赖 [bootstrap5-treeview](https://www.npmjs.com/package/bootstrap5-treeview) 组件。而这个组件又使用 bower 进行构建，请先安装 [bower](https://bower.io/)：`npm i -g bower`，不然在执行 `npm install` 过程中会出错。

3. **运行项目**

使用 Visual Studio 或 Rider 打开解决方案，设置 `StarBlog.Web` 为启动项目并运行。

为了快速启动，本项目默认使用 SQLite 数据库，大部分功能都是使用 FreeSQL 作为 ORM，直接运行项目，无需额外配置 FreeSQL 会自动生成表结构。

4. **访问日志数据库同步**

StarBlog 的访问日志模块为了优化性能，是独立的数据库，使用 EFCore 进行管理，详见: [StarBlog 番外篇 (1) 全新的访问统计功能，异步队列，分库存储](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)

EFCore 不能像 FreeSQL 一样自动生成表结构，需要手动同步数据库，默认也是使用 SQLite 数据库，如有需要可以自行切换 MySQL 或者 PostgreSQL。

首先安装 EFCore 的 cli 工具:

```bash
dotnet tool install --global dotnet-ef
```

同步数据库 (Windows10+)

```powershell
cd StarBlog.Data
$env:CONNECTION_STRING = "Data Source=..\StarBlog.Web\app.log.db"
dotnet ef database update
```

同步数据库 (Linux/MacOS)

```bash
cd StarBlog.Data
set CONNECTION_STRING = "Data Source=../StarBlog.Web/app.log.db"
dotnet ef database update
```

### 初始化

首次启动 StarBlog 项目后，访问 `/Home/Init` 进行管理员账户创建等初始化操作，之后才可以使用这个管理员账号登录管理后台。

**注意**：初始化操作只能执行一次。详情请参考 [StarBlog - (16) 一些新功能 (监控/统计/配置/初始化)](https://www.cnblogs.com/deali/p/16523157.html)。

### 配置

#### 邮件配置

StarBlog 的友情链接、评论系统都用到了发邮件功能，详情见: [StarBlog - (28) 开发友情链接相关接口](https://blog.deali.cn/Blog/Post/f55c8c7610706503)

请在项目根目录中添加 `appsettings.email.json` 文件，以 Outlook 邮箱为例，配置如下

```json
"EmailAccountConfig": {
  "Host": "smtp-mail.outlook.com",
  "Port": 587,
  "FromUsername": "邮箱地址@outlook.com",
  "FromPassword": "邮箱密码",
  "FromAddress": "邮箱地址@outlook.com"
}
```

#### 敏感词检测

StarBlog 使用 DFA 技术实现评论敏感词检测，使用时需要在 StarBlog.Web 项目下放置一个敏感词库文件 `words.json`

为了网络环境的文明和谐，本项目的开源代码里不能提供，需要的同学可以自行搜集。

格式是这样的

```json
[
  {
    "Id": 1,
    "Value": "小可爱",
    "Tag": "暴力"
  },
  {
    "Id": 2,
    "Value": "河蟹",
    "Tag": "广告"
  }
]
```

>如果是学习、测试用途实在需要的话，可以在公众号「程序设计实验室」后台私信 badwords 获取。
>
>**免责声明（Sensitive Word List Disclaimer）**
>
>本敏感词库仅供学习、研究及测试用途。**禁止将本敏感词库用于任何商业用途、违法用途或其他可能违反所在地法律法规的行为。**
>
>请知悉：
>
>- 敏感词库内容来源广泛，**不代表 StarBlog 项目或作者的任何立场或观点**。
>- **禁止用户以任何形式传播、分享、转售、公开发布本敏感词库的全部或部分内容**。
>- 用户在使用过程中，应遵守适用法律法规，**由此产生的任何法律责任由使用者自行承担**。
>- 出于合规与安全考虑，词库内容已作加密处理，使用者需自行承担解密与使用风险。
>
>**使用即代表您已阅读并同意本免责声明的全部内容。**

## 部署

StarBlog 支持多种部署方式，门槛最低的是使用 self-container 模式发布，然后在服务器上直接运行即可。

```bash
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true  --self-contained true
```

发布之后将 `publish` 目录下的文件上传到服务器，运行 **StarBlog.Web** 文件即可。

关于部署的更多方式可以参考: [StarBlog - (31) 发布和部署](https://blog.deali.cn/p/starblog-31)


## 📚 开发笔记

本项目配套了一系列开发笔记，记录了从零开始构建这个博客系统的全过程，同时可以作为 .NetCore 开发的入门学习教程。

> PS: 项目代码更新较快，学习开发笔记系列文章请使用 [v1.0.0 版本代码](https://github.com/Deali-Axy/StarBlog/tree/v1.0.0)
>
> clone 命令
>
> ```bash
> git clone --branch v1.0.0 --single-branch https://github.com/Deali-Axy/StarBlog.git
> ```

- [基于.NetCore开发博客项目 StarBlog - (1) 为什么需要自己写一个博客？](https://www.cnblogs.com/deali/p/16104454.html)
- [基于.NetCore开发博客项目 StarBlog - (2) 环境准备和创建项目](https://www.cnblogs.com/deali/p/16172342.html)
- [基于.NetCore开发博客项目 StarBlog - (3) 模型设计](https://www.cnblogs.com/deali/p/16180920.html)
- [基于.NetCore开发博客项目 StarBlog - (4) markdown博客批量导入](https://www.cnblogs.com/deali/p/16211720.html)
- [基于.NetCore开发博客项目 StarBlog - (5) 开始搭建Web项目](https://www.cnblogs.com/deali/p/16276448.html)
- [基于.NetCore开发博客项目 StarBlog - (6) 页面开发之博客文章列表](https://www.cnblogs.com/deali/p/16286780.html)
- [基于.NetCore开发博客项目 StarBlog - (7) 页面开发之文章详情页面](https://www.cnblogs.com/deali/p/16293309.html)
- [基于.NetCore开发博客项目 StarBlog - (8) 分类层级结构展示](https://www.cnblogs.com/deali/p/16307604.html)
- [基于.NetCore开发博客项目 StarBlog - (9) 图片批量导入](https://www.cnblogs.com/deali/p/16328825.html)
- [基于.NetCore开发博客项目 StarBlog - (10) 图片瀑布流](https://www.cnblogs.com/deali/p/16335162.html)
- [基于.NetCore开发博客项目 StarBlog - (11) 实现访问统计](https://www.cnblogs.com/deali/p/16349155.html)
- [基于.NetCore开发博客项目 StarBlog - (12) Razor页面动态编译](https://www.cnblogs.com/deali/p/16391656.html)
- [基于.NetCore开发博客项目 StarBlog - (13) 加入友情链接功能](https://www.cnblogs.com/deali/p/16421699.html)
- [基于.NetCore开发博客项目 StarBlog - (14) 实现主题切换功能](https://www.cnblogs.com/deali/p/16441294.html)
- [基于.NetCore开发博客项目 StarBlog - (15) 生成随机尺寸图片](https://www.cnblogs.com/deali/p/16457314.html)
- [基于.NetCore开发博客项目 StarBlog - (16) 一些新功能 (监控/统计/配置/初始化)](https://www.cnblogs.com/deali/p/16523157.html)
- [基于.NetCore开发博客项目 StarBlog - (17) 自动下载文章里的外部图片](https://www.cnblogs.com/deali/p/16586437.html)
- [基于.NetCore开发博客项目 StarBlog - (18) 实现本地Typora文章打包上传](https://www.cnblogs.com/deali/p/16758878.html)
- [基于.NetCore开发博客项目 StarBlog - (19) Markdown渲染方案探索](https://www.cnblogs.com/deali/p/16834452.html)
- [基于.NetCore开发博客项目 StarBlog - (20) 图片显示优化](https://www.cnblogs.com/deali/p/16929677.html)
- [基于.NetCore开发博客项目 StarBlog - (21) 开始开发RESTFul接口](https://www.cnblogs.com/deali/p/16989798.html)
- [基于.NetCore开发博客项目 StarBlog - (22) 开发博客文章相关接口](https://www.cnblogs.com/deali/p/16991279.html)
- [基于.NetCore开发博客项目 StarBlog - (23) 文章列表接口分页、过滤、搜索、排序](https://www.cnblogs.com/deali/p/16992573.html)
- [基于.NetCore开发博客项目 StarBlog - (24) 统一接口数据返回格式](https://www.cnblogs.com/deali/p/16995384.html)
- [基于.NetCore开发博客项目 StarBlog - (25) 图片接口与文件上传](https://www.cnblogs.com/deali/p/16999818.html)
- [基于.NetCore开发博客项目 StarBlog - (26) 集成Swagger接口文档](https://www.cnblogs.com/deali/p/17093390.html)
- [基于.NetCore开发博客项目 StarBlog - (27) 使用JWT保护接口](https://blog.sblt.deali.cn:9000/Blog/Post/541b8beae183d29e)
- [基于.NetCore开发博客项目 StarBlog - (28) 开发友情链接相关接口](https://www.cnblogs.com/deali/p/starblog-28.html)
- [基于.NetCore开发博客项目 StarBlog - (29) 开发RSS订阅功能](https://www.cnblogs.com/deali/p/17501704.html)
- [基于.NetCore开发博客项目 StarBlog - (30) 实现评论系统](https://www.cnblogs.com/deali/p/17910094.html)
- [基于.NetCore开发博客项目 StarBlog - (31) 发布和部署](https://www.cnblogs.com/deali/p/18011965)
- [基于.NetCore开发博客项目 StarBlog - (32) 第一期完结](https://www.cnblogs.com/deali/p/18582026)
- ...

番外篇：

- [StarBlog 番外篇 (1) 全新的访问统计功能，异步队列，分库存储](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)
- [StarBlog 番外篇 (2) 深入解析Markdig源码，优化ToC标题提取和文章目录树生成逻辑](https://blog.deali.cn/Blog/Post/d47403a6c7399c44)
- [StarBlog 番外篇 (3) StarBlog Publisher，跨平台一键发布，DeepSeek加持的文章创作神器](https://blog.deali.cn/Blog/Post/211884ccc25b02e2)


## 📷 截图展示

### 博客主页

![](./docs/images/home.png)

### 文章列表

![](./docs/images/post-list.jpg)

### 摄影页面~~（虽然现在还没把拍的照片放上去就是了）~~

![](./docs/images/photos.png)

### 管理后台主页

![](./docs/images/admin.png)

### 后台文章列表

![](./docs/images/admin-posts.png)

### 文章编辑界面

![](./docs/images/admin-post-edit.png)

### 后台图片列表

![](./docs/images/admin-photos.png)

## 🔗 关注我

公众号 | 公众号二维码 
------- | ------ 
![](./docs/images/coding_lab_logo.jpg) | ![](./docs/images/coding_lab_qr_code.jpg)


## 🔄 当前版本的不足之处

从 StarBlog 项目上线至今，我不断学习关于 AspNetCore 的细节知识，相比起刚刚开发这个项目的时候，对框架的熟悉程度提升了一些，自然也发现了之前代码里的局限之处：

- 增删改查的「查」应该使用 patch 方法
- 在 Get 方法接口加上 `[HttpHead]` 来实现对 Head 方法的支持
- 过滤和搜索的接口需要对参数进行 trim
- 不应该将接口的返回值都修改为 `ApiResponse` 类型，应该保留框架的 `ActionResult` 类型，这样功能更多
- 只统一了接口的返回值，没有对异常进行包装，应该使用 `app.UseExceptionHandler` 中间件来实现统一错误处理（也可以使用异常过滤器）
- 对 markdown 的 toc、公式、代码块、表格嵌套图片等还是支持不佳

这些问题将是 v2 版本要解决的。

## 🚀 v2 新版规划

目前规划了一些新的功能和优化，但这肯定不是 v2 版本的全部，各位同学如果有好的建议也可以留言讨论一下~

### 博客前台重构

- 使用 Next.js/Remix/Astro 重构 (具体技术栈待定)
- 使用 nodejs 技术栈的 markdown 解析功能

### 管理后台重构

- 使用基于 react 的技术栈重构

### 新的访问统计功能

- 地理信息可视化
- 搜索引擎收录分析
- 反爬虫功能
- 文章阅读量统计

### 文章编辑功能

- 使用新的 markdown 编辑器（最好像 wagtail 那样所见即所得的）
- 支持在文章中加入更多内容（如视频）

### 文章阅读体验优化

- 使用新的 markdown 渲染工具（目前使用的是我 fork 魔改的 [editor.md](https://github.com/Deali-Axy/editor.md-ext) , 用起来还可以，但这个工具很老了，而且也停更了，我希望找一个维护良好更现代的渲染工具来替代）

### 文章加密

- 设置固定密码
- 关注公众号获取动态密码

### 新版搜索功能

- 使用全文检索引擎
- 加入 Embedding

### AI 功能

- 知识库
- 对话功能
- 文章 AI 总结
- 自动评论



## 🙏 致谢

<p align="center">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.png" alt="JetBrains Rider" width="200">
</p>

<p align="center">
  感谢 <a href="https://jb.gg/OpenSourceSupport">JetBrains Open Source Support</a> 提供的免费开发工具 Rider
</p>
