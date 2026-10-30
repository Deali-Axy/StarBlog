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

Lightbox cases delay original-image responses and decoding to check progressive
previews, stable black loading/error states, rapid navigation, reduced motion,
and the bounded cache. These cases also verify that originals are not requested
until the lightbox opens and that closing stops further adjacent-image preloads.

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

## Blog design regression

`BlogChromeTests` additionally checks the three independent headers/footers on list and article pages at all three viewport widths in both themes. It covers mobile site menus, focus return, back-to-top, English navigation, related/translated article links, canonical URLs, legacy redirects and random-article style preservation. Set `E2E_BLOG_CHROME_SCREENSHOT_DIR` to save header, footer and article screenshots. Include `FullyQualifiedName~BlogChromeTests` in the browser-test filter to run these cases.


`BlogNavigationTests` checks all three designs at 375, 768 and 1440 pixels in both themes, plus category expansion/search, directory-dialog focus, URL history, filtering, sorting and pagination. It also checks the distinct full-width/asymmetric/workbench compositions, desktop archive keyboard selection, mobile article-preview focus return and all three English designs. It expects a populated API with at least two categories that contain children and enough articles for pagination. Set `E2E_SCREENSHOT_DIR` to an absolute directory to save the 18 design screenshots.

For long titles, missing summaries, three category levels and empty-category coverage, create another isolated database:

```sh
python3 tests/StarBlog.E2ETests/prepare_blog_fixture.py \
  apps/api/src/StarBlog.Api/app.db /tmp/starblog-blog-e2e.db
```

Start API and Web against this copy using the isolated-instance configuration above, then run:

```sh
E2E_BASE_URL=http://127.0.0.1:3001 \
E2E_API_BASE_URL=http://127.0.0.1:15039 \
E2E_CHROMIUM_EXECUTABLE_PATH=/usr/bin/google-chrome \
dotnet test tests/StarBlog.E2ETests/StarBlog.E2ETests.csproj \
  --filter FullyQualifiedName~BlogNavigationTests
```

The fixture-specific edge-case test runs when the `Blog E2E root` category is present. Pure URL normalization, page resets, category paths/search, and the disabled-preview setting are covered by `pnpm test:blog` in `apps/web`.

The floating design-preview menu is checked for keyboard selection, Escape/focus return, scoped portal themes, viewport placement and URL/history preservation. It appears on both lists and article pages without taking up header space.
