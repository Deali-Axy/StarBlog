"use client"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { blogThemeClassName } from "@/lib/blog-theme"
import { cn } from "@/lib/utils"
import type { ComponentType } from "react"
import type { BlogCollectionProps } from "./blog-collection-props"
import type { BlogDesignProps } from "./blog-design-shared"
import { FolioCompactPosts } from "./views/folio-compact-posts"
import { FolioControlPanel as ControlPanel } from "./views/folio-control-panel"
import { FolioExpandedPosts } from "./views/folio-expanded-posts"
import { BlogBreadcrumb, BlogDirectory, BlogFilters, BlogResults } from "./views/folio-parts"
export function FolioDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  const Posts = collections[query.view]
  const { posts, title, path } = props
  return <div data-blog-variant="editorial" data-blog-design="folio" className={cn(blogThemeClassName("editorial"), "min-h-[calc(100dvh_-_8rem)] motion-reduce:[&_*]:transition-none! motion-reduce:[&_*]:animate-none! motion-reduce:[&_*]:scroll-auto!")}>
    <div data-blog-layout="folio" className="mx-auto w-[min(100%_-_96px,1200px)] pb-12 max-lg:w-[calc(100%_-_48px)] max-sm:w-[calc(100%_-_32px)]">
      <h1 className="sr-only">{query.categoryId ? title : t.fieldNotes}</h1>
      <div className="pt-5 sm:pt-8">
        <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
          <BlogBreadcrumb path={path} />
          <BlogDirectory {...props} />
        </div>
        <ControlPanel />
      </div>
      <BlogFilters title={title} />
      <BlogResults posts={posts}>
        <Posts posts={posts} />
      </BlogResults>
    </div>
  </div>
}

const collections = { grid: FolioExpandedPosts, list: FolioCompactPosts } satisfies Record<"grid" | "list", ComponentType<BlogCollectionProps>>
