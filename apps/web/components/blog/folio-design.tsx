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
      <header className="grid grid-cols-[minmax(0,1fr)_280px] items-end gap-x-16 gap-y-6 pb-12 pt-12 lg:pb-16 lg:pt-16 max-sm:grid-cols-1 max-sm:gap-5 max-sm:pb-8 max-sm:pt-8">
        <div>
          <p className="mb-6 flex items-center gap-3 font-mono text-[10px] tracking-[.12em] text-muted-foreground max-sm:mb-4 max-sm:text-[9px]">
            <span aria-hidden="true" className="size-1.5 shrink-0 rounded-full bg-primary" />{t.folioRunningTitle}
          </p>
          <h1 className="wrap-anywhere font-heading text-[clamp(48px,6vw,76px)] leading-[1.25] tracking-[-.045em] max-sm:text-[46px]">{query.categoryId ? title : t.fieldNotes}</h1>
        </div>
        <div className="pb-1">
          <p className="font-heading text-sm leading-[2] text-muted-foreground max-sm:max-w-80">{t.folioDescription}</p>
          <p className="mt-5 flex flex-wrap items-center gap-x-3 gap-y-1 font-mono text-[10px] tracking-[.06em] text-muted-foreground max-sm:mt-3">
            <span>{t.postCount(posts.totalCount)}</span><span aria-hidden="true">·</span><span>{t.folioColophon}</span>
          </p>
        </div>
      </header>
      <div className="border-t border-border/60 pt-5">
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
