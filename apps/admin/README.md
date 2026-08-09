# StarBlog 管理后台

这是配套 `StarBlog.Api` 的 Refine + Vite 管理端。它聚合文章、分类、摄影、评论审核、友链、站点配置和多平台发布，默认开发地址为 `http://localhost:5173`。

## 本地启动

先启动 API（默认开发端口为 `5039`）：

```powershell
dotnet run --project src\StarBlog.Api\StarBlog.Api.csproj
```

然后在本目录执行：

```powershell
npm install
npm run dev
```

开发时如 API 不在默认地址，复制 `.env.example` 为 `.env.local`，再修改 `VITE_API_PROXY_TARGET`。生产构建或不使用 Vite 代理时，设置 `VITE_API_URL`；例如：

```powershell
$env:VITE_API_URL = "https://api.example.com"
npm run dev
```

## 内容与发布工作台

- 资源页支持文章、分类、图片、友链和站点配置的新增、编辑、删除；图片新增使用 `multipart/form-data` 上传文件。
- 评论页提供独立的通过与拒绝操作；文章页提供推荐、置顶和英文翻译快捷操作。
- 发布中继会为每次投递保存一份内容快照，防止文章后续编辑改变已审核版本。
- 微信公众号使用官方 API：文章正文图片会上传到微信，封面会作为缩略图上传，最终创建**草稿**，不会自动群发。
- 知乎与掘金目前没有可供普通创作者稳定使用的发布 API。因此后台会生成可复制快照并打开各自的官方创作入口，不会通过 Cookie 或浏览器自动化伪造发布。

## 公众号渠道配置

在“发布中继”新增微信公众号渠道并填入 `AppId`、`AppSecret`、作者、封面地址。`AppSecret` 只接受写入，读取接口永不回传。微信公众号草稿 API 需要可由服务器访问的封面地址；如没有封面，投递会明确报错而不会创建不完整草稿。
