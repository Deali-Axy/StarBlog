# Swagger UI 访问授权配置指南

本指南介绍如何为 StarBlog 项目的 Swagger UI 添加访问授权保护，防止未授权用户访问 API 文档。

## 🔒 功能特性

- **JWT 令牌验证**: 基于现有的 JWT 认证系统
- **灵活配置**: 支持开启/关闭授权保护
- **环境感知**: 可配置仅在生产环境启用
- **详细日志**: 记录访问尝试和授权状态
- **友好错误**: 提供清晰的未授权错误信息

## 🚀 快速开始

### 1. 基本配置（推荐）

在 `Program.cs` 中，Swagger 授权默认已启用：

```csharp
// 默认启用授权保护
app.UseSwaggerPkg(); // requireAuth = true
```

### 2. 禁用授权（仅开发环境）

```csharp
// 禁用授权保护
app.UseSwaggerPkg(requireAuth: false);
```

### 3. 环境条件配置

```csharp
// 仅在生产环境启用授权
var requireAuth = app.Environment.IsProduction();
app.UseSwaggerPkg(requireAuth);
```

## 🔧 高级配置

### 使用配置选项

```csharp
// 在 Program.cs 中配置
builder.Services.ConfigureSwaggerAuth(options => {
    options.RequireAuthentication = true;
    options.OnlyInProduction = true;
    options.UnauthorizedMessage = "请联系管理员获取访问权限";
});

// 使用配置选项
var swaggerOptions = new SwaggerAuthOptions {
    RequireAuthentication = true,
    OnlyInProduction = false
};
app.UseSwaggerAuth(swaggerOptions);
app.UseSwagger();
app.UseSwaggerUI(/* ... */);
```

## 🔑 如何获取访问权限

### 1. 获取 JWT 令牌

首先通过登录接口获取 JWT 令牌：

```bash
# POST /Api/Auth/login
curl -X POST "https://your-domain.com/Api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "your-username",
    "password": "your-password"
  }'
```

响应示例：
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2024-01-01T12:00:00Z"
}
```

### 2. 访问 Swagger UI

有两种方式使用令牌访问 Swagger UI：

#### 方式一：通过浏览器开发者工具

1. 打开浏览器开发者工具（F12）
2. 在 Console 中执行：
```javascript
// 设置 Authorization 头
fetch('/api-docs/swagger', {
  headers: {
    'Authorization': 'Bearer YOUR_JWT_TOKEN_HERE'
  }
}).then(() => {
  // 刷新页面
  location.reload();
});
```

#### 方式二：使用 HTTP 客户端

```bash
# 使用 curl 访问
curl -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  "https://your-domain.com/api-docs/swagger"
```

### 3. 在 Swagger UI 中使用令牌

访问 Swagger UI 后，点击右上角的 "Authorize" 按钮，输入：
```
Bearer YOUR_JWT_TOKEN_HERE
```

## 📋 配置选项说明

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RequireAuthentication` | bool | true | 是否启用认证保护 |
| `OnlyInProduction` | bool | false | 是否仅在生产环境启用 |
| `AllowedRoles` | string[] | [] | 允许的角色列表（暂未实现） |
| `UnauthorizedMessage` | string | null | 自定义未授权消息 |

## 🛡️ 安全最佳实践

### 1. 生产环境配置

```csharp
// 推荐的生产环境配置
if (app.Environment.IsProduction()) {
    // 生产环境必须启用授权
    app.UseSwaggerPkg(requireAuth: true);
} else {
    // 开发环境可选择性启用
    app.UseSwaggerPkg(requireAuth: false);
}
```

### 2. 日志监控

中间件会自动记录以下日志：
- 未授权访问尝试（Warning 级别）
- 成功授权访问（Information 级别）

查看日志示例：
```
[Warning] 未授权访问Swagger UI: /swagger/index.html from 192.168.1.100
[Information] 已授权用户访问Swagger: admin -> /api-docs/swagger
```

### 3. 错误响应格式

未授权访问时返回的 JSON 响应：
```json
{
  "error": "Unauthorized",
  "message": "访问Swagger UI需要有效的JWT令牌认证",
  "details": "请先通过 /Api/Auth/login 接口获取JWT令牌，然后在请求头中添加 'Authorization: Bearer {token}'",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

## 🔍 故障排除

### 常见问题

1. **访问 Swagger 时显示 401 错误**
   - 确认已获取有效的 JWT 令牌
   - 检查令牌是否已过期
   - 确认请求头格式正确：`Authorization: Bearer {token}`

2. **令牌有效但仍无法访问**
   - 检查 JWT 配置是否正确
   - 确认认证中间件的顺序正确
   - 查看应用程序日志获取详细错误信息

3. **开发环境想要禁用授权**
   ```csharp
   app.UseSwaggerPkg(requireAuth: false);
   ```

### 调试技巧

1. **启用详细日志**
   ```json
   // appsettings.Development.json
   {
     "Logging": {
       "LogLevel": {
         "StarBlog.Web.Middlewares.SwaggerAuthMiddleware": "Debug"
       }
     }
   }
   ```

2. **检查中间件顺序**
   确保在 `Program.cs` 中的顺序正确：
   ```csharp
   app.UseAuthentication();  // 必须在 UseSwaggerPkg 之前
   app.UseAuthorization();   // 必须在 UseSwaggerPkg 之前
   app.UseSwaggerPkg();      // Swagger 配置
   ```

## 📚 相关文档

- [ASP.NET Core JWT 认证](https://docs.microsoft.com/aspnet/core/security/authentication/jwt)
- [Swagger/OpenAPI 文档](https://swagger.io/docs/)
- [StarBlog 认证配置](./auth-configuration.md)

## 🤝 贡献

如果您发现问题或有改进建议，请提交 Issue 或 Pull Request。