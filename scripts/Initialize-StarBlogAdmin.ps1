<#
.SYNOPSIS
在 StarBlog.Api 使用的 SQLite 数据库中创建或重置管理员账号。

.DESCRIPTION
此脚本适用于两种场景：全新数据库没有用户时创建首个管理员；数据库已经存在同名用户、
但密码遗失时重置该用户密码。脚本会先创建数据库备份，再在一个事务中更新用户与初始化标记。
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$DatabasePath = (Join-Path $PSScriptRoot "..\apps\api\src\StarBlog.Api\app.db"),

    [Parameter()]
    [ValidatePattern('^[A-Za-z0-9_.-]{3,64}$')]
    [string]$Username = "admin",

    [Parameter()]
    [SecureString]$Password
)

$ErrorActionPreference = "Stop"

# 密码默认通过安全输入框读取，避免明文出现在 PowerShell 历史和进程命令行中。
if (-not $Password) {
    $Password = Read-Host "请输入管理员密码（至少 8 个字符）" -AsSecureString
}

$credential = [System.Management.Automation.PSCredential]::new("starblog-admin", $Password)
$plainPassword = $credential.GetNetworkCredential().Password
if ($plainPassword.Length -lt 8) {
    throw "管理员密码至少需要 8 个字符。"
}

$sqlite = Get-Command sqlite3 -ErrorAction SilentlyContinue
if (-not $sqlite) {
    throw "未找到 sqlite3 命令。请先安装 SQLite CLI，并确保 sqlite3.exe 位于 PATH 中。"
}

$resolvedDatabase = (Resolve-Path -LiteralPath $DatabasePath -ErrorAction Stop).Path
$backupPath = "$resolvedDatabase.admin-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

try {
    # 登录接口使用小写十六进制 SHA-256；这里必须保持同样格式，才能直接写入现有 user 表。
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $passwordBytes = [System.Text.Encoding]::UTF8.GetBytes($plainPassword)
        $passwordHash = [Convert]::ToHexString($sha256.ComputeHash($passwordBytes)).ToLowerInvariant()
    }
    finally {
        $sha256.Dispose()
    }

    # SQLite 的在线备份命令比直接复制数据库文件更可靠，即使数据库使用 WAL 也不会遗漏已提交页面。
    & $sqlite.Source $resolvedDatabase ".backup '$backupPath'"
    if ($LASTEXITCODE -ne 0) {
        throw "创建 SQLite 备份失败，请确认 API 没有独占数据库文件。"
    }

    $newUserId = [Guid]::NewGuid().ToString("N")
    $sql = @"
PRAGMA busy_timeout = 5000;
BEGIN IMMEDIATE;
UPDATE user SET password = '$passwordHash' WHERE name = '$Username';
INSERT INTO user (id, name, password)
SELECT '$newUserId', '$Username', '$passwordHash'
WHERE NOT EXISTS (SELECT 1 FROM user WHERE name = '$Username');
UPDATE config SET value = 'true' WHERE key = 'is_init';
INSERT INTO config (key, value, description)
SELECT 'is_init', 'true', '站点是否已完成初始化'
WHERE NOT EXISTS (SELECT 1 FROM config WHERE key = 'is_init');
COMMIT;
"@

    & $sqlite.Source $resolvedDatabase $sql
    if ($LASTEXITCODE -ne 0) {
        throw "管理员写入失败。请停止 StarBlog.Api 后重试；原数据库备份位于：$backupPath"
    }

    Write-Host "管理员账号 '$Username' 已创建或重置。" -ForegroundColor Green
    Write-Host "数据库：$resolvedDatabase"
    Write-Host "备份：  $backupPath"
}
finally {
    # 尽快释放脚本持有的明文引用；实际密码不会写入终端、日志或脚本参数。
    $plainPassword = $null
    $credential = $null
}
