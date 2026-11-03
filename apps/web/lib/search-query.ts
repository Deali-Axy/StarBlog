import { routes } from "@/lib/routes"
import { utilityStyleHref, type UtilityVariant } from "@/lib/utility-variant"

export const SEARCH_PAGE_SIZE = 12

export function parseSearchQuery(params: { keyword?: string | string[]; page?: string | string[] }) {
  const keyword = typeof params.keyword === "string" ? params.keyword.trim() : ""
  const requestedPage = typeof params.page === "string" ? Number(params.page) : 1
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 && requestedPage <= 1_000_000 ? requestedPage : 1
  return { keyword, page }
}

export function searchPageHref(keyword: string, page = 1, style: UtilityVariant = "flat") {
  const href = `${routes.searchResults(keyword)}${page > 1 ? `&page=${page}` : ""}`
  return utilityStyleHref(href, style)
}
