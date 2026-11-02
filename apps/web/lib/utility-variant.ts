export type UtilityVariant = "flat" | "editorial"

export const UTILITY_VARIANT: UtilityVariant = "flat"

export function resolveUtilityVariant(value?: string | null): UtilityVariant {
  return value === "editorial" ? "editorial" : UTILITY_VARIANT
}

/** Keep unrelated query parameters and anchors; the default needs no override. */
export function utilityStyleHref(href: string, style: UtilityVariant) {
  const url = new URL(href, "https://blog.invalid")
  if (style === UTILITY_VARIANT) url.searchParams.delete("uv")
  else url.searchParams.set("uv", style)
  return `${url.pathname}${url.search}${url.hash}`
}
