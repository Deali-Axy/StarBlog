import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"
import ts from "typescript"

// Compile the pure geometry helpers with the project's existing TypeScript dependency.
async function loadHelper(path) {
  const source = await readFile(new URL(path, import.meta.url), "utf8")
  const { outputText } = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.ESNext } })
  return import(`data:text/javascript;base64,${Buffer.from(outputText).toString("base64")}`)
}
const { layoutMasonry, photoColumns, densityScale } = await loadHelper("../lib/photography/layout.ts")
const { layoutJustified, photoAspect } = await loadHelper("../lib/photography/justify.ts")

function assertGeometry(layout, aspects, width) {
  assert.equal(layout.boxes.length, aspects.length)
  for (const box of layout.boxes) {
    assert.ok(box.width > 0 && box.height > 0)
    assert.ok(box.x >= 0 && box.y >= 0)
    assert.ok(box.x + box.width <= width + 0.001)
    assert.ok(box.y + box.height <= layout.height + 0.001)
    assert.ok(Math.abs(box.width / box.height - aspects[box.index]) < 0.001, "photo composition is preserved")
  }
  for (let i = 0; i < layout.boxes.length; i++) {
    for (let j = i + 1; j < layout.boxes.length; j++) {
      const a = layout.boxes[i], b = layout.boxes[j]
      assert.ok(a.x + a.width <= b.x + 0.001 || b.x + b.width <= a.x + 0.001 || a.y + a.height <= b.y + 0.001 || b.y + b.height <= a.y + 0.001, "photos must not overlap")
    }
  }
}

test("responsive densities match the agreed column counts", () => {
  for (const [width, expected] of [[375, [1, 2, 2]], [768, [2, 3, 4]], [1280, [3, 4, 5]], [1920, [4, 5, 6]]]) {
    assert.deepEqual(["comfortable", "standard", "compact"].map((density) => photoColumns(width, density)), expected)
  }
  assert.ok(densityScale("comfortable") > densityScale("standard"))
  assert.ok(densityScale("compact") < densityScale("standard"))
})

test("both layouts preserve portraits, landscapes and extreme aspect ratios without overlaps", () => {
  const aspects = [1.5, 0.67, 1, 8, 0.15, 2, 0.8, 1.3, 1.5]
  for (const width of [1, 343, 704, 1216, 1856]) {
    for (const density of ["comfortable", "standard", "compact"]) {
      assertGeometry(layoutMasonry(aspects, width, photoColumns(width, density), 12), aspects, width)
      assertGeometry(layoutJustified(aspects, width, 240 * densityScale(density), Math.min(12, width / 2)), aspects, width)
    }
  }
})

test("masonry appends new pages without repositioning existing photos", () => {
  const initial = [1.5, 0.6, 1.2, 2, 0.8]
  const before = layoutMasonry(initial, 1216, 4, 24)
  const after = layoutMasonry([...initial, 1, 1.8, 0.5], 1216, 4, 24)
  assert.deepEqual(after.boxes.slice(0, initial.length), before.boxes)
})

test("empty collections, a single photo and unavailable dimensions remain valid", () => {
  assert.equal(layoutMasonry([], 375, 2, 12).height, 0)
  assert.equal(layoutJustified([], 375, 240, 12).height, 0)
  assert.deepEqual([photoAspect(0, 0), photoAspect(-1, 2), photoAspect(Infinity, 1)], [1.5, 1.5, 1.5])
  assert.equal(photoAspect(8000, 1000), 8)
  assertGeometry(layoutJustified([8], 343, 240, 12), [8], 343)
  assertGeometry(layoutMasonry([0.15], 343, 1, 12), [0.15], 343)
})
