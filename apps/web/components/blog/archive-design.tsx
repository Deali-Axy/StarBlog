"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { Dialog, DialogClose, DialogContent, DialogOverlay, DialogPortal, DialogTitle } from "@/components/ui/dialog"
import type { PostResponse } from "@/lib/api/types"
import { blogThemeClassName } from "@/lib/blog-theme"
import { formatDate, readingMinutes } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { Archive, ArrowUpRight, FileText, X } from "lucide-react"
import type { ComponentType } from "react"
import { useEffect, useRef, useState, type KeyboardEvent } from "react"
import type { BlogCollectionProps } from "./blog-collection-props"
import type { BlogDesignProps } from "./blog-design-shared"
import { useBlogInteractions } from "./blog-interactions"
import { ArchiveCategoryTree as CategoryTree } from "./views/archive-category-tree"
import { ArchiveCompactPosts } from "./views/archive-compact-posts"
import { ArchiveControlPanel as ControlPanel } from "./views/archive-control-panel"
import { ArchiveExpandedPosts } from "./views/archive-expanded-posts"
import { ArchiveSidebarFooter, BlogBreadcrumb, BlogDirectory, BlogFilters, BlogResults } from "./views/archive-parts"
function ArticlePreview({ post, modal = false }: {
  post: PostResponse
  modal?: boolean
}) {
  const { t } = useI18n()
  const heading = <h2 className={"mt-4.5 [font:600_29px/1.65_var(--font-heading)] wrap-anywhere max-[74.9375rem]:text-[25px] max-[39.9375rem]:text-[27px]"}>{post.title}</h2>
  return <div className={"[padding:22px_26px_32px] max-[74.9375rem]:px-5 max-[39.9375rem]:px-5.5"}>
    <div className={"flex items-center justify-between gap-3 pb-5 [border-bottom:1px_solid_var(--border)] [font:9px_ui-monospace,_monospace] [letter-spacing:.12em] text-muted-foreground"}><span>{t.articlePreview}</span><FileText className="size-4" aria-hidden="true" /></div>
    <span className={"inline-block mt-8 [font:10px_ui-monospace,_monospace] text-primary"}>{post.categoryName || t.posts}</span>
    {modal ? <DialogTitle asChild>{heading}</DialogTitle> : heading}
    <dl className={"grid [grid-template-columns:1fr_1fr] gap-4 [border-block:1px_solid_var(--border)] mt-6 py-4 [font:10px_ui-monospace,_monospace] [&_dt]:mb-2 [&_dt]:text-muted-foreground"}>
      <div><dt>{t.updated}</dt><dd><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
      <div><dt>{t.previewReadingTime}</dt><dd>{t.readingTime(readingMinutes(post.content ?? post.summary))}</dd></div>
    </dl>
    <p className={"[padding-block:24px_32px] text-[.8125rem] [line-height:2.2] wrap-anywhere"}>{post.summary || t.noArticleSummary}</p>
    <Link href={postHref(post)} className={"flex items-center justify-between gap-5 [padding:14px_16px] bg-foreground text-background text-xs"}>{t.readArticle}<ArrowUpRight className="size-4" aria-hidden="true" /></Link>
    <p className={"mt-6 text-[.625rem] [line-height:1.9] text-muted-foreground"}>{t.archivePreviewHint}</p>
  </div>
}
export function ArchiveDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  const Posts = collections[query.view]
  const { posts, title, path, categories, treeState } = props
  const { selectedId, setSelectedId } = useBlogInteractions()
  const [previewOpen, setPreviewOpen] = useState(false)
  const rowRefs = useRef(new Map<string, HTMLButtonElement>())
  const previewTrigger = useRef<HTMLButtonElement | null>(null)
  const selected = posts.items.find((post) => post.id === selectedId) ?? posts.items[0]
  useEffect(() => {
    const desktop = window.matchMedia("(min-width: 1024px)")
    const close = () => {
      if(desktop.matches)
        setPreviewOpen(false)
    }
    desktop.addEventListener("change", close)
    return () => desktop.removeEventListener("change", close)
  }, [])
  function select(post: PostResponse, trigger: HTMLButtonElement) {
    setSelectedId(post.id)
    previewTrigger.current = trigger
    if(window.matchMedia("(max-width: 1023px)").matches)
      setPreviewOpen(true)
  }
  function keyboardSelect(event: KeyboardEvent<HTMLButtonElement>, index: number) {
    if(event.key !== "ArrowUp" && event.key !== "ArrowDown" && event.key !== "Home" && event.key !== "End")
      return
    event.preventDefault()
    const next = event.key === "Home" ? 0 : event.key === "End" ? posts.items.length - 1 : Math.max(0, Math.min(posts.items.length - 1, index + (event.key === "ArrowDown" ? 1 : -1)))
    const post = posts.items[next]
    setSelectedId(post.id)
    rowRefs.current.get(post.id)?.focus()
  }
  return <div data-blog-variant="studio" data-blog-design="archive" className={cn(blogThemeClassName("studio"), "[min-height:calc(100dvh_-_8rem)] [border-block:1px_solid_var(--border)] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[scroll-behavior:auto]!")}><div data-blog-layout="archive" className="[width:min(100%_-_64px,_1280px)] mx-auto pb-12 max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pb-8 [width:min(100%_-_48px,_1360px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pb-8">
    <header className={"flex items-center justify-between gap-5 py-6 max-[39.9375rem]:py-5 max-[39.9375rem]:gap-2.5 max-[39.9375rem]:flex-wrap"}>
      <div className={"flex items-center gap-3.5 [&_h1]:text-[23px] [&_h1]:font-medium [&_h1]:wrap-anywhere [&_small]:block [&_small]:mt-[5px] [&_small]:[font:9px_ui-monospace,_monospace] [&_small]:[letter-spacing:.12em] [&_small]:text-muted-foreground max-[39.9375rem]:[&_h1]:text-xl max-[39.9375rem]:[&_small]:text-[.5rem]"}><span className={"flex items-center justify-center w-11.5 h-11.5 [border:1px_solid_var(--foreground)] [box-shadow:3px_3px_0_var(--border)] max-[39.9375rem]:w-9 max-[39.9375rem]:h-9"}><Archive className="size-5" aria-hidden="true" /></span><div>
        <h1>{query.categoryId ? title : t.archiveWorkbench}</h1><small>{t.archiveEyebrow}</small>
      </div></div>
      <div className={"text-right [font:10px_ui-monospace,_monospace] text-muted-foreground [&_strong]:block [&_strong]:text-[25px] [&_strong]:font-normal [&_strong]:text-foreground [&_strong]:mb-[3px] max-[39.9375rem]:ml-auto max-[39.9375rem]:[&_strong]:text-xl"}><strong>{posts.totalCount}</strong>{t.archiveCountLabel}</div>
      <BlogDirectory {...props} />
    </header>
    <div className={"grid [grid-template-columns:210px_minmax(0,_1fr)_minmax(270px,_31%)] border border-border [height:clamp(440px,_calc(100dvh_-_172px),_1000px)] bg-card max-[74.9375rem]:[grid-template-columns:185px_minmax(0,_1fr)_280px] max-[63.9375rem]:block max-[63.9375rem]:h-auto max-[63.9375rem]:max-h-[none]"}>
      <aside className={"flex min-h-0 flex-col [border-right:1px_solid_var(--border)] bg-background max-[63.9375rem]:hidden"}>
        <div className={"flex justify-between [padding:18px_16px] [border-bottom:1px_solid_var(--border)] text-[.6875rem]"}><span>{t.topicDirectory}</span><span aria-hidden="true">⌘</span></div>
        <CategoryTree categoryNodes={categories} state={treeState} className="min-h-0 flex-1" />
        <ArchiveSidebarFooter />
      </aside>
      <div className={"flex min-w-0 min-h-0 flex-col"}>
        <div className={"[padding:12px_18px] [border-bottom:1px_solid_var(--border)] max-[39.9375rem]:px-3"}><BlogBreadcrumb path={path} /><ControlPanel /></div>
        <BlogFilters title={title} />
        <BlogResults posts={posts}>
          <Posts posts={posts} selected={selected} rowRefs={rowRefs} select={select} keyboardSelect={keyboardSelect} />
        </BlogResults>
      </div>
      <aside id="blog-archive-preview" aria-label={t.articlePreview} className={"min-w-0 overflow-y-auto [border-left:1px_solid_var(--border)] bg-background [scrollbar-width:thin] max-[63.9375rem]:hidden"}>
        {selected ? <ArticlePreview post={selected} /> : <div className={"[padding:22px_26px_32px] max-[74.9375rem]:px-5 max-[39.9375rem]:px-5.5"}><p className={"mt-6 text-[.625rem] [line-height:1.9] text-muted-foreground"}>{t.previewEmpty}</p></div>}
      </aside>
    </div>
    <Dialog open={previewOpen && Boolean(selected)} onOpenChange={setPreviewOpen}>
      <DialogPortal>
        <DialogOverlay className={"fixed [inset:0] [z-index:50] [background:rgb(0_0_0_/_.45)] [backdrop-filter:blur(5px)]"} />
        <DialogContent aria-describedby={undefined} className={cn(blogThemeClassName("studio"), "fixed [z-index:50] [inset:16px] overflow-y-auto border border-border bg-card max-[39.9375rem]:[inset:8px] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]! motion-reduce:[&_*]:[scroll-behavior:auto]!")} onCloseAutoFocus={(event) => { event.preventDefault(); previewTrigger.current?.focus() }}>
          <div className={"sticky top-0 flex justify-end [padding:8px_12px] bg-card [z-index:1]"}><DialogClose asChild><button type="button" className={"inline-flex w-9 h-9 items-center justify-center cursor-pointer"} aria-label={t.close}><X className="size-5" /></button></DialogClose></div>
          {selected ? <ArticlePreview post={selected} modal /> : null}
        </DialogContent>
      </DialogPortal>
    </Dialog>
  </div></div>
}
const collections = { grid: ArchiveExpandedPosts, list: ArchiveCompactPosts } satisfies Record<"grid" | "list", ComponentType<BlogCollectionProps>>
