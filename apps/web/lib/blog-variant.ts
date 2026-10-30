/** Blog list design preview. Choose a default and disable PREVIEW after evaluation. */
export type BlogVariant = "library" | "editorial" | "studio"

export const BLOG_VARIANT: BlogVariant = "library"
export const BLOG_PREVIEW = true

export const BLOG_VARIANT_OPTIONS: { id: BlogVariant; labelZh: string; labelEn: string }[] = [
  { id: "library", labelZh: "活字索引", labelEn: "The Index" },
  { id: "editorial", labelZh: "折页手记", labelEn: "Field Notes" },
  { id: "studio", labelZh: "档案工作台", labelEn: "The Archive" },
]

export function parseBlogVariant(value?: string | null): BlogVariant | null {
  return value === "library" || value === "editorial" || value === "studio" ? value : null
}

export function resolveBlogVariant(value?: string | null, preview = BLOG_PREVIEW): BlogVariant {
  return (preview && parseBlogVariant(value)) || BLOG_VARIANT
}
