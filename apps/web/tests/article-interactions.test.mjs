import assert from "node:assert/strict"
import { readFile, access } from "node:fs/promises"
import path from "node:path"
import test from "node:test"
import ts from "typescript"
import { createElement } from "react"
import { renderToStaticMarkup } from "react-dom/server"

// Exercise real layouts, comment sessions and controls with deterministic API data.
const root = new URL("../", import.meta.url)
const modules = new Map()
const mocks = {
  "components/blog/blog-layout-context": `import { createElement } from "react";
    export let variant = "editorial";
    export function setVariant(value) { variant = value }
    export function useBlogLayout() { return { variant } }
    export function BlogStyleLink(props) { return createElement("a", props) }`,
  "features/comments/api": `export let success = true;
    export function setSuccess(value) { success = value }
    export function useComments() { return { isSuccess: success, data: [{ id: "one", visible: true, content: "First", comments: [{ id: "reply", visible: true, content: "Reply" }] }], refetch() {} } }
    export function useCreateComment() { return {} }
    export function useSendCommentOtp() { return {} }`,
  "components/blog/post-extras": `import { createElement } from "react";
    export function PostCopyright({ children }) { return createElement("section", { "data-test-copyright": "" }, children) }
    export function RelatedPosts() { return createElement("section", { "data-test-related": "" }) }`,
}
async function moduleUrl(name) {
  name = name.replace(/\.(tsx?|css)$/, "")
  if(modules.has(name)) return modules.get(name)
  let source = mocks[name]
  if(name === "components/app-providers") source = `import { messages } from "${await moduleUrl("lib/i18n")}"; export function useI18n() { return { locale: "zh", t: messages.zh } }`
  if(!source) {
    let file = `${name}.tsx`
    try { await access(new URL(file, root)) } catch { file = `${name}.ts` }
    source = await readFile(new URL(file, root), "utf8")
  }
  let { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext, jsx: ts.JsxEmit.ReactJSX } })
  outputText = outputText.replace(/^import (\w+) from "[^"\n]+\.css";?$/gm, "const $1 = {};")
  for(const [, specifier] of outputText.matchAll(/from "([^"]+)"/g)) {
    if(specifier.startsWith("data:")) continue
    const resolved = specifier.startsWith("@/") ? await moduleUrl(specifier.slice(2))
      : specifier.startsWith(".") ? await moduleUrl(path.posix.join(path.posix.dirname(name), specifier))
      : import.meta.resolve(specifier)
    outputText = outputText.replaceAll(`"${specifier}"`, JSON.stringify(resolved))
  }
  const url = `data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`
  modules.set(name, url)
  return url
}
const layoutContext = await import(await moduleUrl("components/blog/blog-layout-context"))
const commentApi = await import(await moduleUrl("features/comments/api"))
const { CommentSessionProvider } = await import(await moduleUrl("components/blog/comment-section"))
const { ArticleReadingProvider } = await import(await moduleUrl("components/blog/article-reading"))
const { ArticleMobileActions, ArticleCommentLink } = await import(await moduleUrl("components/blog/article-interactions"))
const layouts = {}
for(const [theme, component] of [["editorial", "FolioArticle"], ["studio", "ArchiveArticle"], ["library", "IndexArticle"]]) {
  layouts[theme] = (await import(await moduleUrl(`components/blog/views/${theme}-article`)))[component]
}
const post = { id: "post", title: "Reading", summary: "Summary", creationTime: "2026-11-17T00:00:00Z", lastUpdateTime: "2026-11-17T00:00:00Z" }
function render(theme, headings) {
  layoutContext.setVariant(theme)
  return renderToStaticMarkup(createElement(CommentSessionProvider, { postId: post.id },
    createElement(ArticleReadingProvider, { items: headings, layoutKey: theme },
      createElement(layouts[theme], { post, headings, minutes: 5, permalink: "/blog/post" }, createElement("p", null, "Article body")),
      createElement(ArticleMobileActions, { title: post.title }))))
}

test("all themes expose comments and sharing with or without a table of contents", () => {
  for(const theme of Object.keys(layouts)) {
    for(const headings of [[], [{ id: "intro", depth: 2, text: "Introduction" }]]) {
      const html = render(theme, headings)
      assert.equal((html.match(/id="article-comments"/g) ?? []).length, 1, theme)
      assert.ok(html.includes('data-article-interactions="sidebar"'), theme)
      const header = html.match(/<header\b[^>]*>([\s\S]*?)<\/header>/)?.[1] ?? ""
      assert.ok(!header.includes('href="#article-comments"'), "header does not duplicate sidebar actions")
      assert.ok(!header.includes('<button'), "header stays focused on article metadata")
      assert.ok(html.includes('data-article-interactions="end"'), theme)
      assert.ok(html.includes("data-article-mobile-actions"), theme)
      assert.ok((html.match(/href="#article-comments"/g) ?? []).length >= 3, theme)
      assert.ok(html.includes('tabular-nums">2</span>'), "counts replies as well as root comments")
      assert.ok(html.indexOf('data-test-copyright') < html.indexOf('id="article-comments"'), theme)
      assert.ok(html.indexOf('id="article-comments"') < html.indexOf('data-test-related'), theme)
      assert.ok(html.indexOf('<textarea') < html.indexOf('type="email"'), "writing comes before verification")
      assert.match(html, /<div hidden="" class="space-y-3">/, "verification starts collapsed")
    }
  }
})

test("comment shortcuts avoid showing a false zero before comments load", () => {
  commentApi.setSuccess(false)
  const html = renderToStaticMarkup(createElement(CommentSessionProvider, { postId: post.id }, createElement(ArticleCommentLink)))
  assert.ok(html.includes('href="#article-comments"'))
  assert.ok(!html.includes('tabular-nums'))
  commentApi.setSuccess(true)
})

const { useArticleActions } = await import(await moduleUrl("components/blog/article-actions"))
test("sharing omits preview theme and scroll position while preserving article language", async () => {
  const previousWindow = globalThis.window
  const previousNavigator = Object.getOwnPropertyDescriptor(globalThis, "navigator")
  let shared
  globalThis.window = { location: { href: "https://example.com/blog/post?bv=studio&lang=en#article-comments" } }
  Object.defineProperty(globalThis, "navigator", { configurable: true, value: { share: async value => { shared = value } } })
  try {
    await useArticleActions().share("Reading")
    assert.deepEqual(shared, { title: "Reading", url: "https://example.com/blog/post?lang=en" })
  } finally {
    if(previousWindow === undefined) delete globalThis.window
    else globalThis.window = previousWindow
    if(previousNavigator) Object.defineProperty(globalThis, "navigator", previousNavigator)
    else delete globalThis.navigator
  }
})
