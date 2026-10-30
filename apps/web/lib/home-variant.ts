import { FEATURED_CATEGORIES_VARIANT, parseFeaturedCategoriesVariant, type FeaturedCategoriesVariant } from "@/lib/featured-categories-variant"

/** Add a named design here and its scoped styles in home-sections.module.css. */
export const HOME_VARIANT_OPTIONS = [
  { id: "editorial", labelZh: "刊物内页", labelEn: "The Publication" },
  { id: "desk", labelZh: "个人书桌", labelEn: "The Writing Desk" },
] as const

export type HomeVariant = (typeof HOME_VARIANT_OPTIONS)[number]["id"]
export const HOME_VARIANT: HomeVariant = "editorial"
export const homePreferencesCookie = "starblog.home-design"

export type HomePreferences = { variant: HomeVariant; categories: FeaturedCategoriesVariant }

export function parseHomeVariant(value: unknown): HomeVariant | null {
  return HOME_VARIANT_OPTIONS.find(option => option.id === value)?.id ?? null
}

export function resolveHomePreferences(query: { hv?: unknown; fc?: unknown }, saved = ""): HomePreferences {
  const [variant, categories] = saved.split(".")
  return {
    variant: parseHomeVariant(query.hv) ?? parseHomeVariant(variant) ?? HOME_VARIANT,
    categories: parseFeaturedCategoriesVariant(typeof query.fc === "string" ? query.fc : null)
      ?? parseFeaturedCategoriesVariant(categories) ?? FEATURED_CATEGORIES_VARIANT,
  }
}

export function serializeHomePreferences(preferences: HomePreferences) {
  return `${preferences.variant}.${preferences.categories}`
}

/** Both choices travel together; unrelated query parameters and anchors survive. */
export function homeDesignHref(href: string, preferences: HomePreferences) {
  const url = new URL(href, "https://home.invalid")
  url.searchParams.set("hv", preferences.variant)
  url.searchParams.set("fc", preferences.categories)
  return `${url.pathname}${url.search}${url.hash}`
}

export function groupHomePostsByYear<T extends { lastUpdateTime: string }>(posts: T[]) {
  const groups = new Map<string, T[]>()
  for (const post of posts) {
    const year = /^\d{4}-/.test(post.lastUpdateTime) ? post.lastUpdateTime.slice(0, 4) : ""
    const group = groups.get(year) ?? []
    group.push(post)
    groups.set(year, group)
  }
  return Array.from(groups, ([year, items]) => ({ year, items }))
}
