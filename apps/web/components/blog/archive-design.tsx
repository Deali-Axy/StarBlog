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
import { Archive, ArrowUpRight, X } from "lucide-react"
import type { ComponentType } from "react"
import { useEffect, useRef, useState, type KeyboardEvent } from "react"
import { ArticleCover } from "./article-cover"
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
    const { coverMode } = useBlogInteractions()
    const heading = <h2 className={"mt-4.5 [font:600_29px/1.65_var(--font-heading)] wrap-anywhere max-[74.9375rem]:text-[25px] max-[39.9375rem]:text-[27px]"}>{post.title}</h2>

    return <div className={"pt-[22px] px-[26px] pb-[32px] max-[74.9375rem]:px-5 max-[39.9375rem]:px-5.5"}>
        {modal ? <DialogTitle asChild>{heading}</DialogTitle> : heading}

        <dl className={"grid grid-cols-[1fr_1fr] gap-4 [border-block:1px_solid_var(--border)] mt-6 py-4 [font:10px_ui-monospace,_monospace] [&_dt]:mb-2 [&_dt]:text-muted-foreground"}>
            <div><dt>{t.updated}</dt><dd><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
            <div><dt>{t.previewReadingTime}</dt><dd>{t.readingTime(readingMinutes(post.content ?? post.summary))}</dd></div>
        </dl>

        <ArticleCover
            key={`${post.id}:${post.coverUrl ?? "auto"}:${post.coverHidden}`}
            post={post}
            eager
            mode={coverMode} />

        <p className={"pbs-[24px] pbe-[32px] text-[.8125rem] leading-[2.2] wrap-anywhere"}>{post.summary || t.noArticleSummary}</p>
        <Link href={postHref(post)} className={"flex items-center justify-between gap-5 py-[14px] px-[16px] bg-foreground text-background text-xs"}>{t.readArticle}<ArrowUpRight className="size-4" aria-hidden="true" /></Link>
        <p className={"mt-6 text-[.625rem] leading-[1.9] text-muted-foreground"}>{t.archivePreviewHint}</p>
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
            if (desktop.matches)
                setPreviewOpen(false)
        }
        desktop.addEventListener("change", close)
        return () => desktop.removeEventListener("change", close)
    }, [])

    function select(post: PostResponse, trigger: HTMLButtonElement) {
        setSelectedId(post.id)
        previewTrigger.current = trigger
        if (window.matchMedia("(max-width: 1023px)").matches)
            setPreviewOpen(true)
    }

    function keyboardSelect(event: KeyboardEvent<HTMLButtonElement>, index: number) {
        if (event.key !== "ArrowUp" && event.key !== "ArrowDown" && event.key !== "Home" && event.key !== "End")
            return
        event.preventDefault()
        const next = event.key === "Home" ? 0 : event.key === "End" ? posts.items.length - 1 : Math.max(0, Math.min(posts.items.length - 1, index + (event.key === "ArrowDown" ? 1 : -1)))
        const post = posts.items[next]
        setSelectedId(post.id)
        rowRefs.current.get(post.id)?.focus()
    }

    return <div data-blog-variant="studio" data-blog-design="archive" className={cn(blogThemeClassName("studio"), "min-h-[calc(100dvh_-_var(--blog-header-height))] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:animate-none! motion-reduce:[&_*]:scroll-auto!")}><div data-blog-layout="archive" className="w-full">
        <div className="grid h-[calc(100dvh_-_var(--blog-header-height))] min-h-0 grid-cols-[240px_minmax(0,1fr)_minmax(300px,31%)] grid-rows-[auto_minmax(0,1fr)] bg-background max-[74.9375rem]:grid-cols-[210px_minmax(0,1fr)_280px] max-[63.9375rem]:block max-[63.9375rem]:h-auto max-[63.9375rem]:min-h-0">
            <header className="col-start-1 row-start-1 grid grid-cols-[28px_minmax(0,1fr)] items-center gap-x-2.5 gap-y-2 border-r border-b border-border bg-background px-3.5 py-3.5 max-[63.9375rem]:border-r-0 max-[39.9375rem]:px-3">
                <span className="flex size-7 shrink-0 items-center justify-center border border-foreground shadow-[2px_2px_0_var(--border)]"><Archive className="size-3.5" aria-hidden="true" /></span>
                <div className="flex min-w-0 items-center justify-between gap-2">
                    <h1 className="min-w-0 wrap-anywhere text-base font-medium leading-snug">{query.categoryId ? title : t.archiveWorkbench}</h1>
                    <span className="shrink-0 font-mono text-[10px] leading-4 tabular-nums text-muted-foreground"><span className="sr-only">{t.archiveCountLabel}: </span>{posts.totalCount}</span>
                </div>
                <p className="col-span-full font-mono text-[8px] leading-relaxed tracking-[.06em] text-muted-foreground">{t.archiveEyebrow}</p>
                <div className="col-span-full hidden max-[63.9375rem]:block"><BlogDirectory {...props} /></div>
            </header>
            <aside className="col-start-1 row-start-2 flex min-h-0 flex-col border-r border-border bg-background max-[63.9375rem]:hidden">
                <CategoryTree categoryNodes={categories} state={treeState} className="min-h-0 flex-1" />
                <ArchiveSidebarFooter />
            </aside>
            <div className="col-start-2 row-start-1 row-span-2 flex min-w-0 min-h-0 flex-col">
                <div className="border-b border-border px-4 py-2 max-[39.9375rem]:px-3">{path.length ? <div className="pb-1"><BlogBreadcrumb path={path} /></div> : null}<ControlPanel /></div>
                <BlogFilters title={title} />
                <BlogResults posts={posts}>
                    <Posts posts={posts} selected={selected} rowRefs={rowRefs} select={select} keyboardSelect={keyboardSelect} />
                </BlogResults>
            </div>
            <aside id="blog-archive-preview" aria-label={t.articlePreview} className={"col-start-3 row-start-1 row-span-2 min-w-0 overflow-y-auto [border-left:1px_solid_var(--border)] bg-background [scrollbar-width:thin] max-[63.9375rem]:hidden"}>
                {selected ? <ArticlePreview post={selected} /> : <div className={"pt-[22px] px-[26px] pb-[32px] max-[74.9375rem]:px-5 max-[39.9375rem]:px-5.5"}><p className={"mt-6 text-[.625rem] leading-[1.9] text-muted-foreground"}>{t.previewEmpty}</p></div>}
            </aside>
        </div>
        <Dialog open={previewOpen && Boolean(selected)} onOpenChange={setPreviewOpen}>
            <DialogPortal>
                <DialogOverlay className={"fixed inset-0 z-50 [background:rgb(0_0_0_/_.45)] [backdrop-filter:blur(5px)]"} />
                <DialogContent aria-describedby={undefined} className={cn(blogThemeClassName("studio"), "fixed z-50 inset-[16px] overflow-y-auto border border-border bg-card max-[39.9375rem]:inset-[8px] motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:animate-none! motion-reduce:[&_*]:scroll-auto!")} onCloseAutoFocus={(event) => { event.preventDefault(); previewTrigger.current?.focus() }}>
                    <div className={"sticky top-0 flex justify-end py-[8px] px-[12px] bg-card z-1"}><DialogClose asChild><button type="button" className={"inline-flex w-9 h-9 items-center justify-center cursor-pointer"} aria-label={t.close}><X className="size-5" /></button></DialogClose></div>
                    {selected ? <ArticlePreview post={selected} modal /> : null}
                </DialogContent>
            </DialogPortal>
        </Dialog>
    </div></div>
}
const collections = { grid: ArchiveExpandedPosts, list: ArchiveCompactPosts } satisfies Record<"grid" | "list", ComponentType<BlogCollectionProps>>
