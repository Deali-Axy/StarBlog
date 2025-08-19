# SlugGenerator 使用指南

## 功能概述

SlugGenerator 是一个利用 LLM 为博客文章自动生成 URL Slug 的服务。它具有以下特性：

- ✅ **智能 Slug 生成**：使用 AI 将中文标题转换为 SEO 友好的英文 Slug
- ✅ **自动重试机制**：处理临时网络错误
- ✅ **唯一性保证**：确保生成的 Slug 在数据库中唯一
- ✅ **字符清理**：自动清理和规范化生成的 Slug
- ✅ **长度控制**：限制 Slug 长度符合最佳实践
- ✅ **详细日志记录**：便于监控和调试
- ✅ **速率限制保护**：避免触发 API 限制
- ✅ **错误恢复**：单个文章失败不影响整体流程

## 配置说明

在 `appsettings.json` 中配置参数：

```json
{
  "SlugGenerator": {
    "MaxRetries": 3,              // 最大重试次数
    "DelayBetweenRequests": 2000, // 请求间隔(毫秒)
    "MaxSlugLength": 50,          // 最大 Slug 长度
    "TimeoutSeconds": 30          // 超时时间(秒)
  }
}
```

## 使用方法

### 1. 基本使用

```bash
cd tools/DataProc
dotnet run
# 选择选项 3: SlugGenerator (URL Slug 生成)
```

### 2. 监控运行状态

程序会输出详细的运行日志：

```
[INFO] 开始生成文章 Slug - 待处理: 15, 总数: 100
[INFO] 开始为文章 [ASP.NET Core 开发笔记] 生成 Slug
aspnet-core-development-notes
[INFO] 文章 [post-123] Slug 生成成功: aspnet-core-development-notes
[INFO] Slug 生成完成 - 成功: 14, 失败: 1
```

## 功能特性

### 1. 智能翻译和转换

- 将中文标题准确翻译为英文
- 保留技术术语和专有名词
- 转换为小写字母
- 用连字符替换空格和标点

### 2. 字符清理和规范化

- 只保留字母、数字和连字符
- 移除连续的连字符
- 去除开头和结尾的连字符
- 移除引号和其他特殊字符

### 3. 唯一性保证

- 检查数据库中是否已存在相同 Slug
- 自动添加数字后缀确保唯一性
- 防止无限循环的安全机制

### 4. 长度控制

- 默认最大长度 50 字符
- 符合 SEO 最佳实践
- 可通过配置调整

## 示例转换

| 原标题 | 生成的 Slug |
|--------|-------------|
| ASP.NET Core 开发笔记 | aspnet-core-development-notes |
| Vue.js 组件设计模式 | vuejs-component-design-patterns |
| 微服务架构实践指南 | microservices-architecture-practice-guide |
| Docker 容器化部署 | docker-containerization-deployment |

## 安全建议

### 1. API 密钥保护

确保 LLM API 密钥安全：

```json
// appsettings.llm.json - 不要提交到版本控制
{
  "LLM": {
    "Key": "your-secret-key",
    "Endpoint": "https://api.provider.com",
    "Model": "model-name"
  }
}
```

### 2. 速率限制设置

根据你的 API 套餐调整速率限制：

- **免费套餐**：通常每分钟 3-5 次请求
- **付费套餐**：可能支持更高频率

### 3. 成本控制

- 使用 `DelayBetweenRequests` 控制请求频率
- 监控 API 使用量和费用
- Slug 生成相对简单，token 消耗较少

## 故障排除

### 常见错误

1. **速率限制错误**
   ```
   Error: Rate limit exceeded
   ```
   **解决方案**：增加 `DelayBetweenRequests` 值

2. **生成的 Slug 为空**
   ```
   Error: 生成的 Slug 为空或无效
   ```
   **解决方案**：检查提示词模板和 LLM 响应

3. **数据库连接错误**
   ```
   Error: 无法连接到数据库
   ```
   **解决方案**：检查连接字符串配置

### 调试技巧

1. **启用详细日志**：设置日志级别为 `Debug`
2. **检查生成过程**：观察控制台输出的流式生成过程
3. **验证配置**：确保所有配置项正确设置

## 最佳实践

1. **批量处理**：建议在低峰时段运行
2. **备份数据**：运行前备份数据库
3. **监控进度**：关注日志输出和成功率
4. **定期检查**：验证生成的 Slug 质量

## 技术实现

- **流式生成**：实时显示 AI 生成过程
- **重试机制**：指数退避策略
- **并发控制**：避免数据库锁定
- **内存优化**：逐个处理文章，避免内存溢出
