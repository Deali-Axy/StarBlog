"use client"

import { useQuery } from "@tanstack/react-query"

import { getCategoryTree, getHome, listPosts, searchPosts } from "@/lib/api/queries"
import type { ListPostsQuery, SearchQuery } from "@/lib/api/types"

export const queryKeys = {
  home: ["site", "home"] as const,
  posts: (query: ListPostsQuery) => ["posts", query] as const,
  categories: ["categories", "tree"] as const,
  search: (query: SearchQuery) => ["search", query] as const,
}

export function useHome() {
  return useQuery({ queryKey: queryKeys.home, queryFn: getHome })
}

export function usePosts(query: ListPostsQuery) {
  return useQuery({ queryKey: queryKeys.posts(query), queryFn: () => listPosts(query) })
}

export function useCategoryTree() {
  return useQuery({ queryKey: queryKeys.categories, queryFn: getCategoryTree })
}

export function useSearch(query: SearchQuery, enabled = true) {
  return useQuery({
    queryKey: queryKeys.search(query),
    queryFn: () => searchPosts(query),
    enabled: enabled && Boolean(query.keyword.trim()),
  })
}
