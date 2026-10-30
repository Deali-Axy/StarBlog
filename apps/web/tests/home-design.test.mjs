import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"
import { createRequire } from "node:module"
import { readFileSync, existsSync } from "node:fs"
import { fileURLToPath } from "node:url"
import { runInNewContext } from "node:vm"

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

// Render real sections, replacing only the browser providers and Next's link.
const require = createRequire(import.meta.url)
const React = require("react")
const { renderToStaticMarkup } = require("react-dom/server")

function homeRenderer(variant) {
  const cache = new Map()
  const translations = new Proxy({}, { get: (_, key) => String(key) })
  function load(name) {
    if (name === "@/components/app-providers") return { useI18n: () => ({ t: translations, locale: "zh" }) }
    if (name === "@/components/home/home-design-context") return { useHomeDesign: () => ({ variant }) }
    if (name === "next/link") return { __esModule: true, default: ({ children, ...props }) => React.createElement("a", props, children) }
    if (!name.startsWith("@/")) return require(name)
    if (cache.has(name)) return cache.get(name)
    const base = fileURLToPath(new URL(`../${name.slice(2)}`, import.meta.url))
    const path = [base + ".tsx", base + ".ts"].find(existsSync)
    assert.ok(path, `module exists: ${name}`)
    const { outputText } = ts.transpileModule(readFileSync(path, "utf8"), {
      compilerOptions: { module: ts.ModuleKind.CommonJS, jsx: ts.JsxEmit.ReactJSX, esModuleInterop: true },
    })
    const compiledModule = { exports: {} }
    runInNewContext(outputText, { module: compiledModule, exports: compiledModule.exports, require: load, process }, { filename: path })
    cache.set(name, compiledModule.exports)
    return compiledModule.exports
  }
  return (file, component, props) => renderToStaticMarkup(React.createElement(load(`@/components/home/${file}`)[component], props))
}

for (const variant of ["editorial", "desk"]) {
  test(`${variant} renders every recommended article and photo beyond the old limits`, () => {
    const render = homeRenderer(variant)
    const posts = Array.from({ length: 6 }, (_, index) => ({ id: `post-${index}`, title: `Story ${index}`, summary: "A summary", lastUpdateTime: "2026-10-30T12:00:00Z" }))
    const photos = Array.from({ length: 15 }, (_, index) => ({ id: `photo-${index}`, title: `Frame ${index}`, width: index % 2 ? 1000 : 1500, height: index % 2 ? 1500 : 1000 }))
    const articles = render("featured-sections", "FeaturedPosts", { posts })
    const prints = render("featured-photos", "FeaturedPhotos", { photos })
    assert.equal((articles.match(/<article\b/g) || []).length, posts.length)
    assert.equal((prints.match(/<figure\b/g) || []).length, photos.length)
    for (const post of posts) assert.ok(articles.includes(post.title))
    for (const photo of photos) assert.ok(prints.includes(`alt="${photo.title}"`))
    assert.equal(render("featured-sections", "FeaturedPosts", { posts: [] }), "")
    assert.equal(render("featured-photos", "FeaturedPhotos", { photos: [] }), "")
    assert.equal((render("featured-photos", "FeaturedPhotos", { photos: photos.slice(0, 1) }).match(/<figure\b/g) || []).length, 1)
  })
}

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
