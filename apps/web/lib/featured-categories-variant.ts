/**
 * Homepage「精选分类」布局开关。
 *
 * - deck: 堆叠翻阅
 * - rail: 横滑磁吸
 * - catalog: 悬停目录预览
 * - accordion: 手风琴展开
 * - tilt: 3D 倾斜封面墙
 *
 * FEATURED_CATEGORIES_PREVIEW=true 时显示切换器；选定后改 VARIANT 并关掉 PREVIEW。
 */
export type FeaturedCategoriesVariant = "deck" | "rail" | "catalog" | "accordion" | "tilt"

export const FEATURED_CATEGORIES_VARIANT: FeaturedCategoriesVariant = "deck"

export const FEATURED_CATEGORIES_PREVIEW = true

export const FEATURED_CATEGORIES_VARIANT_OPTIONS: {
  id: FeaturedCategoriesVariant
  labelZh: string
  labelEn: string
}[] = [
  { id: "deck", labelZh: "堆叠翻阅", labelEn: "Card deck" },
  { id: "rail", labelZh: "横滑磁吸", labelEn: "Snap rail" },
  { id: "catalog", labelZh: "悬停目录", labelEn: "Hover catalog" },
  { id: "accordion", labelZh: "手风琴", labelEn: "Accordion" },
  { id: "tilt", labelZh: "立体封面", labelEn: "3D tilt" },
]

export function parseFeaturedCategoriesVariant(value?: string | null): FeaturedCategoriesVariant | null {
  if (
    value === "deck" ||
    value === "rail" ||
    value === "catalog" ||
    value === "accordion" ||
    value === "tilt"
  ) {
    return value
  }
  return null
}
