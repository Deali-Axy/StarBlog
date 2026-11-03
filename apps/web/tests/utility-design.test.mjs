import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"

const source = await readFile(new URL("../lib/utility-variant.ts", import.meta.url), "utf8")
const { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext } })
const { resolveUtilityVariant, utilityStyleHref } = await import(`data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`)

test("utility pages default to flat and reject unsupported URL styles", () => {
  for (const value of [undefined, null, "", "flat", "studio", "EDITORIAL", "<script>"]) {
    assert.equal(resolveUtilityVariant(value), "flat")
  }
  assert.equal(resolveUtilityVariant("editorial"), "editorial")
})

test("style switching preserves the page, other query values and anchor, and removes the default override", () => {
  for (const path of ["/links", "/rss", "/search"]) {
    const href = `${path}?keyword=A%26B&preview=1#link-apply`
    const editorial = utilityStyleHref(href, "editorial")
    const parsed = new URL(editorial, "https://blog.invalid")
    assert.equal(parsed.pathname, path)
    assert.equal(parsed.searchParams.get("uv"), "editorial")
    assert.equal(parsed.searchParams.get("keyword"), "A&B")
    assert.equal(parsed.searchParams.get("preview"), "1")
    assert.equal(parsed.hash, "#link-apply")
    assert.equal(utilityStyleHref(editorial, "flat"), href)
    assert.equal(utilityStyleHref(`https://blog.invalid${path}?uv=editorial`, "flat"), path)
  }
})
