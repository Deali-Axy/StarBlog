<#
.SYNOPSIS
通过 StarBlog.Api 首次初始化接口创建管理员账号。

.DESCRIPTION
重构后的密码使用 ASP.NET Identity PBKDF2 哈希，不能再直接写入 SQLite。
此脚本在用户表为空时调用 /api/v1/site/initialization；若站点已初始化，请使用管理端登录或重置流程。
#>
[CmdletBinding()]
param(
    [Parameter()]
    [string]$ApiBase = "http://localhost:5039",

    [Parameter()]
    [ValidatePattern('^[A-Za-z0-9_.-]{3,64}$')]
    [string]$Username = "admin",

    [Parameter()]
    [SecureString]$Password,

    [Parameter()]
    [string]$HostName = "http://localhost:5039"
)

$ErrorActionPreference = "Stop"

if (-not $Password) {
    $Password = Read-Host "请输入管理员密码（至少 8 个字符）" -AsSecureString
}

$credential = [System.Management.Automation.PSCredential]::new("starblog-admin", $Password)
$plainPassword = $credential.GetNetworkCredential().Password
if ($plainPassword.Length -lt 8) {
    throw "管理员密码至少需要 8 个字符。"
}

try {
    $state = Invoke-RestMethod -Uri "$ApiBase/api/v1/site/initialization" -Method Get
    if ($state.isInitialized) {
        throw "站点已经完成初始化，不能再次创建首个管理员。"
    }

    Invoke-RestMethod -Uri "$ApiBase/api/v1/site/initialization" -Method Post -ContentType "application/json" -Body (@{
        username = $Username
        password = $plainPassword
        host = $HostName
        defaultRender = "frontend"
    } | ConvertTo-Json)

    Write-Host "管理员账号 '$Username' 已通过 API 初始化。" -ForegroundColor Green
}
finally {
    $plainPassword = $null
    $credential = $null
}
