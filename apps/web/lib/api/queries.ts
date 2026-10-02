import { apiGet } from "@/lib/api/client"
import type {
  CategoryNodeResponse,
  HomeResponse,
  ListPostsQuery,
  PageResult,
  PostResponse,
  SearchQuery,
  SearchResultResponse,
  SiteOverviewResponse,
} from "@/lib/api/types"

const revalidate = { next: { revalidate: 300 } }

export function getHome() {
  return apiGet<HomeResponse>("/api/v1/site/home", undefined, revalidate)
}

export function getOverview() {
  return apiGet<SiteOverviewResponse>("/api/v1/site/overview", undefined, revalidate)
}

export function listPosts(query: ListPostsQuery = {}) {
  return apiGet<PageResult<PostResponse>>(
    "/api/v1/posts",
    {
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 12,
      search: query.search,
      categoryId: query.categoryId,
    },
    revalidate,
  )
}

export function getPost(id: string) {
  return apiGet<PostResponse>(`/api/v1/posts/${encodeURIComponent(id)}`, undefined, revalidate)
}

export function getPostBySlug(slug: string) {
  return apiGet<PostResponse>(
    `/api/v1/posts/by-slug/${encodeURIComponent(slug)}`,
    undefined,
    revalidate,
  )
}

export function getCategoryTree() {
  return apiGet<CategoryNodeResponse[]>("/api/v1/categories/tree", undefined, revalidate)
}

export function searchPosts(query: SearchQuery) {
  return apiGet<SearchResultResponse[]>(
    "/api/v1/site/search",
    {
      keyword: query.keyword,
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 20,
    },
    { cache: "no-store" },
  )
}

const POST_ID = /^[0-9a-f]{16}$/i

export async function getPublishedPost(idOrSlug: string) {
  const tryFetch = async (fn: () => Promise<PostResponse>) => {
    try {
      const post = await fn()
      if (!post.isPublish) return null
      return post
    } catch (error) {
      if (typeof error === "object" && error && "status" in error && error.status === 404) {
        return null
      }
      throw error
    }
  }

  if (POST_ID.test(idOrSlug)) {
    return (await tryFetch(() => getPost(idOrSlug))) ?? tryFetch(() => getPostBySlug(idOrSlug))
  }
  return (await tryFetch(() => getPostBySlug(idOrSlug))) ?? tryFetch(() => getPost(idOrSlug))
}
