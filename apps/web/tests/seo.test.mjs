import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"

const modules = new Map()
async function moduleUrl(path) {
  if (modules.has(path)) return modules.get(path)
  const source = await readFile(new URL(`../${path}`, import.meta.url), "utf8")
  let { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext, target: ts.ScriptTarget.ES2022 } })
  for (const [, specifier] of outputText.matchAll(/from "([^"]+)"/g)) {
    const resolved = specifier === "@/lib/api/queries"
      ? `data:text/javascript,${encodeURIComponent('export const listPosts = q => globalThis.seoApi.listPosts(q); export const listPhotos = q => globalThis.seoApi.listPhotos(q); export const getCategoryTree = () => globalThis.seoApi.getCategoryTree();')}`
      : specifier.startsWith("@/") ? await moduleUrl(`${specifier.slice(2)}.ts`) : import.meta.resolve(specifier)
    outputText = outputText.replaceAll(`"${specifier}"`, JSON.stringify(resolved))
  }
  const url = `data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`
  modules.set(path, url)
  return url
}
const { pageMetadata, blogMetadata, plainDescription, articlePath, articleLanguages, serializeJsonLd } = await import(await moduleUrl("lib/seo.ts"))
const { parseBlogQuery } = await import(await moduleUrl("lib/blog-query.ts"))
const { sitemapIndex, sitemapChunk, xmlEscape } = await import(await moduleUrl("lib/sitemap.ts"))
const site = (await import(await moduleUrl("lib/site.ts"))).site
const categories = [{ id: 4, text: "Development", tags: ["20"], nodes: [{ id: 5, text: "C# & .NET", tags: ["10"] }] }]

test("every page supplies independent titles, canonical URLs, descriptions and share cards", () => {
  for (const locale of ["zh", "en"]) {
    const metadata = pageMetadata({ locale, title: "About", description: "About the author", path: "/about" })
    assert.equal(metadata.title.absolute, `About · ${site.name}`)
    assert.equal(metadata.alternates.canonical, `${site.url}/about`)
    assert.equal(metadata.openGraph.url, metadata.alternates.canonical)
    assert.equal(metadata.openGraph.title, metadata.title.absolute)
    assert.equal(metadata.twitter.title, metadata.title.absolute)
    assert.equal(metadata.twitter.description, metadata.description)
    assert.ok(metadata.openGraph.images[0].url.startsWith("http"))
    assert.ok(metadata.twitter.images[0].startsWith("http"))
    assert.equal(metadata.robots.index, true)
    assert.equal(metadata.openGraph.locale, locale === "en" ? "en_US" : "zh_CN")
  }
})

test("category and paginated canonical URLs retain content parameters and discard appearance", () => {
  const query = parseBlogQuery({ categoryId: "5", page: "2", pageSize: "6", bv: "studio", view: "list" })
  const metadata = blogMetadata("zh", query, categories)
  const canonical = new URL(metadata.alternates.canonical)
  assert.equal(canonical.pathname, "/blog")
  assert.equal(canonical.searchParams.get("categoryId"), "5")
  assert.equal(canonical.searchParams.get("page"), "2")
  assert.equal(canonical.searchParams.get("pageSize"), "6")
  assert.equal(canonical.searchParams.has("view"), false)
  assert.equal(canonical.searchParams.has("bv"), false)
  assert.match(metadata.title.absolute, /C# & \.NET/)
  assert.match(metadata.title.absolute, /第 2 页/)
  assert.equal(metadata.robots.index, true)
})

test("search, unknown categories and alternate sorting are not indexed; sorted pages retain their own identity", () => {
  for (const params of [{ search: "A & B" }, { categoryId: "9999" }, { sortBy: "title", page: "2" }, { sortType: "asc" }]) {
    const metadata = blogMetadata("en", parseBlogQuery(params), categories)
    assert.equal(metadata.robots.index, false)
    assert.equal(metadata.robots.follow, true)
    if (params.sortBy) assert.equal(new URL(metadata.alternates.canonical).searchParams.get("sortBy"), "title")
  }
})

test("translated articles use their own canonical and reciprocal language alternatives", () => {
  const post = { id: "123", slug: "article" }
  assert.equal(articlePath(post), "/blog/article")
  assert.equal(articlePath({ ...post, language: "en" }), "/blog/article?lang=en")
  const languages = articleLanguages(post, [{ language: "en" }, { language: "ja" }, { language: "bad<script>" }])
  assert.equal(languages["zh-CN"], `${site.url}/blog/article`)
  assert.equal(languages["x-default"], languages["zh-CN"])
  assert.equal(languages.en, `${site.url}/blog/article?lang=en`)
  assert.equal(languages.ja, `${site.url}/blog/article?lang=ja`)
  assert.equal(Object.hasOwn(languages, "bad<script>"), false)
})

test("empty summaries fall back, Markdown is readable, and Unicode is not split", () => {
  assert.equal(plainDescription("  ", "Fallback"), "Fallback")
  assert.equal(plainDescription("## Hello **world**\n[Read more](https://example.com) ![Photo](x.png)", "Fallback"), "Hello world Read more")
  const result = plainDescription("😀".repeat(180), "Fallback")
  assert.equal(Array.from(result).length, 160)
  assert.equal(result.includes("\uFFFD"), false)
})

test("structured data cannot terminate its script element", () => {
  const data = { name: '</script><script>alert("x")</script>', description: "A & B" }
  const serialized = serializeJsonLd(data)
  assert.equal(serialized.includes("<"), false)
  assert.deepEqual(JSON.parse(serialized), data)
})

function page(items, number, pageSize) {
  return { items: items.slice((number - 1) * pageSize, number * pageSize), totalCount: items.length, totalPages: Math.ceil(items.length / pageSize), page: number, pageSize }
}
const posts = Array.from({ length: 1015 }, (_, i) => ({ id: String(i), slug: `post-${i}`, isPublish: true, lastUpdateTime: "2026-01-02T00:00:00Z" }))
const photos = Array.from({ length: 105 }, (_, i) => ({ id: String(i), title: 'Photo < & " >', filePath: "目录/photo & image.jpg" }))
globalThis.seoApi = {
  listPosts: async q => page(posts, q.page, q.pageSize),
  listPhotos: async q => page(photos, q.page, q.pageSize),
  getCategoryTree: async () => categories,
}

test("sitemaps use the public origin and advertise bounded chunks", async () => {
  const xml = await sitemapIndex()
  assert.ok(xml.includes(`${site.url}/sitemaps/pages/1.xml`))
  assert.ok(xml.includes(`${site.url}/sitemaps/posts/2.xml`))
  assert.equal(xml.includes("/sitemaps/posts/3.xml"), false)
  assert.ok(xml.includes(`${site.url}/sitemaps/images/1.xml`))
  assert.equal(xml.includes("localhost"), false)
  const images = await sitemapIndex(true)
  assert.equal(images.includes("/posts/"), false)
})

test("post sitemap chunks have no gaps or overlap at API and sitemap boundaries", async () => {
  const first = await sitemapChunk("posts", 1)
  const second = await sitemapChunk("posts", 2)
  assert.equal((first.match(/<url>/g) || []).length, 1000)
  assert.equal((second.match(/<url>/g) || []).length, 15)
  assert.ok(first.includes("/blog/post-999</loc>"))
  assert.equal(first.includes("/blog/post-1000</loc>"), false)
  assert.ok(second.includes("/blog/post-1000</loc>"))
  assert.ok(second.includes("<lastmod>2026-01-02T00:00:00.000Z</lastmod>"))
  assert.equal(await sitemapChunk("posts", 3), null)
  for (const number of [0, -1, NaN, Infinity, 1.1, 1000001]) assert.equal(await sitemapChunk("posts", number), null)
  assert.equal(await sitemapChunk("unknown", 1), null)
})

test("photo sitemaps escape XML and encode image path segments", async () => {
  const xml = await sitemapChunk("images", 1)
  assert.equal((xml.match(/<url>/g) || []).length, 105)
  assert.ok(xml.includes('Photo &lt; &amp; &quot; &gt;'))
  assert.ok(xml.includes('/media/photography/%E7%9B%AE%E5%BD%95/photo%20%26%20image.jpg'))
  assert.equal(xmlEscape("< & > \" '"), "&lt; &amp; &gt; &quot; &apos;")
})

test("page sitemap exposes categories and paginated photo catalog without invented modification dates", async () => {
  const xml = await sitemapChunk("pages", 1)
  assert.ok(xml.includes("/blog?categoryId=5"), xml)
  assert.ok(xml.includes("/photography/catalog?page=3"))
  assert.equal(xml.includes("<lastmod>"), false)
  assert.equal(xml.includes("/search"), false)
})
