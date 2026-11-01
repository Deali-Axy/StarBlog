"use client"

import { useI18n } from "@/components/app-providers"
import { ArchiveDesign } from "@/components/blog/archive-design"
import type { BlogDesignProps } from "@/components/blog/blog-design-shared"
import { useBlogLayout } from "@/components/blog/blog-layout-context"
import { BlogNavigationProvider, useBlogNavigation } from "@/components/blog/blog-navigation"
import type { CategoryTreeState } from "@/components/blog/category-tree"
import { FolioDesign } from "@/components/blog/folio-design"
import { IndexDesign } from "@/components/blog/index-design"
import type { CategoryNodeResponse, PageResult, PostResponse } from "@/lib/api/types"
import { categoryPath } from "@/lib/blog-categories"
import type { BlogQuery } from "@/lib/blog-query"
import type { BlogVariant } from "@/lib/blog-variant"
import { BLOG_PREVIEW } from "@/lib/blog-variant"
import type { ComponentType } from "react"
import { useMemo, useState } from "react"
import { BlogInteractionProvider } from "./blog-interactions"
const designs = { library: IndexDesign, editorial: FolioDesign, studio: ArchiveDesign } satisfies Record<BlogVariant, ComponentType<BlogDesignProps>>

interface BlogExplorerProps {
  categories: CategoryNodeResponse[]
  posts: PageResult<PostResponse>
  query: BlogQuery
}

export function BlogExplorer({ categories, posts, query }: BlogExplorerProps) {
  const { variant } = useBlogLayout()
  const activeQuery = { ...query, bv: BLOG_PREVIEW ? variant : undefined }
  return <BlogNavigationProvider query={activeQuery} variant={variant}>
    <BlogExplorerContent categories={categories} posts={posts} query={activeQuery} />
  </BlogNavigationProvider>
}

function BlogExplorerContent({ categories, posts, query }: BlogExplorerProps) {
  const { t } = useI18n()
  const { variant } = useBlogNavigation()
  const [categorySearch, setCategorySearch] = useState("")
  const path = useMemo(() => categoryPath(categories, query.categoryId), [categories, query.categoryId])
  const [treeState, setTreeState] = useState(() => ({ categoryId: query.categoryId, expanded: new Set(path.slice(0, -1).map((node) => node.id)) }))
  if(treeState.categoryId !== query.categoryId) {
    setTreeState({ categoryId: query.categoryId, expanded: new Set([...treeState.expanded, ...path.slice(0, -1).map((node) => node.id)]) })
  }
  const state: CategoryTreeState = {
    expanded: treeState.expanded, search: categorySearch, setSearch: setCategorySearch,
    toggle: (id) => setTreeState((previous) => {
      const expanded = new Set(previous.expanded)
      if(expanded.has(id)) expanded.delete(id)
      else expanded.add(id)
      return { ...previous, expanded }
    }),
  }
  const props: BlogDesignProps = { categories, posts, path, treeState: state, title: path.at(-1)?.text ?? (query.categoryId ? String(query.categoryId) : t.allPosts) }
  const Design = designs[variant]
  return <BlogInteractionProvider><Design {...props} /></BlogInteractionProvider>
}
