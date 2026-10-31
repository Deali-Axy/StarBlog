import { FEATURED_CATEGORIES_VARIANT, FEATURED_CATEGORIES_VARIANT_OPTIONS, parseFeaturedCategoriesVariant, type FeaturedCategoriesVariant } from "@/lib/featured-categories-variant"

export const HOME_VARIANT_OPTIONS = [
  { id: "editorial", labelZh: "刊物内页", labelEn: "The Publication" },
  { id: "desk", labelZh: "个人书桌", labelEn: "The Writing Desk" },
] as const
export type HomeVariant = (typeof HOME_VARIANT_OPTIONS)[number]["id"]
export const HOME_VARIANT: HomeVariant = "editorial"
export const homePreferencesCookie = "starblog.home-design"
export const HOME_PALETTES = [
  { id: "paper", labelZh: "暖纸", labelEn: "Warm paper" },
  { id: "slate", labelZh: "冷灰", labelEn: "Cool slate" },
  { id: "forest", labelZh: "森林", labelEn: "Forest" },
  { id: "ink", labelZh: "墨色", labelEn: "Ink" },
] as const
export const HOME_FRAMES = [
  { id: "sidebar", labelZh: "左侧栏", labelEn: "Sidebar", defaultNavigation: "numbered" },
  { id: "topbar", labelZh: "顶部框架", labelEn: "Top bar", defaultNavigation: "compact" },
] as const
export const HOME_NAVIGATIONS = [
  { id: "numbered", labelZh: "编号目录", labelEn: "Numbered index", frames: ["sidebar"], defaultFrame: "sidebar" },
  { id: "compact", labelZh: "紧凑导航", labelEn: "Compact navigation", frames: ["topbar"], defaultFrame: "topbar" },
] as const
export const HOME_HEROES = [
  { id: "cover", labelZh: "大字封面", labelEn: "Typographic cover" },
  { id: "signature", labelZh: "极简签名", labelEn: "Minimal signature" },
  { id: "photograph", labelZh: "全幅摄影", labelEn: "Photographic cover" },
] as const
export const HOME_FOOTERS = [
  { id: "colophon", labelZh: "刊物落款", labelEn: "Colophon" },
  { id: "contact", labelZh: "联系名片", labelEn: "Contact card" },
] as const
export const homeSettingOptions = {
  "theme.palette": HOME_PALETTES,
  "theme.typography": [{ id: "literary", labelZh: "衬线标题", labelEn: "Literary serif" }, { id: "modern", labelZh: "全无衬线", labelEn: "Modern sans" }],
  "theme.density": [{ id: "compact", labelZh: "紧凑", labelEn: "Compact" }, { id: "standard", labelZh: "标准", labelEn: "Standard" }, { id: "relaxed", labelZh: "舒展", labelEn: "Relaxed" }],
  "theme.texture": [{ id: "none", labelZh: "无纹理", labelEn: "Plain" }, { id: "paper", labelZh: "纸张", labelEn: "Paper" }, { id: "dots", labelZh: "点阵", labelEn: "Dots" }],
  "theme.border": [{ id: "solid", labelZh: "实线", labelEn: "Solid" }, { id: "dashed", labelZh: "虚线", labelEn: "Dashed" }, { id: "none", labelZh: "无边框", labelEn: "None" }],
  "theme.shadow": [{ id: "none", labelZh: "无阴影", labelEn: "None" }, { id: "soft", labelZh: "柔和", labelEn: "Soft" }, { id: "offset", labelZh: "偏移阴影", labelEn: "Offset" }],
  frame: HOME_FRAMES, navigation: HOME_NAVIGATIONS,
  contentWidth: [{ id: "narrow", labelZh: "窄版", labelEn: "Narrow" }, { id: "standard", labelZh: "标准", labelEn: "Standard" }, { id: "wide", labelZh: "宽版", labelEn: "Wide" }],
  hero: HOME_HEROES, footer: HOME_FOOTERS,
  "sections.featuredPosts": [{ id: "editorial", labelZh: "刊物", labelEn: "Editorial" }, { id: "notes", labelZh: "便笺", labelEn: "Notes" }],
  "sections.latestPosts": [{ id: "timeline", labelZh: "时间索引", labelEn: "Timeline" }, { id: "ledger", labelZh: "手记", labelEn: "Ledger" }],
  "sections.interests": [{ id: "cloud", labelZh: "词云", labelEn: "Word cloud" }, { id: "tags", labelZh: "标签", labelEn: "Tags" }],
  "sections.categories": FEATURED_CATEGORIES_VARIANT_OPTIONS,
  "sections.photos": [{ id: "contactSheet", labelZh: "接触印样", labelEn: "Contact sheet" }, { id: "prints", labelZh: "散落相纸", labelEn: "Loose prints" }],
  "sections.links": [{ id: "directory", labelZh: "目录", labelEn: "Directory" }, { id: "cards", labelZh: "名片", labelEn: "Cards" }],
} as const

type OptionId<K extends keyof typeof homeSettingOptions> = (typeof homeSettingOptions)[K][number]["id"]
export type HomeColorMode = "light" | "dark"
export type HomeMode = HomeColorMode | "system"
export type HomeBaseColors = { primary: string; background: string; foreground: string }
export type HomeThemeSettings = {
  palette: OptionId<"theme.palette">; typography: OptionId<"theme.typography">; density: OptionId<"theme.density">
  fontScale: number; texture: OptionId<"theme.texture">; border: OptionId<"theme.border">; shadow: OptionId<"theme.shadow">
  colors: Record<HomeColorMode, Partial<HomeBaseColors>>
}
export type HomeSectionVariants = {
  featuredPosts: OptionId<"sections.featuredPosts">; latestPosts: OptionId<"sections.latestPosts">; interests: OptionId<"sections.interests">
  categories: FeaturedCategoriesVariant; photos: OptionId<"sections.photos">; links: OptionId<"sections.links">
}
export type HomeResolvedConfig = {
  theme: HomeThemeSettings; frame: OptionId<"frame">; navigation: OptionId<"navigation">; contentWidth: OptionId<"contentWidth">
  hero: OptionId<"hero">; footer: OptionId<"footer">; sections: HomeSectionVariants
}
export type HomeOverrides = Partial<Omit<HomeResolvedConfig, "theme" | "sections">> & {
  theme?: Partial<Omit<HomeThemeSettings, "colors">> & { colors?: Partial<Record<HomeColorMode, Partial<HomeBaseColors>>> }
  sections?: Partial<HomeSectionVariants>
}
export type HomePreferences = { version: 2; preset: HomeVariant; overrides: HomeOverrides }
export type HomeSettingPath = keyof typeof homeSettingOptions | "theme.fontScale" | `theme.colors.${HomeColorMode}.${keyof HomeBaseColors}`
export const HOME_COLOR_PATHS = (["light", "dark"] as const).flatMap(mode => (["primary", "background", "foreground"] as const).map(color => `theme.colors.${mode}.${color}` as const))
const paths: HomeSettingPath[] = [...Object.keys(homeSettingOptions) as (keyof typeof homeSettingOptions)[], "theme.fontScale", ...HOME_COLOR_PATHS]
const baseTheme: HomeThemeSettings = { palette: "paper", typography: "literary", density: "standard", fontScale: 100, texture: "none", border: "solid", shadow: "none", colors: { light: {}, dark: {} } }
export const HOME_PRESETS: Record<HomeVariant, HomeResolvedConfig> = {
  editorial: { theme: baseTheme, frame: "sidebar", navigation: "numbered", contentWidth: "standard", hero: "cover", footer: "colophon", sections: { featuredPosts: "editorial", latestPosts: "timeline", interests: "cloud", categories: FEATURED_CATEGORIES_VARIANT, photos: "contactSheet", links: "directory" } },
  desk: { theme: { ...baseTheme, border: "dashed", shadow: "offset" }, frame: "topbar", navigation: "compact", contentWidth: "standard", hero: "signature", footer: "contact", sections: { featuredPosts: "notes", latestPosts: "ledger", interests: "tags", categories: FEATURED_CATEGORIES_VARIANT, photos: "prints", links: "cards" } },
}
function record(value: unknown): Record<string, unknown> {
  return value !== null && typeof value === "object" && !Array.isArray(value) ? value as Record<string, unknown> : {}
}
export function homeSettingValue(value: unknown, path: HomeSettingPath): unknown {
  return path.split(".").reduce<unknown>((current, key) => Object.prototype.hasOwnProperty.call(record(current), key) ? record(current)[key] : undefined, value)
}
function put(value: object, path: HomeSettingPath, next: unknown) {
  const parts = path.split(".")
  let target = value as Record<string, unknown>
  for (const key of parts.slice(0, -1)) target = target[key] = record(target[key])
  if (next === undefined) delete target[parts.at(-1)!]
  else target[parts.at(-1)!] = next
}
function valid(path: HomeSettingPath, value: unknown) {
  if (path === "theme.fontScale") return typeof value === "number" && value >= 90 && value <= 115 && value % 5 === 0
  if (path.startsWith("theme.colors.")) return typeof value === "string" && /^#[0-9a-f]{6}$/i.test(value)
  return homeSettingOptions[path as keyof typeof homeSettingOptions].some(option => option.id === value)
}
export function parseHomeVariant(value: unknown): HomeVariant | null {
  return HOME_VARIANT_OPTIONS.find(option => option.id === value)?.id ?? null
}
/** Whitelist every leaf; URL data is never spread into the configuration. */
export function normalizeHomePreferences(value: unknown): HomePreferences {
  const input = record(value)
  const preset = parseHomeVariant(input.preset) ?? HOME_VARIANT
  const defaults = HOME_PRESETS[preset]
  const resolved = structuredClone(defaults)
  for (const path of paths) {
    const next = homeSettingValue(input.overrides, path)
    if (valid(path, next)) put(resolved, path, typeof next === "string" && next.startsWith("#") ? next.toLowerCase() : next)
  }
  const navigation = HOME_NAVIGATIONS.find(option => option.id === resolved.navigation)!
  if (!(navigation.frames as readonly string[]).includes(resolved.frame)) {
    if (valid("navigation", homeSettingValue(input.overrides, "navigation"))) resolved.frame = navigation.defaultFrame
    else resolved.navigation = HOME_FRAMES.find(option => option.id === resolved.frame)!.defaultNavigation
  }
  const overrides: HomeOverrides = {}
  for (const path of paths) {
    const next = homeSettingValue(resolved, path)
    if (next !== homeSettingValue(defaults, path)) put(overrides, path, next)
  }
  return { version: 2, preset, overrides }
}
export function resolveHomeConfig(preferences: HomePreferences): HomeResolvedConfig {
  const clean = normalizeHomePreferences(preferences)
  const config = structuredClone(HOME_PRESETS[clean.preset])
  for (const path of paths) {
    const value = homeSettingValue(clean.overrides, path)
    if (value !== undefined) put(config, path, value)
  }
  return config
}
export function setHomeSetting(preferences: HomePreferences, path: HomeSettingPath, value?: unknown): HomePreferences {
  const next = structuredClone(preferences)
  put(next.overrides, path, value)
  if (path === "theme.palette") delete next.overrides.theme?.colors
  const selected = value ?? homeSettingValue(HOME_PRESETS[next.preset], path)
  if (path === "frame") {
    const frame = HOME_FRAMES.find(option => option.id === selected)
    if (frame) next.overrides.navigation = frame.defaultNavigation
  }
  if (path === "navigation") {
    const navigation = HOME_NAVIGATIONS.find(option => option.id === selected)
    if (navigation) next.overrides.frame = navigation.defaultFrame
  }
  return normalizeHomePreferences(next)
}
function parseSaved(value: unknown): HomePreferences | null {
  if (typeof value !== "string" || value.length > 3500) return null
  if (value.startsWith("v2.")) {
    try {
      const decoded: unknown = JSON.parse(atob(value.slice(3).replaceAll("-", "+").replaceAll("_", "/")))
      return record(decoded).version === 2 ? normalizeHomePreferences(decoded) : null
    } catch { return null }
  }
  const [variant, categories] = value.split(".")
  if (!parseHomeVariant(variant) && !parseFeaturedCategoriesVariant(categories)) return null
  return normalizeHomePreferences({ preset: parseHomeVariant(variant) ?? HOME_VARIANT, overrides: { sections: { categories } } })
}
export function resolveHomePreferences(query: { hc?: unknown; hv?: unknown; fc?: unknown }, saved = ""): HomePreferences {
  const shared = typeof query.hc === "string" && query.hc.startsWith("v2.") ? parseSaved(query.hc) : null
  if (shared) return shared
  let result = parseSaved(saved) ?? normalizeHomePreferences({})
  const legacyPreset = parseHomeVariant(query.hv)
  if (legacyPreset) result = normalizeHomePreferences({ preset: legacyPreset, overrides: { sections: { categories: resolveHomeConfig(result).sections.categories } } })
  const categories = typeof query.fc === "string" ? parseFeaturedCategoriesVariant(query.fc) : null
  if (categories) result = setHomeSetting(result, "sections.categories", categories)
  return result
}
export function serializeHomePreferences(preferences: HomePreferences) {
  return `v2.${btoa(JSON.stringify(normalizeHomePreferences(preferences))).replaceAll("+", "-").replaceAll("/", "_").replace(/=+$/, "")}`
}
export function homeDesignHref(href: string, preferences: HomePreferences) {
  const url = new URL(href, "https://home.invalid")
  url.searchParams.delete("hv")
  url.searchParams.delete("fc")
  url.searchParams.set("hc", serializeHomePreferences(preferences))
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
