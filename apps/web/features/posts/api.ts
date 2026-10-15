"use client"

import { useQuery } from "@tanstack/react-query"

import { getCategoryTree, getHome, listPosts, searchPosts } from "@/lib/api/queries"
import type { ListPostsQuery, SearchQuery } from "@/lib/api/types"
import { useI18n } from "@/components/app-providers"

export const queryKeys = {
  home: ["site", "home"] as const,
  posts: (query: ListPostsQuery) => ["posts", query] as const,
  categories: ["categories", "tree"] as const,
  search: (query: SearchQuery) => ["search", query] as const,
}

export function useHome() {
  const { locale } = useI18n()
  return useQuery({ queryKey: [...queryKeys.home, locale], queryFn: () => getHome(locale) })
}

export function usePosts(query: ListPostsQuery) {
  const { locale } = useI18n()
  return useQuery({ queryKey: [...queryKeys.posts(query), locale], queryFn: () => listPosts(query, locale) })
}

export function useCategoryTree() {
  return useQuery({ queryKey: queryKeys.categories, queryFn: getCategoryTree })
}

export function useSearch(query: SearchQuery, enabled = true) {
  const { locale } = useI18n()
  return useQuery({
    queryKey: [...queryKeys.search(query), locale],
    queryFn: () => searchPosts(query, locale),
    enabled: enabled && Boolean(query.keyword.trim()),
  })
}
