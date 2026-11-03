import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"
import { renderToStaticMarkup } from "react-dom/server"

// As in the existing tests, use the installed TypeScript toolchain without a runner.
const modules = new Map()
async function moduleUrl(path) {
  if (modules.has(path)) return modules.get(path)
  const source = await readFile(new URL(`../${path}`, import.meta.url), "utf8")
  let { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext, jsx: ts.JsxEmit.ReactJSX } })
  outputText = outputText.replace(/^import "(?:server-only|[^"]+\.css)";?$/gm, "")
    .replace(/^import styles from "[^"]+\.css";?$/gm, "const styles = {};")
  for (const [, specifier] of outputText.matchAll(/from "([^"]+)"/g)) {
    const resolved = specifier.startsWith("@/") ? await moduleUrl(`${specifier.slice(2)}.ts`) : import.meta.resolve(specifier)
    outputText = outputText.replaceAll(`"${specifier}"`, JSON.stringify(resolved))
  }
  const url = `data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`
  modules.set(path, url)
  return url
}
const { renderArticleMarkdown } = await import(await moduleUrl("components/blog/markdown.tsx"))
const { articleImageUrl, currentHeadingIndex, headingIdFromHash } = await import(await moduleUrl("lib/markdown.ts"))
function render(source) {
  const result = renderArticleMarkdown(source)
  return { ...result, html: renderToStaticMarkup(result.content) }
}

test("legacy article and translation images resolve within the original post media directory", () => {
  assert.equal(articleImageUrl("image.jpg", "abc"), "/media/blog/abc/image.jpg")
  assert.equal(articleImageUrl("./文章.assets\\image.jpg", "abc"), "/media/blog/abc/文章.assets/image.jpg")
  for (const src of ["https://example.com/a.jpg", "//cdn.example.com/a.jpg", "/media/a.jpg", "#image"]) {
    assert.equal(articleImageUrl(src, "abc"), src)
  }
  assert.equal(articleImageUrl("../../private", "abc"), "")
  const result = renderArticleMarkdown("![图](image.jpg)", "abc")
  assert.match(renderToStaticMarkup(result.content), /src="\/media\/blog\/abc\/image.jpg"/)
})

test("TOC targets equal rendered IDs for Chinese, punctuation, duplicates, inline code and all heading levels", () => {
  const { headings, html } = render([
    "# 开始阅读", "## C# 与 .NET：实战！", "### 使用 `await` 与 **异步** [链接](https://example.com)",
    "## 重复标题", "## 重复标题", "#### 跳级", "##### 五级", "###### 六级", "下划线标题\n---",
    "```md\n## 不是标题\n```", "> ## 引用中的标题", "## ![图片说明](/media/example.jpg)",
  ].join("\n\n"))
  assert.equal(headings.length, 11)
  assert.equal(new Set(headings.map(item => item.id)).size, headings.length)
  assert.deepEqual(headings.slice(0, 3).map(item => item.depth), [1, 2, 3])
  assert.equal(headings[2].text, "使用 await 与 异步 链接")
  assert.equal(headings.at(-1).text, "图片说明")
  assert.ok(headings.some(item => item.depth === 6))
  assert.ok(!headings.some(item => item.text === "不是标题"))
  for (const item of headings) {
    assert.ok(html.includes(`id="${item.id}"`), `Missing target: ${item.id}`)
    assert.equal(headingIdFromHash(`#${encodeURIComponent(item.id)}`), item.id)
  }
  assert.ok(html.includes("hljs"), "code is highlighted during server rendering")
})

test("empty heading IDs get unique targets without colliding with existing section IDs", () => {
  const { headings, html } = render("# !!!\n\n# section-1\n\n# !!!\n\n# section-3\n\n# ???")
  assert.equal(new Set(headings.map(item => item.id)).size, 5)
  for (const item of headings) {
    assert.ok(item.id)
    assert.ok(html.includes(`id="${item.id}"`))
  }
})

test("each document starts fresh and translated headings come from translated content", () => {
  assert.deepEqual(render("## 重复\n\n## 重复").headings.map(item => item.id), ["重复", "重复-1"])
  assert.equal(render("## 重复").headings[0].id, "重复")
  assert.equal(render("## Translated section").headings[0].id, "translated-section")
  assert.deepEqual(render("").headings, [])
  assert.deepEqual(render("Plain text without headings").headings, [])
})

test("GFM tables, task lists and footnotes work while raw HTML and unsafe URLs stay inert", () => {
  const { html, headings } = render([
    "## Read safely", "| A | B |\n| :- | -: |\n| One | Two |", "- [x] Task", "A note[^1].\n\n[^1]: Footnote text.",
    '<script>alert("xss")</script>', "[unsafe](javascript:alert(1))", '<h2 id="injected">Raw heading</h2>',
  ].join("\n\n"))
  assert.match(html, /<table\b[^>]*>/)
  assert.ok(html.includes('type="checkbox"'))
  assert.ok(html.includes("Footnote text."))
  assert.ok(html.includes('id="user-content-fnref-1"'), "footnotes retain their return targets")
  assert.ok(!html.includes("<script>"))
  assert.ok(!html.includes('href="javascript:'))
  assert.deepEqual(headings.map(item => item.text), ["Read safely"])
})

test("scroll tracking stays active between sections, handles the end and decodes hashes safely", () => {
  assert.equal(currentHeadingIndex([], 180), -1)
  assert.equal(currentHeadingIndex([300, 900, 1600], 180), 0)
  assert.equal(currentHeadingIndex([-1200, -400, 500], 180), 1)
  assert.equal(currentHeadingIndex([-1200, -400, 500], 180, true), 2)
  assert.equal(headingIdFromHash("#%E4%B8%AD%E6%96%87"), "中文")
  assert.equal(headingIdFromHash("#bad%encoding"), "bad%encoding")
})
