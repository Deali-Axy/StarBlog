import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"

const modules = new Map()
async function moduleUrl(name) {
  if (modules.has(name)) return modules.get(name)
  const source = await readFile(new URL(`../lib/${name}.ts`, import.meta.url), "utf8")
  let { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext } })
  for (const [, dependency] of outputText.matchAll(/from "@\/lib\/([\w-]+)"/g)) {
    outputText = outputText.replaceAll(`"@/lib/${dependency}"`, JSON.stringify(await moduleUrl(dependency)))
  }
  const url = `data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`
  modules.set(name, url)
  return url
}
const { parseBlogQuery, updateBlogQuery, blogQueryHref } = await import(await moduleUrl("blog-query"))
const { resolveBlogVariant, BLOG_VARIANT } = await import(await moduleUrl("blog-variant"))
const { categoryPath, filterCategories } = await import(await moduleUrl("blog-categories"))

test("preview can be fixed to the configured style, regardless of URL override", () => {
  assert.equal(resolveBlogVariant("studio"), "studio")
  assert.equal(resolveBlogVariant("invalid"), BLOG_VARIANT)
  assert.equal(resolveBlogVariant("editorial", false), BLOG_VARIANT)
  assert.equal(resolveBlogVariant("studio", false), BLOG_VARIANT)
})

test("untrusted URL values normalize without sending invalid category IDs or pagination to the API", () => {
  const defaults = parseBlogQuery({})
  assert.equal(defaults.page, 1)
  assert.equal(defaults.pageSize, 12)
  assert.equal(defaults.sortBy, "lastUpdateTime")
  assert.equal(defaults.view, "grid")
  for (const invalid of ["NaN", "Infinity", "0", "-5", "1.2", "2147483648", "9007199254740992"]) {
    assert.equal(parseBlogQuery({ page: invalid }).page, 1)
    assert.equal(parseBlogQuery({ categoryId: invalid }).categoryId, undefined)
  }
  assert.equal(parseBlogQuery({ search: "   " }).search, undefined)
  assert.equal(parseBlogQuery({ pageSize: "999" }).pageSize, 100)
  assert.equal(parseBlogQuery({ bv: ["studio", "editorial"] }).bv, BLOG_VARIANT)
})

test("filter and sort changes reset page while view and style changes preserve it", () => {
  const query = parseBlogQuery({ page: "5", pageSize: "6", categoryId: "12", search: "React & C#", sortBy: "title", sortType: "asc", view: "list", bv: "studio" })
  for (const patch of [{ categoryId: 15 }, { categoryId: undefined }, { search: undefined }, { sortBy: "creationTime" }, { sortType: "desc" }, { pageSize: 24 }]) {
    assert.equal(updateBlogQuery(query, patch).page, 1)
  }
  assert.equal(updateBlogQuery(query, { view: "grid" }).page, 5)
  assert.equal(updateBlogQuery(query, { bv: "editorial" }).page, 5)
  const url = new URL(blogQueryHref(query, { categoryId: undefined, search: undefined }), "http://local.test")
  assert.equal(url.searchParams.get("page"), null)
  assert.equal(url.searchParams.get("categoryId"), null)
  assert.equal(url.searchParams.get("search"), null)
  for (const [key, expected] of Object.entries({ pageSize: "6", view: "list", sortBy: "title", sortType: "asc", bv: "studio" })) {
    assert.equal(url.searchParams.get(key), expected)
  }
  const searchUrl = new URL(blogQueryHref(query), "http://local.test")
  assert.equal(searchUrl.searchParams.get("search"), "React & C#")
})

const node = (id, text, nodes = []) => ({ id, text, nodes, href: "", tags: ["0"] })
const tree = [node(1, "Development", [node(2, "Frontend", [node(3, "React")]), node(4, "Backend")]), node(5, "Photography")]

test("deep category lookup includes every ancestor, and name search keeps only matching paths", () => {
  assert.deepEqual(categoryPath(tree, 3).map(node => node.id), [1, 2, 3])
  assert.deepEqual(categoryPath(tree, 999), [])
  const filtered = filterCategories(tree, "  REACT ")
  assert.deepEqual(filtered.map(node => node.id), [1])
  assert.deepEqual(filtered[0].nodes[0].nodes.map(node => node.id), [3])
  assert.deepEqual(filterCategories(tree, "backend")[0].nodes.map(node => node.id), [4])
  assert.deepEqual(filterCategories(tree, "no match"), [])
  assert.equal(filterCategories(tree, ""), tree)
  assert.equal(tree[0].nodes.length, 2, "the source tree remains available when search is cleared")
})
