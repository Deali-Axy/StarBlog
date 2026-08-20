# Swagger 与 JWT 使用指南

StarBlog.Api 的 Swagger UI 默认地址为 `http://localhost:5039/swagger`。文档页面和 OpenAPI JSON 可以匿名访问；具体 API 是否需要登录，仍由控制器上的 `[Authorize]` 决定。

## 获取管理员账号

全新数据库第一次打开管理端 `http://localhost:5173/login` 时，会显示一次性初始化表单。提交后管理员账号会直接写入业务 SQLite；用户表已有记录后，该匿名初始化入口会自动关闭。

如果数据库已有管理员但密码遗失，请先停止 API，然后在仓库根目录运行：

```powershell
.\scripts\Initialize-StarBlogAdmin.ps1 -Username admin
```

脚本会安全读取新密码、备份数据库，再创建或重置指定账号。

## 在 Swagger 中调用管理接口

1. 调用 `POST /api/v1/auth/tokens`，请求体示例：

   ```json
   {
     "username": "admin",
     "password": "your-password"
   }
   ```

2. 从响应 JSON 的 `token` 字段取得 JWT。响应不再包装 `ApiResponse`。
3. 点击 Swagger 页面右上角的 **Authorize**。
4. 只粘贴 Token 本身。Swagger 使用标准 HTTP Bearer 方案，会自动添加 `Bearer ` 前缀。
5. 再调用 `/api/v1/admin/*` 等受保护接口。

## 常见问题

- 访问 `/swagger` 仍返回“请先获取 JWT”：正在运行的仍是旧构建，请停止并重新启动 `StarBlog.Api`。
- Swagger 可打开但管理接口返回 401：Token 缺失、过期，或粘贴时额外输入了 `Bearer `。
- 管理端首次打开直接显示登录而不是初始化：SQLite 的 `user` 表已经有记录；密码未知时使用上面的初始化脚本重置。
- Vite 报 `ECONNREFUSED`：确认 API 正在 `http://localhost:5039` 监听，或通过 `VITE_API_PROXY_TARGET` 修改代理目标。

## 安全说明

公开 API 文档不等于公开管理 API。JWT 校验仍在认证与授权中间件中执行，Swagger 仅负责展示接口以及为测试请求附加令牌。生产环境若不希望公开接口结构，应在反向代理层限制 `/swagger` 和 `/swagger/*`，而不是让浏览器通过 URL 或查询参数传递 JWT。
