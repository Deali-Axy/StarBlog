/**
 * Homepage「精选分类」布局开关。
 *
 * - magazine: 杂志封面墙（不对称 bento）
 * - star: 星图导航（散点连线，移动端降级为列表）
 * - deck: 堆叠翻阅（motion 拖拽/按钮翻卡）
 *
 * 将 FEATURED_CATEGORIES_PREVIEW 设为 true 时，首页区块标题旁会出现切换器，方便对比。
 * 选定方案后：改 VARIANT，并把 PREVIEW 设回 false。
 */
export type FeaturedCategoriesVariant = "magazine" | "star" | "deck"

export const FEATURED_CATEGORIES_VARIANT: FeaturedCategoriesVariant = "deck"

/** 为 true 时在首页显示方案切换器（仅用于对比，上线前关掉）。 */
export const FEATURED_CATEGORIES_PREVIEW = true

export const FEATURED_CATEGORIES_VARIANT_OPTIONS: {
  id: FeaturedCategoriesVariant
  labelZh: string
  labelEn: string
}[] = [
  { id: "deck", labelZh: "堆叠翻阅", labelEn: "Card deck" },
  { id: "magazine", labelZh: "杂志封面", labelEn: "Magazine" },
  { id: "star", labelZh: "星图导航", labelEn: "Star chart" },
]

export function parseFeaturedCategoriesVariant(value?: string | null): FeaturedCategoriesVariant | null {
  if (value === "magazine" || value === "star" || value === "deck") return value
  return null
}
