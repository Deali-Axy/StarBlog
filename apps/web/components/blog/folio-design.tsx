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
  return <div data-blog-variant="editorial" data-blog-design="folio" className={cn(blogThemeClassName("editorial"), "[min-height:calc(100dvh_-_8rem)] [border-block:1px_solid_var(--border)] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[scroll-behavior:auto]!")}><div data-blog-layout="folio" className="[width:min(100%_-_64px,_1280px)] mx-auto pb-12 max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pb-8 [width:min(100%_-_96px,_1200px)] max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pb-8">
    <div className={"flex items-center justify-between gap-4 [padding-block:24px_18px] [border-bottom:1px_solid_var(--foreground)] [font:10px_ui-monospace,_monospace] [letter-spacing:.15em] max-[39.9375rem]:[padding-block:18px_12px] max-[39.9375rem]:text-[.5rem] max-[39.9375rem]:[letter-spacing:.07em]"}><span>{t.folioRunningTitle}</span><span>{t.folioPage} {String(query.page).padStart(2, "0")}</span></div>
    <header className={"relative grid [grid-template-columns:minmax(0,_1fr)_240px] items-center gap-16 [padding-block:44px_36px] max-[63.9375rem]:gap-9 max-[63.9375rem]:[grid-template-columns:minmax(0,_1fr)_180px] max-[39.9375rem]:block max-[39.9375rem]:[padding-block:30px_26px]"}>
      <h1 className={"[font-family:var(--font-heading)] text-[clamp(56px,_6.5vw,_92px)] [line-height:1.3] [letter-spacing:-.055em] wrap-anywhere max-[39.9375rem]:text-[53px]"}>{query.categoryId ? title : t.fieldNotes}</h1>
      <p className={"[font-family:var(--font-heading)] text-[.9375rem] [line-height:2.1] text-muted-foreground [&_>_span]:block [&_>_span]:mt-3.5 [&_>_span]:[font:10px_ui-monospace,_monospace] [&_>_span]:[letter-spacing:.12em] max-[39.9375rem]:mt-4.5 max-[39.9375rem]:text-xs max-[39.9375rem]:[&_>_span]:mt-3 max-[39.9375rem]:[&_>_span]:text-[.5625rem]"}>{t.folioDescription}<span>{t.postCount(posts.totalCount)} · {t.folioColophon}</span></p>
    </header>
    <div className={"flex flex-wrap justify-between items-center gap-4 py-4 [border-block:1px_solid_var(--border)] max-[39.9375rem]:gap-2.5"}><BlogBreadcrumb path={path} /><BlogDirectory {...props} /></div>
    <ControlPanel />
    <BlogFilters title={title} />
    <BlogResults posts={posts}>
      <Posts posts={posts} />
    </BlogResults>
  </div></div>
}
const collections = { grid: FolioExpandedPosts, list: FolioCompactPosts } satisfies Record<"grid" | "list", ComponentType<BlogCollectionProps>>
