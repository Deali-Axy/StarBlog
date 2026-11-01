"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { Dialog, DialogClose, DialogContent, DialogOverlay, DialogPortal, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import type { CategoryNodeResponse, PageResult, PostResponse } from "@/lib/api/types"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { ArrowDownRight, ArrowUpRight, ChevronRight, Rss, Shuffle, X } from "lucide-react"
import { useState, type ReactNode } from "react"
import type { BlogDesignProps } from "../blog-design-shared"
import { IndexCategoryTree as CategoryTree } from "./index-category-tree"
import { IndexPaginationNav as PaginationNav } from "./index-pagination"
export function BlogBreadcrumb({ path }: {
  path: CategoryNodeResponse[]
}) {
  const { t } = useI18n()
  const { href, query } = useBlogNavigation()
  return <nav aria-label={t.categoryPath} className={"flex items-center flex-wrap gap-1.5 text-[.6875rem] text-muted-foreground [&_a:hover]:text-foreground max-[39.9375rem]:text-[.625rem]"}>
    <BlogLink href={href({ categoryId: undefined })}>{t.posts}</BlogLink>
    {path.map((node) => <span key={node.id} className="inline-flex min-w-0 items-center gap-1">
      <ChevronRight className="size-3 shrink-0" />
      <BlogLink href={href({ categoryId: node.id })} aria-current={query.categoryId === node.id ? "page" : undefined} className="max-w-36 truncate">{node.text}</BlogLink>
    </span>)}
  </nav>
}
export function BlogDirectoryFooter() {
  const { t } = useI18n()
  return <div className={"flex justify-between gap-4 [padding:16px_12px_0] [border-top:1px_solid_var(--border)] text-[.6875rem] text-muted-foreground [&_a]:inline-flex [&_a]:items-center [&_a]:gap-2"}>
    <Link href={routes.blogRandom}><Shuffle className="size-3" />{t.randomPost}</Link>
    <a href={routes.feed}><Rss className="size-3" />{t.subscribe}</a>
  </div>
}
export function BlogDirectory({ categories, treeState, title, }: Pick<BlogDesignProps, "categories" | "treeState" | "title">) {
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  return <Dialog open={open} onOpenChange={setOpen}>
    <DialogTrigger asChild>
      <button type="button" aria-label={t.openCategories} className={"inline-flex items-center justify-between gap-4 min-h-11 max-w-full [padding:10px_16px] [border:1px_solid_var(--foreground)] text-xs cursor-pointer [&_span]:overflow-hidden [&_span]:text-ellipsis [&_span]:whitespace-nowrap [&&]:border-0 [&&]:bg-highlight [&&]:[color:#202719] [&&]:min-w-[170px] max-[39.9375rem]:[&&]:min-w-0 max-[39.9375rem]:[&&]:max-w-[55%] max-[39.9375rem]:[&&]:px-2.5 max-[39.9375rem]:[&&]:text-[.6875rem]"}>
        <span>{t.topicDirectory} / {title}</span><ArrowDownRight className="size-4 shrink-0" />
      </button>
    </DialogTrigger>
    <DialogPortal>
      <DialogOverlay className={"fixed [inset:0] [z-index:50] [background:rgb(0_0_0_/_.45)] [backdrop-filter:blur(5px)]"} />
      <DialogContent data-directory-layout="wide" className={cn(blogThemeClassName("library"), "fixed [z-index:50] top-[10dvh] left-[50%] flex flex-col [width:min(1050px,_calc(100vw_-_64px))] max-h-[80dvh] [transform:translateX(-50%)] border border-border p-5 [box-shadow:0_20px_80px_rgb(0_0_0_/_.2)] bg-card max-[39.9375rem]:[inset:16px] max-[39.9375rem]:w-auto max-[39.9375rem]:max-h-[none] max-[39.9375rem]:p-3 max-[39.9375rem]:[transform:none] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[scroll-behavior:auto]!")}>
        <div className={"flex items-center justify-between gap-4 [padding:4px_12px_16px] [border-bottom:1px_solid_var(--border)] [&_h2]:text-[1.375rem] [&_h2]:font-medium"}>
          <DialogTitle>{t.topicDirectory}</DialogTitle>
          <DialogClose asChild><button type="button" aria-label={t.close} className={"inline-flex w-9 h-9 items-center justify-center cursor-pointer"}><X className="size-5" /></button></DialogClose>
        </div>
        <CategoryTree categoryNodes={categories} state={treeState} onSelect={() => setOpen(false)} className={"min-h-0"} />
        <BlogDirectoryFooter />
      </DialogContent>
    </DialogPortal>
  </Dialog>
}
export function BlogFilters({ title }: {
  title: string
}) {
  const { t } = useI18n()
  const { query, href } = useBlogNavigation()
  if(!query.categoryId && !query.search)
    return null
  return <div className={"flex flex-wrap gap-2 py-2.5 text-[.6875rem] [&_a]:inline-flex [&_a]:items-center [&_a]:gap-2 [&_a]:max-w-full [&_a]:[padding:4px_8px] [&_a]:border [&_a]:border-border [&_a_span]:overflow-hidden [&_a_span]:text-ellipsis [&_a_span]:whitespace-nowrap"}>
    {query.categoryId ? <BlogLink href={href({ categoryId: undefined })} aria-label={`${t.removeCategory}: ${title}`}><span>{title}</span><X className="size-3 shrink-0" /></BlogLink> : null}
    {query.search ? <BlogLink href={href({ search: undefined })} aria-label={`${t.removeSearch}: ${query.search}`}><span>“{query.search}”</span><X className="size-3 shrink-0" /></BlogLink> : null}
    <BlogLink href={href({ search: undefined, categoryId: undefined, page: 1 })}>{t.clearFilters}</BlogLink>
  </div>
}
export function BlogResults({ posts, children }: {
  posts: PageResult<PostResponse>
  children: ReactNode
}) {
  const { t } = useI18n()
  const { query, pending, href } = useBlogNavigation()
  const filtered = Boolean(query.categoryId || query.search)
  const unavailable = posts.totalCount > 0 && !posts.items.length && query.page > posts.totalPages
  return <>
    <div role="status" aria-live="polite" className={"min-h-5 text-[.6875rem] text-primary"}>{pending ? t.loadingPosts : <span className="sr-only">{t.postCount(posts.totalCount)}</span>}</div>
    <section aria-label={t.posts} aria-busy={pending} className={"[transition:opacity_.15s] [&[aria-busy=true]]:[opacity:.5]"}>
      <div className={""}>
        {posts.items.length ? children : <div className={"flex min-h-[320px] flex-col items-center justify-center gap-4 text-center p-8 [border-block:1px_solid_var(--border)] [&_h2]:text-xl [&_p]:text-[.8125rem] [&_p]:text-muted-foreground [&_a]:[border-bottom:1px_solid_currentColor] [&_a]:pb-1"}>
          <span aria-hidden="true" className="font-mono text-4xl text-muted-foreground">∅</span>
          <h2>{unavailable ? t.pageUnavailable : filtered ? t.noMatchingPosts : t.noPosts}</h2>
          {!filtered && !unavailable ? <p>{t.noPublishedPosts}</p> : null}
          {filtered || unavailable ? <BlogLink href={href(unavailable ? { page: 1 } : { search: undefined, categoryId: undefined, page: 1 })}>
            {unavailable ? t.backToFirstPage : t.clearFilters}<ArrowUpRight className="ml-2 inline size-3" />
          </BlogLink> : null}
        </div>}
      </div>
      <PaginationNav totalPages={posts.totalPages} totalCount={posts.totalCount} itemCount={posts.items.length} />
    </section>
  </>
}
