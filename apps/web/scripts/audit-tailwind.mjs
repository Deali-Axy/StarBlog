// Read-only inventory. Run from any directory; redirect stdout to keep the JSON.
// Uses the installed Tailwind parser (an unstable API), not a migration codemod.
import { readFile, readdir } from "node:fs/promises"
import { createRequire } from "node:module"
import path from "node:path"
import { fileURLToPath } from "node:url"
import ts from "typescript"
import { __unstable__loadDesignSystem } from "tailwindcss"

const root = fileURLToPath(new URL("../", import.meta.url))
const require = createRequire(new URL("../package.json", import.meta.url))
const excluded = new Set(["node_modules", ".next", ".git", "out", "build", "coverage"])
async function walk(directory) {
  const result = []
  for (const entry of (await readdir(directory, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
    if (excluded.has(entry.name)) continue
    const absolute = path.join(directory, entry.name)
    if (entry.isDirectory()) result.push(...await walk(absolute))
    else result.push(path.relative(root, absolute))
  }
  return result
}
const files = await walk(root)
const sourceFiles = files.filter(file => /^(app|components|features|lib|hooks)\//.test(file) && /\.[cm]?[jt]sx?$/.test(file))
const cssFiles = files.filter(file => /\.(css|scss|sass|less)$/.test(file))
const designSystem = await __unstable__loadDesignSystem(await readFile(path.join(root, "app/globals.css"), "utf8"), {
  base: root,
  async loadStylesheet(id, base) {
    // These two packages expose separate JS/style entry points.
    const absolute = id === "tailwindcss" ? require.resolve("tailwindcss/index.css")
      : id === "tw-animate-css" ? path.join(root, "node_modules/tw-animate-css/dist/tw-animate.css")
        : id.startsWith(".") ? path.resolve(base, id) : require.resolve(id)
    return { path: absolute, base: path.dirname(absolute), content: await readFile(absolute, "utf8") }
  },
})
const occurrences = []
const classAttributes = []
const inlineStyles = []
const dynamicClassTemplates = []
const uncompiledClassTokens = []
const css = []
const cache = new Map()
function inspect(token) {
  if (cache.has(token)) return cache.get(token)
  const candidate = designSystem.parseCandidate(token)[0]
  if (!candidate || !designSystem.candidatesToCss([token])[0]) {
    cache.set(token, null)
    return null
  }
  const variants = []
  function visitVariant(variant) {
    variants.push(variant)
    if (variant.variant) visitVariant(variant.variant)
  }
  candidate.variants.forEach(visitVariant)
  const kind = candidate.kind === "arbitrary" ? candidate.property.startsWith("--") ? "custom-property" : "arbitrary-property"
    : candidate.kind === "functional" && (candidate.value?.kind === "arbitrary" || candidate.modifier?.kind === "arbitrary") ? "arbitrary-value"
      : "standard-utility"
  const result = {
    kind,
    property: candidate.property ?? null,
    root: candidate.root ?? null,
    value: typeof candidate.value === "string" ? candidate.value : candidate.value?.value ?? null,
    important: candidate.important,
    breakpoints: variants.filter(v => ["min", "max"].includes(v.root) && v.value?.kind === "arbitrary").map(v => `${v.root}-[${v.value.value}]`),
    selectors: variants.filter(v => v.kind === "arbitrary").map(v => v.selector),
    canonical: designSystem.canonicalizeCandidates([token])[0],
  }
  cache.set(token, result)
  return result
}
for (const file of sourceFiles) {
  const source = await readFile(path.join(root, file), "utf8")
  const ast = ts.createSourceFile(file, source, ts.ScriptTarget.Latest, true, file.endsWith("x") ? ts.ScriptKind.TSX : ts.ScriptKind.TS)
  function location(offset) {
    const pos = ast.getLineAndCharacterOfPosition(offset)
    return { file, line: pos.line + 1, column: pos.character + 1 }
  }
  function visit(node) {
    function inClassContext() {
      for (let parent = node.parent; parent; parent = parent.parent) {
        if (ts.isJsxAttribute(parent)) return parent.name.getText(ast) === "className"
        if (ts.isCallExpression(parent) && /^(cn|cva|clsx|twMerge)$/.test(parent.expression.getText(ast))) return true
        if (ts.isVariableDeclaration(parent) && /class/i.test(parent.name.getText(ast))) return true
        if (ts.isBlock(parent) || ts.isSourceFile(parent)) break
      }
      return false
    }
    if (ts.isJsxAttribute(node) && node.name.getText(ast) === "className") {
      const expression = node.initializer?.getText(ast) ?? ""
      classAttributes.push({ ...location(node.getStart(ast)), characters: expression.length, expression })
    }
    if (ts.isJsxAttribute(node) && node.name.getText(ast) === "style") {
      inlineStyles.push({ ...location(node.getStart(ast)), expression: node.initializer?.getText(ast) ?? "" })
    }
    if (ts.isTemplateExpression(node) && inClassContext()) {
      dynamicClassTemplates.push({ ...location(node.getStart(ast)), expression: node.getText(ast) })
    }
    if (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node) || ts.isTemplateHead(node) || ts.isTemplateMiddle(node) || ts.isTemplateTail(node)) {
      // Original literal source preserves exact locations. Partial template tokens
      // fail the parser and are excluded; runtime-generated classes need review.
      const raw = node.getText(ast).slice(1, ts.isTemplateHead(node) || ts.isTemplateMiddle(node) ? -2 : -1)
      for (const match of raw.matchAll(/\S+/g)) {
        const token = match[0]
        if (!/[\[\]!]/.test(token)) continue
        const details = inspect(token)
        if (details) occurrences.push({ ...location(node.getStart(ast) + 1 + match.index), token, ...details })
        else if (inClassContext()) uncompiledClassTokens.push({ ...location(node.getStart(ast) + 1 + match.index), token })
      }
    }
    ts.forEachChild(node, visit)
  }
  visit(ast)
}
for (const file of cssFiles) {
  const source = await readFile(path.join(root, file), "utf8")
  css.push({ file, lines: source.split("\n").length, customPropertyDeclarations: [...source.matchAll(/--[\w-]+\s*:/g)].length })
}
function countBy(items, key) {
  const counts = new Map()
  for (const item of items) {
    const values = key(item)
    for (const value of Array.isArray(values) ? values : [values]) counts.set(value, (counts.get(value) ?? 0) + 1)
  }
  return Object.fromEntries([...counts.entries()].sort((a, b) => b[1] - a[1] || String(a[0]).localeCompare(String(b[0]))))
}
const bracketOccurrences = occurrences.filter(row => row.token.includes("["))
const longAttributes = classAttributes.filter(row => row.characters > 500).sort((a, b) => b.characters - a.characters)
const packageJson = JSON.parse(await readFile(require.resolve("tailwindcss/package.json"), "utf8"))
const result = {
  schemaVersion: 1,
  tailwindVersion: packageJson.version,
  method: "Static literal tokens parsed and compiled against installed Tailwind and project CSS imports. Counts are source occurrences, not runtime DOM counts or a violation count. Comments/tests/generated/dependency files are excluded from class metrics. Non-style string literals can match; no claim of whole-program data-flow analysis. Canonical suggestions need semantic/cascade review.",
  scope: { totalFiles: files.length, sourceFiles: sourceFiles.length, sourcePaths: sourceFiles, excludedDirectories: [...excluded], css },
  summary: {
    bracketOccurrences: bracketOccurrences.length,
    uniqueBracketTokens: new Set(bracketOccurrences.map(row => row.token)).size,
    filesWithBrackets: new Set(bracketOccurrences.map(row => row.file)).size,
    byKind: countBy(bracketOccurrences, row => row.kind),
    arbitraryProperties: countBy(bracketOccurrences.filter(row => row.kind === "arbitrary-property"), row => row.property),
    arbitraryValueUtilities: countBy(bracketOccurrences.filter(row => row.kind === "arbitrary-value"), row => row.root),
    breakpoints: countBy(bracketOccurrences, row => row.breakpoints),
    selectors: countBy(bracketOccurrences, row => row.selectors),
    byFile: countBy(bracketOccurrences, row => row.file),
    byFeature: countBy(bracketOccurrences, row => row.file.startsWith("components/") ? row.file.split("/").slice(0, 2).join("/") : row.file.split("/")[0]),
    importantOccurrences: occurrences.filter(row => row.important).length,
    specificityBoostOccurrences: bracketOccurrences.filter(row => row.selectors.some(selector => /&&/.test(selector))).length,
    classAttributes: classAttributes.length,
    classAttributesOver500Characters: longAttributes.length,
    inlineStyleAttributes: inlineStyles.length,
    dynamicClassTemplates: dynamicClassTemplates.length,
    uncompiledClassTokens: uncompiledClassTokens.length,
  },
  occurrences,
  longClassAttributes: longAttributes,
  inlineStyles,
  dynamicClassTemplates,
  uncompiledClassTokens,
}
process.stdout.write(`${JSON.stringify(result, null, 2)}\n`)
