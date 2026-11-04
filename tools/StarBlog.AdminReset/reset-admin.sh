#!/bin/sh
set -eu
cd /home/deali/projects/starblog-v2/maintenance/admin-reset
docker cp StarBlog.AdminReset.dll starblog-api-v2:/app/StarBlog.AdminReset.dll >/dev/null
exec docker exec -it --user 1654:1654 starblog-api-v2 dotnet exec --runtimeconfig /app/StarBlog.Api.runtimeconfig.json --depsfile /app/StarBlog.Api.deps.json /app/StarBlog.AdminReset.dll
