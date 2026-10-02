export const routes = {
  home: "/",
  blog: "/blog",
  search: "/search",
  feed: "/feed",
  post(idOrSlug: string) {
    return `/blog/${encodeURIComponent(idOrSlug)}`
  },
  blogList(params: {
    page?: number
    pageSize?: number
    search?: string
    categoryId?: number
    sortBy?: string
    sortType?: "asc" | "desc"
    view?: "grid" | "list"
  } = {}) {
    const query = new URLSearchParams()
    if (params.page && params.page > 1) query.set("page", String(params.page))
    if (params.pageSize && params.pageSize !== 12) query.set("pageSize", String(params.pageSize))
    if (params.search) query.set("search", params.search)
    if (params.categoryId) query.set("categoryId", String(params.categoryId))
    if (params.sortBy && params.sortBy !== "lastUpdateTime") query.set("sortBy", params.sortBy)
    if (params.sortType && params.sortType !== "desc") query.set("sortType", params.sortType)
    if (params.view && params.view !== "grid") query.set("view", params.view)
    const suffix = query.toString()
    return suffix ? `/blog?${suffix}` : "/blog"
  },
  searchResults(keyword: string) {
    return `/search?keyword=${encodeURIComponent(keyword)}`
  },
}

export function postHref(post: { id: string; slug?: string | null }) {
  return routes.post(post.slug || post.id)
}
