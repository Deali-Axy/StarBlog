import { defineConfig, globalIgnores } from "eslint/config"
import nextVitals from "eslint-config-next/core-web-vitals"
import nextTs from "eslint-config-next/typescript"

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  {
    files: ["components/**/*.tsx"],
    ignores: ["components/ui/**"],
    rules: { "no-restricted-imports": ["error", { paths: [{ name: "radix-ui", message: "Compose source-owned primitives from @/components/ui." }] }] },
  },
  {
    files: ["components/{home,blog,photography}/views/**/*.tsx"],
    rules: { "no-restricted-imports": ["error", {
      paths: [{ name: "radix-ui", message: "Import local UI primitives." }],
      patterns: [
        { group: ["**/home-design-context"], message: "Select presentations in slots; pass state or consume a behavior-only hook." },
        { group: ["**/blog-layout-context"], importNames: ["useBlogLayout"], message: "A presentation must not select its own style." },
      ],
    }] },
  },
  globalIgnores([".next/**", "out/**", "build/**", "next-env.d.ts"]),
])

export default eslintConfig
