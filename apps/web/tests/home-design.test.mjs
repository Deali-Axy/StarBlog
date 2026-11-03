import { dirname, relative, resolve } from "node:path"
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

const helpers = await import(await moduleUrl("home-variant"))
const { HOME_VARIANT, homeSettingOptions, normalizeHomePreferences, resolveHomeConfig, setHomeSetting, parseHomeVariant, resolveHomePreferences, serializeHomePreferences, homeDesignHref, groupHomePostsByYear } = helpers
const { homeColors, homeThemeStyle } = await import(await moduleUrl("home-theme"))
const { FEATURED_CATEGORIES_VARIANT } = await import(await moduleUrl("featured-categories-variant"))

// Render real sections, replacing only the browser providers and Next's link.
const require = createRequire(import.meta.url)
const React = require("react")
const { renderToStaticMarkup } = require("react-dom/server")

function homeRenderer(variant, overrides = {}) {
  const preferences = normalizeHomePreferences({ preset: variant, overrides })
  const config = resolveHomeConfig(preferences)
  const cache = new Map()
  const translations = new Proxy({}, { get: (_, key) => String(key) })
  function load(name, importer) {
    if (name.startsWith(".")) name = "@/" + relative(fileURLToPath(new URL("../", import.meta.url)), resolve(dirname(importer), name)).replaceAll("\\", "/")
    if (name === "@/components/app-providers") return { useI18n: () => ({ t: translations, locale: "zh" }) }
    if (name === "@/components/home/home-design-context") return { useHomeDesign: () => ({ config, preferences }) }
    if (name === "next/link") return { __esModule: true, default: ({ children, ...props }) => React.createElement("a", props, children) }
    if (name === "@/lib/home-variant") return helpers
    if (name.endsWith(".module.css")) return { __esModule: true, default: new Proxy({}, { get: (_, key) => String(key) }) }
    if (!name.startsWith("@/")) return require(name)
    if (cache.has(name)) return cache.get(name)
    const base = fileURLToPath(new URL(`../${name.slice(2)}`, import.meta.url))
    const path = [base + ".tsx", base + ".ts"].find(existsSync)
    assert.ok(path, `module exists: ${name}`)
    const { outputText } = ts.transpileModule(readFileSync(path, "utf8"), {
      compilerOptions: { module: ts.ModuleKind.CommonJS, jsx: ts.JsxEmit.ReactJSX, esModuleInterop: true },
    })
    const compiledModule = { exports: {} }
    runInNewContext(outputText, { module: compiledModule, exports: compiledModule.exports, require: name => load(name, path), process }, { filename: path })
    cache.set(name, compiledModule.exports)
    return compiledModule.exports
  }
  return (file, component, props) => renderToStaticMarkup(React.createElement(load(`@/components/home/${file}`)[component], props))
}

for (const variant of ["journal", "editorial", "desk"]) {
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

test("the default journal matches the chosen shared design and preserves saved designs", () => {
  const shared = "v2.eyJ2ZXJzaW9uIjoyLCJwcmVzZXQiOiJlZGl0b3JpYWwiLCJvdmVycmlkZXMiOnsidGhlbWUiOnsidGV4dHVyZSI6ImRvdHMiLCJib3JkZXIiOiJkYXNoZWQifSwic2VjdGlvbnMiOnsiZmVhdHVyZWRQb3N0cyI6Im5vdGVzIiwicGhvdG9zIjoicHJpbnRzIiwibGlua3MiOiJjYXJkcyJ9fX0"
  const defaults = resolveHomePreferences({})
  assert.equal(defaults.preset, "journal")
  assert.deepEqual(defaults.overrides, {})
  assert.deepEqual(resolveHomeConfig(defaults), resolveHomeConfig(resolveHomePreferences({ hc: shared })))
  assert.equal(resolveHomeConfig(resolveHomePreferences({}, "editorial.tilt")).sections.photos, "contactSheet")
  assert.equal(resolveHomePreferences({}, serializeHomePreferences(defaults)).preset, "journal")
})

test("legacy cookies and links migrate to complete presets with independent categories", () => {
  const migrated = resolveHomePreferences({}, "desk.tilt")
  assert.equal(migrated.version, 2)
  assert.equal(migrated.preset, "desk")
  assert.equal(resolveHomeConfig(migrated).navigation, "compact")
  assert.equal(resolveHomeConfig(migrated).sections.categories, "tilt")
  assert.equal(resolveHomePreferences({ hv: "editorial" }, "desk.tilt").preset, "editorial")
  assert.equal(resolveHomeConfig(resolveHomePreferences({ fc: "rail" }, "desk.tilt")).sections.categories, "rail")
  assert.equal(resolveHomePreferences({ hv: ["desk"], fc: ["rail"] }, "editorial.tilt").preset, "editorial")
  assert.equal(resolveHomeConfig(resolveHomePreferences({ hv: "toString", fc: "__proto__" }, "broken.corrupt")).sections.categories, FEATURED_CATEGORIES_VARIANT)
  assert.equal(parseHomeVariant(null), null)
  assert.equal(parseHomeVariant("future-style"), null)
})
test("every registered choice round-trips and independent overrides reset sparsely", () => {
  for (const [path, options] of Object.entries(homeSettingOptions)) for (const option of options) {
    const value = setHomeSetting(normalizeHomePreferences({}), path, option.id)
    assert.deepEqual(resolveHomePreferences({}, serializeHomePreferences(value)), value)
    assert.equal(helpers.homeSettingValue(resolveHomeConfig(value), path), option.id)
  }
  let value = setHomeSetting(normalizeHomePreferences({}), "sections.photos", "prints")
  value = setHomeSetting(value, "sections.categories", "rail")
  assert.equal(resolveHomeConfig(value).sections.featuredPosts, "notes")
  assert.equal(resolveHomeConfig(value).sections.photos, "prints")
  assert.deepEqual(setHomeSetting(setHomeSetting(value, "sections.photos"), "sections.categories"), normalizeHomePreferences({}))
})
test("frame and navigation selection, external conflicts and resets stay compatible", () => {
  let value = setHomeSetting(normalizeHomePreferences({}), "navigation", "compact")
  assert.equal(resolveHomeConfig(value).frame, "topbar")
  value = setHomeSetting(value, "frame", "sidebar")
  assert.equal(resolveHomeConfig(value).navigation, "numbered")
  assert.deepEqual(value.overrides, {})
  assert.equal(resolveHomeConfig(normalizeHomePreferences({ overrides: { frame: "topbar" } })).navigation, "compact")
  assert.equal(resolveHomeConfig(normalizeHomePreferences({ overrides: { frame: "topbar", navigation: "numbered" } })).frame, "sidebar")
})
test("shared snapshots ignore receiver cookies and legacy parameters; damaged versions fall back", () => {
  const value = setHomeSetting(normalizeHomePreferences({}), "theme.palette", "forest")
  assert.deepEqual(resolveHomePreferences({ hc: serializeHomePreferences(value), hv: "desk", fc: "rail" }, "desk.tilt"), value)
  assert.deepEqual(resolveHomePreferences({ hc: "v2.broken", fc: "rail" }, "desk.tilt"), setHomeSetting(resolveHomePreferences({}, "desk.tilt"), "sections.categories", "rail"))
  assert.equal(resolveHomePreferences({ hc: "v2." + btoa(JSON.stringify({ version: 3, preset: "desk" })) }).preset, HOME_VARIANT)
})
test("unknown fields, prototype keys, invalid colors, scales and oversized input are rejected", () => {
  const value = normalizeHomePreferences(JSON.parse('{"preset":"__proto__","overrides":{"theme":{"palette":"constructor","mode":"light","fontScale":900,"colors":{"light":{"primary":"url(bad)","background":"#12ABef"}}},"hero":"unknown","__proto__":{"polluted":true}}}'))
  assert.equal(value.preset, HOME_VARIANT)
  assert.deepEqual(value.overrides, { theme: { colors: { light: { background: "#12abef" } } } })
  assert.equal({}.polluted, undefined)
  assert.equal(resolveHomePreferences({ hc: "v2." + "a".repeat(4000) }, "desk.tilt").preset, "desk")
})
test("custom colors are mode-specific; changing palette clears both mode overrides", () => {
  let value = setHomeSetting(normalizeHomePreferences({}), "theme.colors.light.primary", "#123456")
  value = setHomeSetting(value, "theme.colors.dark.primary", "#abcdef")
  const theme = resolveHomeConfig(value).theme
  assert.equal(homeColors(theme, "light").primary, "#123456")
  assert.equal(homeColors(theme, "dark").primary, "#abcdef")
  assert.notEqual(homeColors(theme, "light").primaryForeground, homeColors(theme, "dark").primaryForeground)
  assert.deepEqual(setHomeSetting(value, "theme.palette", "forest").overrides, { theme: { palette: "forest" } })
  for (const palette of helpers.HOME_PALETTES) {
    const config = resolveHomeConfig(setHomeSetting(normalizeHomePreferences({}), "theme.palette", palette.id))
    const style = homeThemeStyle(config)
    assert.match(style["--home-background-light"], /^#[a-f0-9]{6}$/)
    assert.notEqual(style["--home-background-light"], style["--home-background-dark"])
    assert.ok(!Object.hasOwn(config.theme, "mode"))
  }
})
test("primary action text keeps at least 4.5:1 contrast across custom grayscale colors", () => {
  const luminance = hex => [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255).map(v => v <= 0.04045 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4).reduce((sum, v, i) => sum + v * [0.2126, 0.7152, 0.0722][i], 0)
  for (let channel = 0; channel < 256; channel++) {
    const primary = "#" + channel.toString(16).padStart(2, "0").repeat(3)
    const theme = resolveHomeConfig(setHomeSetting(normalizeHomePreferences({}), "theme.colors.light.primary", primary)).theme
    const colors = homeColors(theme, "light")
    const a = luminance(colors.primary), b = luminance(colors.primaryForeground)
    assert.ok((Math.max(a, b) + 0.05) / (Math.min(a, b) + 0.05) >= 4.5, primary)
  }
})

test("history links preserve queries and anchors while replacing duplicate legacy fields", () => {
  const value = setHomeSetting(normalizeHomePreferences({ preset: "desk" }), "sections.categories", "rail")
  const url = new URL(homeDesignHref("/?keep=C%23&fc=deck&fc=tilt&hv=desk#home-latest-title", value), "https://test.invalid")
  assert.equal(url.searchParams.get("keep"), "C#")
  assert.equal(url.hash, "#home-latest-title")
  assert.equal(url.searchParams.has("hv"), false)
  assert.equal(url.searchParams.has("fc"), false)
  assert.equal(url.searchParams.getAll("hc").length, 1)
  assert.deepEqual(resolveHomePreferences(Object.fromEntries(url.searchParams), "editorial.tilt"), value)
})
test("all heroes render one heading and photography without an image falls back", () => {
  const props = { locale: "zh", site: { authorName: "A long author name (Alias)", authorTitle: "Developer", authorBio: "Biography", skills: [] }, overview: null, photo: null }
  for (const hero of ["cover", "signature", "photograph"]) {
    const html = homeRenderer("editorial", { hero })("home-hero", "HomeHero", props)
    assert.equal((html.match(/<h1\b/g) || []).length, 1)
    assert.ok(html.includes("A long author name"))
    if (hero === "photograph") assert.ok(html.includes('role="status"'))
  }
  const html = homeRenderer("editorial", { hero: "photograph" })("home-hero", "HomeHero", { ...props, photo: { id: "photo-1", title: "A photograph", width: 1600, height: 900 } })
  assert.ok(html.includes('data-home-hero="photograph"'))
  assert.ok(html.includes('alt="A photograph"'))
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
