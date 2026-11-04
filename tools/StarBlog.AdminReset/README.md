# 管理员改名与密码重置

在已部署的 Linux API 容器中交互运行，可同时修改用户名与密码，也可只改其中一项。始终更新原用户记录并保留 ID；用户名需 3–64 个字符，新密码需 8–256 个字符，输入不回显。新名称冲突会取消整个更新。

当前服务器已准备好此工具。用户在自己的终端执行：

```sh
ssh deali.cn
bash /home/deali/projects/starblog-v2/maintenance/admin-reset/reset-admin.sh
```

依次输入：当前用户名（仅一个账号时默认选中）、新用户名（回车保留）、新密码（回车保留原密码）。设置新密码时需要再次确认，工具随后用新用户名和新密码调用容器内 API 验证登录。只改名时使用新用户名和原密码登录。

修改前，使用 SQLite 在线备份 API 在 `/app/data/admin-reset-backups/` 创建完整数据库备份，目录仅 API 用户可访问。工具不会创建新用户，也不需要删除账号或重新初始化站点。账号和密码在同一事务中更新。已有 JWT 会话沿用原到期时间；此工具不轮换站点签名密钥。

## 构建与安装

```sh
dotnet build tools/StarBlog.AdminReset/StarBlog.AdminReset.csproj -c Release
dotnet run --project tools/StarBlog.AdminReset/StarBlog.AdminReset.csproj -c Release --no-build -- --self-test
```

将 `bin/Release/net10.0/StarBlog.AdminReset.dll` 和 `reset-admin.sh` 复制到服务器 `/home/deali/projects/starblog-v2/maintenance/admin-reset/`，该目录权限设为 700。密码仅在用户自己的终端中输入，不应放在命令参数、环境变量或聊天消息中。

脚本先将工具 DLL 复制进现有 API 容器，然后以 API 的 UID/GID 1654 运行，复用容器内 `StarBlog.Api.runtimeconfig.json` 与 `StarBlog.Api.deps.json` 中的运行依赖；无需重启应用或发布新版本镜像。容器重建后再次运行脚本即可。API 升级运行框架或 SQLite 依赖后，应重新构建并验证此工具兼容性。

只读检查与内存自测：

```sh
docker exec --user 1654:1654 starblog-api-v2 dotnet exec --runtimeconfig /app/StarBlog.Api.runtimeconfig.json --depsfile /app/StarBlog.Api.deps.json /app/StarBlog.AdminReset.dll --check
docker exec --user 1654:1654 starblog-api-v2 dotnet exec --runtimeconfig /app/StarBlog.Api.runtimeconfig.json --depsfile /app/StarBlog.Api.deps.json /app/StarBlog.AdminReset.dll --self-test
```

`--check` 仅显示现有管理员名称；`--self-test` 使用内存数据库验证改名、密码哈希、ID 保留及失败回滚，不修改线上账号。
