param(
  [string]$WebRoot = "",
  [string]$OutputRoot = "",
  [int]$Retention = 30,
  [switch]$NoZip
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($WebRoot)) {
  $WebRoot = (Join-Path $repoRoot "apps\web-legacy")
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
  $OutputRoot = (Join-Path $repoRoot "backups\StarBlog.Web")
}

$project = (Join-Path $repoRoot "tools\StarBlogBackup\StarBlog.BackupTool.csproj")

$argsList = @("run", "--project", $project, "--", "backup", "--webRoot", $WebRoot, "--outputRoot", $OutputRoot, "--retention", $Retention)
if ($NoZip) {
  $argsList += "--no-zip"
}

dotnet @argsList
