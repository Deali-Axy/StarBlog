import { BLOG_PREVIEW, resolveBlogVariant, type BlogVariant } from "@/lib/blog-variant"
import { routes } from "@/lib/routes"

export interface BlogQuery {
  page: number
  pageSize: number
  search?: string
  categoryId?: number
  sortBy: "lastUpdateTime" | "creationTime" | "title"
  sortType: "asc" | "desc"
  view: "grid" | "list"
  bv?: BlogVariant
}

export type BlogSearchParams = Partial<Record<keyof BlogQuery, string | string[]>>

function positiveInteger(value: string | undefined, fallback: number) {
  const number = Number(value)
  // API query parameters use Int32.
  return Number.isSafeInteger(number) && number > 0 && number <= 2_147_483_647 ? number : fallback
}

export function parseBlogQuery(params: BlogSearchParams): BlogQuery {
  const single = (key: keyof BlogQuery) => typeof params[key] === "string" ? params[key] as string : undefined
  const categoryId = positiveInteger(single("categoryId"), 0) || undefined
  const sortBy = single("sortBy")
  return {
    page: positiveInteger(single("page"), 1),
    pageSize: Math.min(100, positiveInteger(single("pageSize"), 12)),
    search: single("search")?.trim() || undefined,
    categoryId,
    sortBy: sortBy === "title" || sortBy === "creationTime" ? sortBy : "lastUpdateTime",
    sortType: single("sortType") === "asc" ? "asc" : "desc",
    view: single("view") === "list" ? "list" : "grid",
    bv: BLOG_PREVIEW ? resolveBlogVariant(single("bv")) : undefined,
  }
}

export function updateBlogQuery(query: BlogQuery, patch: Partial<BlogQuery>): BlogQuery {
  const resetsPage = ["categoryId", "search", "sortBy", "sortType", "pageSize"].some((key) => key in patch)
  return { ...query, ...(resetsPage ? { page: 1 } : {}), ...patch }
}

export function blogQueryHref(query: BlogQuery, patch: Partial<BlogQuery> = {}) {
  return routes.blogList(updateBlogQuery(query, patch))
}
