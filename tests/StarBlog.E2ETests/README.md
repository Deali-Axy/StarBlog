# Browser tests

Start the API and Web, then run from the repository root:

```sh
E2E_BASE_URL=http://127.0.0.1:3001 \
E2E_CHROMIUM_EXECUTABLE_PATH=/usr/bin/google-chrome \
dotnet test tests/StarBlog.E2ETests/StarBlog.E2ETests.csproj
```

Omit `E2E_CHROMIUM_EXECUTABLE_PATH` to use Playwright's installed Chromium. The
photography tests expect at least nine photos. The pagination case exercises
failure/retry when more than 48 photos exist; smaller galleries exercise the
completed state instead.

For complete pagination and long-title coverage, make an isolated database copy:

```sh
python3 tests/StarBlog.E2ETests/prepare_photography_fixture.py \
  apps/api/src/StarBlog.Api/app.db /tmp/starblog-photography-e2e.db
```

Run an API instance from its normal working directory, with
`ConnectionStrings__Default='Data Source=/tmp/starblog-photography-e2e.db'`,
`ASPNETCORE_URLS=http://127.0.0.1:15039`, and `Outbox__BatchSize=0`. It reuses the
existing media files and changes only the temporary database. Start Web with
`INTERNAL_API_BASE_URL=http://127.0.0.1:15039`, and set
`E2E_API_BASE_URL=http://127.0.0.1:15039` when running tests so browser requests
use that same API even if testing a production build with precompiled rewrites.

Pure photo-layout checks use the Web app's existing TypeScript dependency:

```sh
cd apps/web
pnpm test:photography
```
