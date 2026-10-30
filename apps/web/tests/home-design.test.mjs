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

const { HOME_VARIANT, parseHomeVariant, resolveHomePreferences, serializeHomePreferences, homeDesignHref, groupHomePostsByYear } = await import(await moduleUrl("home-variant"))
const { FEATURED_CATEGORIES_VARIANT } = await import(await moduleUrl("featured-categories-variant"))

test("explicit links override saved choices independently; absent or invalid values fall back safely", () => {
  assert.deepEqual(resolveHomePreferences({}, "desk.tilt"), { variant: "desk", categories: "tilt" })
  assert.deepEqual(resolveHomePreferences({ hv: "editorial" }, "desk.tilt"), { variant: "editorial", categories: "tilt" })
  assert.deepEqual(resolveHomePreferences({ fc: "accordion" }, "desk.tilt"), { variant: "desk", categories: "accordion" })
  assert.deepEqual(resolveHomePreferences({ hv: ["desk"], fc: ["rail"] }, "editorial.tilt"), { variant: "editorial", categories: "tilt" })
  assert.deepEqual(resolveHomePreferences({ hv: "toString", fc: "__proto__" }, "broken.corrupt"), { variant: HOME_VARIANT, categories: FEATURED_CATEGORIES_VARIANT })
  assert.equal(parseHomeVariant(null), null)
  assert.equal(parseHomeVariant("unknown-future-style"), null)
})

test("all category styles remain selectable independently of the old inline-preview flag", () => {
  for (const categories of ["deck", "rail", "catalog", "accordion", "tilt"]) {
    const preferences = { variant: "desk", categories }
    assert.deepEqual(resolveHomePreferences({}, serializeHomePreferences(preferences)), preferences)
    assert.deepEqual(resolveHomePreferences({ fc: categories }, "desk.accordion"), preferences)
  }
})

test("history entries carry both choices and preserve unrelated queries and anchors", () => {
  const original = "/?keep=C%23&fc=deck&fc=tilt#home-latest-title"
  const before = homeDesignHref(original, { variant: "editorial", categories: "accordion" })
  const after = homeDesignHref(before, { variant: "desk", categories: "rail" })
  for (const [href, variant, categories] of [[before, "editorial", "accordion"], [after, "desk", "rail"]]) {
    const url = new URL(href, "https://test.invalid")
    assert.equal(url.searchParams.get("keep"), "C#")
    assert.equal(url.hash, "#home-latest-title")
    assert.deepEqual(url.searchParams.getAll("fc"), [categories])
    assert.deepEqual(resolveHomePreferences(Object.fromEntries(url.searchParams), "desk.tilt"), { variant, categories })
  }
})

test("year groups handle a year boundary and missing dates without changing the input", () => {
  const posts = [
    { id: "a", lastUpdateTime: "2026-01-01T00:00:00Z" },
    { id: "b", lastUpdateTime: "2025-12-31T23:00:00Z" },
    { id: "c", lastUpdateTime: "2025-01-10T00:00:00Z" },
    { id: "d", lastUpdateTime: "" },
  ]
  const original = structuredClone(posts)
  const groups = groupHomePostsByYear(posts)
  assert.deepEqual(groups.map(group => [group.year, group.items.map(post => post.id)]), [["2026", ["a"]], ["2025", ["b", "c"]], ["", ["d"]]])
  assert.deepEqual(posts, original)
  assert.deepEqual(groupHomePostsByYear([]), [])
})
