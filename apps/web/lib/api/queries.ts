import { apiGet } from "@/lib/api/client"
import type {
  CategoryNodeResponse,
  CategoryWordCloudItem,
  CommentResponse,
  HomeResponse,
  LinkResponse,
  ListPhotosQuery,
  ListPostsQuery,
  PageResult,
  PhotoResponse,
  PostResponse,
  PostTranslationResponse,
  SearchQuery,
  SearchResultResponse,
  SiteOverviewResponse,
} from "@/lib/api/types"

const revalidate = { next: { revalidate: 300 } }
const noStore = { cache: "no-store" as const }

function notFoundToNull(error: unknown) {
  if (typeof error === "object" && error && "status" in error && error.status === 404) return null
  throw error
}

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
      sortBy: query.sortBy,
      sortType: query.sortType,
      includeDescendants: query.includeDescendants,
    },
    revalidate,
  )
}

export function getPost(id: string, lang?: string) {
  return apiGet<PostResponse>(
    `/api/v1/posts/${encodeURIComponent(id)}`,
    lang ? { lang } : undefined,
    revalidate,
  )
}

export function getPostBySlug(slug: string) {
  return apiGet<PostResponse>(
    `/api/v1/posts/by-slug/${encodeURIComponent(slug)}`,
    undefined,
    revalidate,
  )
}

export function getRelatedPosts(id: string, limit = 4) {
  return apiGet<PostResponse[]>(
    `/api/v1/posts/${encodeURIComponent(id)}/related`,
    { limit },
    revalidate,
  )
}

export function getRandomPost() {
  return apiGet<PostResponse>("/api/v1/posts/random", undefined, noStore)
}

export function getPostTranslations(id: string) {
  return apiGet<PostTranslationResponse[]>(
    `/api/v1/posts/${encodeURIComponent(id)}/translations`,
    undefined,
    revalidate,
  )
}

export function getCategoryTree() {
  return apiGet<CategoryNodeResponse[]>("/api/v1/categories/tree", undefined, revalidate)
}

export function getCategoryWordCloud() {
  return apiGet<CategoryWordCloudItem[]>("/api/v1/categories/word-cloud", undefined, revalidate)
}

export function searchPosts(query: SearchQuery) {
  return apiGet<SearchResultResponse[]>(
    "/api/v1/site/search",
    {
      keyword: query.keyword,
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 20,
    },
    noStore,
  )
}

export function listPhotos(query: ListPhotosQuery = {}) {
  return apiGet<PageResult<PhotoResponse>>(
    "/api/v1/photos",
    {
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 12,
    },
    revalidate,
  )
}

export function getPhoto(id: string) {
  return apiGet<PhotoResponse>(`/api/v1/photos/${encodeURIComponent(id)}`, undefined, revalidate)
}

export function getRandomPhoto() {
  return apiGet<PhotoResponse>("/api/v1/site/photos/random", undefined, noStore)
}

export async function getAdjacentPhoto(id: string, direction: "next" | "previous") {
  try {
    return await apiGet<PhotoResponse>(
      `/api/v1/site/photos/${encodeURIComponent(id)}/adjacent/${direction}`,
      undefined,
      revalidate,
    )
  } catch (error) {
    return notFoundToNull(error)
  }
}

export function getCommentsByPost(postId: string) {
  return apiGet<CommentResponse[]>(
    `/api/v1/comments/by-post/${encodeURIComponent(postId)}`,
    undefined,
    noStore,
  )
}

export function listVisibleLinks() {
  return apiGet<LinkResponse[]>("/api/v1/links", undefined, revalidate)
}

const POST_ID = /^[0-9a-f]{16}$/i

export async function getPublishedPost(idOrSlug: string, lang?: string) {
  const tryFetch = async (fn: () => Promise<PostResponse>) => {
    try {
      const post = await fn()
      if (!post.isPublish) return null
      return post
    } catch (error) {
      return notFoundToNull(error)
    }
  }

  const byId = POST_ID.test(idOrSlug)
  const post = byId
    ? ((await tryFetch(() => getPost(idOrSlug, lang))) ?? (await tryFetch(() => getPostBySlug(idOrSlug))))
    : ((await tryFetch(() => getPostBySlug(idOrSlug))) ?? (await tryFetch(() => getPost(idOrSlug, lang))))

  if (post && lang && !byId) {
    return (await tryFetch(() => getPost(post.id, lang))) ?? post
  }
  return post
}
