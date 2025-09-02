# SummaryGenerator 使用指南

## 功能概述

SummaryGenerator 是一个利用 LLM 为博客文章自动生成摘要的服务。它具有以下特性：

- ✅ **速率限制保护**：避免触发 API 限制
- ✅ **自动重试机制**：处理临时网络错误
- ✅ **内容长度控制**：防止 token 超限
- ✅ **详细日志记录**：便于监控和调试
- ✅ **可配置参数**：灵活调整运行参数
- ✅ **错误恢复**：单个文章失败不影响整体流程

## 配置说明

在 `appsettings.summary.json` 中配置参数：

```json
{
  "SummaryGenerator": {
    "MaxRetries": 3,              // 最大重试次数
    "DelayBetweenRequests": 2000, // 请求间隔(毫秒)
    "MaxContentLength": 8000,     // 最大内容长度
    "BatchSize": 10,              // 批处理大小
    "EnableParallelProcessing": false, // 是否并行处理
    "MaxConcurrency": 2,          // 最大并发数
    "TimeoutSeconds": 60          // 超时时间(秒)
  }
}
```

## 使用方法

### 1. 基本使用

```bash
cd tools/DataProc
dotnet run -- summary
```

### 2. 监控运行状态

程序会输出详细的运行日志：

```
[INFO] 开始生成文章摘要 - 待处理: 25, 总数: 100
[INFO] 开始为文章 [ASP.NET Core 开发笔记] 生成摘要
[INFO] 文章 [post-123] 摘要生成成功
[WARN] 达到速率限制，等待 1.5 秒
[INFO] 摘要生成完成 - 成功: 23, 失败: 2
```

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

- 设置合理的 `MaxContentLength` 避免过多 token 消耗
- 使用 `DelayBetweenRequests` 控制请求频率
- 监控 API 使用量和费用

## 故障排除

### 常见错误

1. **速率限制错误**
   ```
   Error: Rate limit exceeded
   ```
   **解决方案**：增加 `DelayBetweenRequests` 值

2. **Token 超限错误**
   ```
   Error: Maximum context length exceeded
   ```
   **解决方案**：减少 `MaxContentLength` 值

3. **网络超时**
   ```
   Error: Request timeout
   ```
   **解决方案**：增加 `TimeoutSeconds` 值

### 调试技巧

1. **启用详细日志**：
   ```json
   {
     "Logging": {
       "LogLevel": {
         "DataProc.Services.SummaryGenerator": "Debug"
       }
     }
   }
   ```

2. **测试单个文章**：
   修改代码临时限制处理数量：
   ```csharp
   var posts = await postRepo.Where(e => string.IsNullOrWhiteSpace(e.Summary))
       .Take(1) // 只处理一篇文章
       .ToListAsync();
   ```

## 性能优化

### 1. 批量处理

对于大量文章，考虑分批处理：

```csharp
// 分批处理，避免长时间运行
var batches = posts.Chunk(settings.BatchSize);
foreach (var batch in batches) {
    // 处理当前批次
    await ProcessBatch(batch);
    
    // 批次间休息
    await Task.Delay(TimeSpan.FromMinutes(1));
}
```

### 2. 并行处理（谨慎使用）

如果 API 支持较高并发：

```csharp
if (settings.EnableParallelProcessing) {
    var semaphore = new SemaphoreSlim(settings.MaxConcurrency);
    var tasks = posts.Select(async post => {
        await semaphore.WaitAsync();
        try {
            return await ProcessPost(post);
        }
        finally {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}
```

## 测试建议

### 1. 单元测试

```csharp
[Test]
public async Task GenerateSummary_ShouldHandleEmptyContent() {
    // 测试空内容处理
    var post = new Post { Content = "" };
    var result = await summaryGenerator.GenerateSummaryWithRetry(post);
    Assert.That(result.IsFailed);
}
```

### 2. 集成测试

```csharp
[Test]
public async Task GenerateSummary_ShouldRespectRateLimit() {
    // 测试速率限制
    var startTime = DateTime.UtcNow;
    
    await summaryGenerator.Run();
    
    var duration = DateTime.UtcNow - startTime;
    var expectedMinDuration = TimeSpan.FromMilliseconds(
        posts.Count * settings.DelayBetweenRequests);
    
    Assert.That(duration, Is.GreaterThan(expectedMinDuration));
}
```

### 3. 手动测试

1. 准备测试数据：创建几篇没有摘要的测试文章
2. 运行生成器：观察日志输出和错误处理
3. 验证结果：检查生成的摘要质量和数据库更新

## 监控和维护

### 1. 日志监控

定期检查日志文件：
- 成功率统计
- 错误模式分析
- 性能指标监控

### 2. 数据库监控

```sql
-- 检查摘要生成进度
SELECT 
    COUNT(*) as total_posts,
    COUNT(Summary) as posts_with_summary,
    COUNT(*) - COUNT(Summary) as posts_without_summary
FROM Posts;

-- 检查最近生成的摘要
SELECT Id, Title, Summary, LastUpdateTime 
FROM Posts 
WHERE Summary IS NOT NULL 
ORDER BY LastUpdateTime DESC 
LIMIT 10;
```

### 3. 成本监控

- 定期检查 API 使用量
- 设置使用量告警
- 优化提示词以减少 token 消耗
