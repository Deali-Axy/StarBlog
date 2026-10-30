import type { BlogVariant } from "@/lib/blog-variant"

export const routes = {
  home: "/",
  blog: "/blog",
  blogRandom: "/blog/random",
  search: "/search",
  feed: "/feed",
  photography: "/photography",
  photographyRandom: "/photography/random",
  about: "/about",
  links: "/links",
  rss: "/rss",
  post(idOrSlug: string) {
    return `/blog/${encodeURIComponent(idOrSlug)}`
  },
  legacyPost(slug: string) {
    return `/p/${encodeURIComponent(slug)}`
  },
  photo(id: string) {
    return `/photography/${encodeURIComponent(id)}`
  },
  blogList(
    params: {
      page?: number
      pageSize?: number
      search?: string
      categoryId?: number
      sortBy?: string
      sortType?: "asc" | "desc"
      view?: "grid" | "list"
      bv?: BlogVariant
    } = {},
  ) {
    const query = new URLSearchParams()
    if (params.page && params.page > 1) query.set("page", String(params.page))
    if (params.pageSize && params.pageSize !== 12) query.set("pageSize", String(params.pageSize))
    if (params.search) query.set("search", params.search)
    if (params.categoryId) query.set("categoryId", String(params.categoryId))
    if (params.sortBy && params.sortBy !== "lastUpdateTime") query.set("sortBy", params.sortBy)
    if (params.sortType && params.sortType !== "desc") query.set("sortType", params.sortType)
    if (params.view && params.view !== "grid") query.set("view", params.view)
    if (params.bv) query.set("bv", params.bv)
    const suffix = query.toString()
    return suffix ? `/blog?${suffix}` : "/blog"
  },
  photographyList(params: { page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    if (params.page && params.page > 1) query.set("page", String(params.page))
    if (params.pageSize && params.pageSize !== 12) query.set("pageSize", String(params.pageSize))
    const suffix = query.toString()
    return suffix ? `/photography?${suffix}` : "/photography"
  },
  searchResults(keyword: string) {
    return `/search?keyword=${encodeURIComponent(keyword)}`
  },
}

export function postHref(post: { id: string; slug?: string | null }) {
  return routes.post(post.slug || post.id)
}
