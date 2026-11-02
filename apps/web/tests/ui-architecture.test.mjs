import assert from "node:assert/strict"
import { readdir, readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"
const root = new URL("../", import.meta.url)
async function files(directory) {
  const entries = await readdir(new URL(directory, root), { withFileTypes: true })
  return (await Promise.all(entries.map(entry => entry.isDirectory() ? files(`${directory}/${entry.name}`) : `${directory}/${entry.name}`))).flat()
}
const components = (await files("components")).filter(file => /\.tsx?$/.test(file))
test("business components compose local primitives and presentations cannot read layout selectors", async () => {
  for (const file of components) {
    const source = await readFile(new URL(file, root), "utf8")
    const ast = ts.createSourceFile(file, source, ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX)
    function visit(node) {
      if (ts.isImportDeclaration(node)) {
        if (!file.startsWith("components/ui/")) assert.notEqual(node.moduleSpecifier.text, "radix-ui", file)
        if (file.includes("/views/")) {
          assert.ok(!node.moduleSpecifier.text.endsWith("home-design-context"), file)
          assert.ok(!/\buseBlogLayout\b/.test(node.importClause?.getText(ast) ?? ""), file)
          assert.ok(!/\buseUtilityDesign\b/.test(node.importClause?.getText(ast) ?? ""), file)
        }
      }
      if (file.includes("/views/") && ts.isJsxAttribute(node) && ["className", "style"].includes(node.name.text)) {
        const expression = node.initializer?.getText(ast) ?? ""
        assert.ok(!/\b(?:config|designClasses|variant)\b|\bquery\.view\b/.test(expression), `${file}: ${expression}`)
        assert.ok(!/data-(?:blog-chrome|blog-variant|directory-layout|home-variant|home-layout|article-layout|view)[=\]]/.test(expression), `${file}: style selectors must not change layout`)
      }
      ts.forEachChild(node, visit)
    }
    visit(ast)
  }
})
test("remaining CSS owns tokens, native pseudo-elements and effects, not switchable layouts", async () => {
  const cssFiles = (await files("components")).filter(file => file.endsWith(".css"))
  for (const file of cssFiles) {
    assert.ok(["components/home/home-design.module.css", "components/blog/article-native.module.css"].includes(file), file)
    const css = await readFile(new URL(file, root), "utf8")
    assert.doesNotMatch(css, /data-(?:blog-chrome|directory-layout|home-variant|home-layout|article-layout)/)
    assert.doesNotMatch(css, /(?:grid-template|flex-direction|padding|margin|columns)\s*:/)
  }
})
