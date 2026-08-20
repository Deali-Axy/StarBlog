# StarBlog.Web 备份工具

该工具用于备份 `apps/web-legacy` 的关键可变数据：
- SQLite：`app.db`（必备）与 `app.log.db`（可选）
- 媒体资源：`wwwroot/media/blog`、`wwwroot/media/photography`

默认输出为 zip 备份包，并可按数量做保留策略清理。

## 1. 直接执行（推荐）

在仓库根目录执行：

```powershell
pwsh -File .\tools\StarBlogBackup\backup-starblog-web.ps1
```

指定输出目录与保留份数：

```powershell
pwsh -File .\tools\StarBlogBackup\backup-starblog-web.ps1 -OutputRoot D:\Backups\StarBlog -Retention 14
```

不打包 zip（输出为目录）：

```powershell
pwsh -File .\tools\StarBlogBackup\backup-starblog-web.ps1 -NoZip
```

## 2. 直接运行工具

```powershell
dotnet run --project .\tools\StarBlogBackup\StarBlog.BackupTool.csproj -- backup
```

自定义参数：

```powershell
dotnet run --project .\tools\StarBlogBackup\StarBlog.BackupTool.csproj -- backup `
  --webRoot C:\code\starblog\starblog\apps\web-legacy `
  --outputRoot D:\Backups\StarBlog `
  --retention 30
```

## 3. 计划任务（Windows Task Scheduler）

建议用“每天/每周”调度 `pwsh`，并指向脚本：

程序/脚本：
```
pwsh
```

添加参数：
```
-NoProfile -ExecutionPolicy Bypass -File "C:\code\starblog\starblog\tools\StarBlogBackup\backup-starblog-web.ps1" -OutputRoot "D:\Backups\StarBlog" -Retention 30
```

起始于（可选）：
```
C:\code\starblog\starblog
```

## 4. 恢复（可选）

恢复会覆盖 `StarBlog.Web` 下同名文件，务必在站点停止时执行：

```powershell
dotnet run --project .\tools\StarBlogBackup\StarBlog.BackupTool.csproj -- restore `
  --webRoot C:\code\starblog\starblog\apps\web-legacy `
  --input D:\Backups\StarBlog\StarBlog.Web_20260212_120000.zip `
  --overwrite
```
