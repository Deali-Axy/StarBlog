"use client"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { blogThemeClassName } from "@/lib/blog-theme"
import { cn } from "@/lib/utils"
import type { ComponentType } from "react"
import type { BlogCollectionProps } from "./blog-collection-props"
import type { BlogDesignProps } from "./blog-design-shared"
import { IndexCompactPosts } from "./views/index-compact-posts"
import { IndexControlPanel as ControlPanel } from "./views/index-control-panel"
import { IndexExpandedPosts } from "./views/index-expanded-posts"
import { BlogBreadcrumb, BlogDirectory, BlogFilters, BlogResults } from "./views/index-parts"
export function IndexDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  const Posts = collections[query.view]
  const { posts, title, path } = props
  return <div data-blog-variant="library" data-blog-design="index" className={cn(blogThemeClassName("library"), "[min-height:calc(100dvh_-_8rem)] [border-block:1px_solid_var(--border)] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[scroll-behavior:auto]!")}><div data-blog-layout="index" className="[width:min(100%_-_64px,_1280px)] mx-auto pb-12 max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pb-8">
    <header className={"grid [grid-template-columns:minmax(0,_1fr)_210px] items-end gap-10 [padding-block:56px_40px] min-[93.75rem]:pt-16 max-[63.9375rem]:[grid-template-columns:minmax(0,_1fr)_170px] max-[63.9375rem]:gap-6 max-[39.9375rem]:block max-[39.9375rem]:relative max-[39.9375rem]:py-8 max-[39.9375rem]:pr-15"}>
      <div>
        <p className={"text-[.6875rem] [letter-spacing:.18em] mb-4.5 max-[39.9375rem]:text-[.5625rem] max-[39.9375rem]:mb-4"}>{t.indexEyebrow}</p>
        <h1 className={"text-[clamp(64px,_8.3vw,_120px)] font-bold [letter-spacing:-.075em] [line-height:1.12] wrap-anywhere [&:lang(en)]:text-[clamp(64px,_7vw,_108px)] max-[39.9375rem]:text-[55px] max-[39.9375rem]:[&:lang(en)]:text-[55px]"}>{query.categoryId ? title : t.typeIndex}</h1>
        <p className={"text-xs [line-height:1.9] max-w-[260px] mt-5 text-muted-foreground max-[39.9375rem]:max-w-[190px] max-[39.9375rem]:mt-4.5 max-[39.9375rem]:text-[.625rem]"}>{t.indexDescription}</p>
      </div>
      <div className={"relative [padding:16px_20px] bg-highlight [color:#202719] [transform:rotate(-4deg)] [&_strong]:block [&_strong]:[font:500_72px/1_ui-monospace,_monospace] [&_strong]:[letter-spacing:-.08em] [&_span]:block [&_span]:mt-4 [&_span]:text-[.625rem] [&_span]:[letter-spacing:.14em] max-[63.9375rem]:[&_strong]:text-[56px] max-[39.9375rem]:absolute max-[39.9375rem]:right-0 max-[39.9375rem]:bottom-8 max-[39.9375rem]:[padding:12px_10px] max-[39.9375rem]:w-22.5 max-[39.9375rem]:[&_strong]:text-[34px] max-[39.9375rem]:[&_span]:text-[.5rem] max-[39.9375rem]:[&_span]:mt-2.5 max-[39.9375rem]:[&_span]:[letter-spacing:0]"}><strong>{posts.totalCount}</strong><span>{t.indexCountLabel}</span></div>
    </header>
    <div className={"flex justify-between items-center gap-4 [border-top:3px_solid_var(--foreground)] [border-bottom:1px_solid_var(--foreground)] py-3 max-[39.9375rem]:gap-2.5"}>
      <BlogDirectory {...props} />
      <BlogBreadcrumb path={path} />
    </div>
    <ControlPanel />
    <BlogFilters title={title} />
    <BlogResults posts={posts}>
      <Posts posts={posts} />
    </BlogResults>
  </div></div>
}
const collections = { grid: IndexExpandedPosts, list: IndexCompactPosts } satisfies Record<"grid" | "list", ComponentType<BlogCollectionProps>>
