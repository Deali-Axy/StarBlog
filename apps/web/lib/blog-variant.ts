/** Blog list design preview. Choose a default and disable PREVIEW after evaluation. */
export type BlogVariant = "library" | "editorial" | "studio"

export const BLOG_VARIANT: BlogVariant = "library"
export const BLOG_PREVIEW = true

export const BLOG_VARIANT_OPTIONS: { id: BlogVariant; labelZh: string; labelEn: string }[] = [
  { id: "library", labelZh: "现代知识库", labelEn: "Library" },
  { id: "editorial", labelZh: "编辑杂志", labelEn: "Editorial" },
  { id: "studio", labelZh: "科技面板", labelEn: "Studio" },
]

export function parseBlogVariant(value?: string | null): BlogVariant | null {
  return value === "library" || value === "editorial" || value === "studio" ? value : null
}

export function resolveBlogVariant(value?: string | null, preview = BLOG_PREVIEW): BlogVariant {
  return (preview && parseBlogVariant(value)) || BLOG_VARIANT
}
