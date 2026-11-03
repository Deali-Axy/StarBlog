import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import { createElement } from "react"
import { renderToStaticMarkup } from "react-dom/server"
import ts from "typescript"

const modules = new Map()
async function moduleUrl(path) {
  if (modules.has(path)) return modules.get(path)
  const source = await readFile(new URL(`../${path}`, import.meta.url), "utf8")
  let { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext, jsx: ts.JsxEmit.ReactJSX } })
  for (const [, dependency] of outputText.matchAll(/from "@\/lib\/([\w-]+)"/g)) {
    outputText = outputText.replaceAll(`"@/lib/${dependency}"`, JSON.stringify(await moduleUrl(`lib/${dependency}.ts`)))
  }
  outputText = outputText.replaceAll('"react/jsx-runtime"', JSON.stringify(import.meta.resolve("react/jsx-runtime")))
  const url = `data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`
  modules.set(path, url)
  return url
}

const { parseSearchQuery, searchPageHref } = await import(await moduleUrl("lib/search-query.ts"))
const { HighlightedText, snippetText } = await import(await moduleUrl("components/search/search-text.tsx"))

test("search normalizes empty and duplicate parameters and rejects unsafe page numbers", () => {
  assert.deepEqual(parseSearchQuery({ keyword: "  React  ", page: "2" }), { keyword: "React", page: 2 })
  assert.deepEqual(parseSearchQuery({ keyword: ["React", "Vue"], page: ["2", "3"] }), { keyword: "", page: 1 })
  for (const page of ["0", "-1", "1.5", "NaN", "Infinity", "1000001"]) {
    assert.equal(parseSearchQuery({ page }).page, 1)
  }
})

test("submissions and pagination encode keywords, retain editorial style and reset new searches to page one", () => {
  const keyword = "C# & React 中文"
  const next = new URL(searchPageHref(keyword, 2, "editorial"), "https://blog.invalid")
  assert.equal(next.pathname, "/search")
  assert.equal(next.searchParams.get("keyword"), keyword)
  assert.equal(next.searchParams.get("page"), "2")
  assert.equal(next.searchParams.get("uv"), "editorial")
  const first = new URL(searchPageHref(keyword, 1, "editorial"), "https://blog.invalid")
  assert.equal(first.searchParams.get("page"), null)
  assert.equal(first.searchParams.get("uv"), "editorial")
  const flat = new URL(searchPageHref(keyword), "https://blog.invalid")
  assert.equal(flat.searchParams.get("uv"), null)
})

test("highlighted snippets only render literal mark delimiters and escape all other HTML", () => {
  const html = renderToStaticMarkup(createElement(HighlightedText, { text: '<img src=x onerror=alert(1)> <mark>React</mark> <mark><script>alert(1)</script></mark>' }))
  assert.ok(!html.includes("<img"))
  assert.ok(!html.includes("<script"))
  assert.ok(html.includes("&lt;img"))
  assert.ok(html.includes("&lt;script"))
  assert.equal((html.match(/<mark /g) ?? []).length, 2)
  assert.equal(snippetText("# Intro\n\n[<mark>React</mark>](https://example.com) `hooks`"), "Intro <mark>React</mark> hooks")
})
