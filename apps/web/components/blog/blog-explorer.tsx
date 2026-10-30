"use client"

import Link from "next/link"
import { useSearchParams } from "next/navigation"
import { useMemo, useState } from "react"
import { Dialog } from "radix-ui"
import { BookOpen, ChevronRight, Command, FolderOpen, Library, Menu, MoveUpRight, Rss, SearchX, Shuffle, X } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { BlogArticleCard } from "@/components/blog/blog-article-card"
import { BlogLink, BlogNavigationProvider, useBlogNavigation } from "@/components/blog/blog-navigation"
import { CategoryTree, type CategoryTreeState } from "@/components/blog/category-tree"
import { ControlPanel } from "@/components/blog/control-panel"
import { PaginationNav } from "@/components/blog/pagination-nav"
import { Button } from "@/components/ui/button"
import type { CategoryNodeResponse, PageResult, PostResponse } from "@/lib/api/types"
import { categoryPath } from "@/lib/blog-categories"
import type { BlogQuery } from "@/lib/blog-query"
import { blogThemeClassName } from "@/lib/blog-theme"
import { BLOG_PREVIEW, BLOG_VARIANT_OPTIONS, resolveBlogVariant, type BlogVariant } from "@/lib/blog-variant"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

interface BlogExplorerProps {
  categories: CategoryNodeResponse[]
  posts: PageResult<PostResponse>
  query: BlogQuery
}

const variantIcons = { library: Library, editorial: BookOpen, studio: Command }

export function BlogExplorer({ categories, posts, query }: BlogExplorerProps) {
  const searchParams = useSearchParams()
  const variant = resolveBlogVariant(searchParams.get("bv"))
  const activeQuery = { ...query, bv: BLOG_PREVIEW ? variant : undefined }
  return (
    <BlogNavigationProvider query={activeQuery} variant={variant}>
      <BlogExplorerContent categories={categories} posts={posts} query={activeQuery} />
    </BlogNavigationProvider>
  )
}

function BlogExplorerContent({ categories, posts, query }: BlogExplorerProps) {
  const { t, locale } = useI18n()
  const { variant, pending, href } = useBlogNavigation()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [categorySearch, setCategorySearch] = useState("")
  const path = useMemo(() => categoryPath(categories, query.categoryId), [categories, query.categoryId])
  const [treeState, setTreeState] = useState(() => ({
    categoryId: query.categoryId,
    expanded: new Set(path.slice(0, -1).map((node) => node.id)),
  }))
  // Reconcile a new selection before rendering; manual collapses stay respected otherwise.
  if (treeState.categoryId !== query.categoryId) {
    setTreeState({
      categoryId: query.categoryId,
      expanded: new Set([...treeState.expanded, ...path.slice(0, -1).map((node) => node.id)]),
    })
  }
  const state: CategoryTreeState = {
    expanded: treeState.expanded,
    search: categorySearch,
    setSearch: setCategorySearch,
    toggle: (id) => setTreeState((previous) => {
      const expanded = new Set(previous.expanded)
      if (expanded.has(id)) expanded.delete(id)
      else expanded.add(id)
      return { ...previous, expanded }
    }),
  }
  const selectedName = path.at(-1)?.text ?? (query.categoryId ? String(query.categoryId) : t.allPosts)
  const title = query.categoryId ? selectedName : t.allPosts
  const Icon = variantIcons[variant]
  const filtered = Boolean(query.search || query.categoryId)
  const pageUnavailable = posts.totalCount > 0 && posts.items.length === 0 && query.page > posts.totalPages
  const clearPatch = { search: undefined, categoryId: undefined, page: 1 }
  const sidebarClassName = cn("flex min-h-0 flex-col",
    variant === "library" && "rounded-2xl border border-border bg-card shadow-sm shadow-foreground/2",
    variant === "editorial" && "border-t-2 border-foreground lg:border-r lg:border-r-border lg:pr-4",
    variant === "studio" && "rounded-lg border border-border bg-card/70")

  function switchVariant(next: BlogVariant) {
    const url = new URL(window.location.href)
    url.searchParams.set("bv", next)
    window.history.pushState(null, "", `${url.pathname}${url.search}${url.hash}`)
  }

  const sidebar = (mobile = false) => (
    <>
      <CategoryTree categoryNodes={categories} state={state} onSelect={mobile ? () => setDrawerOpen(false) : undefined} className="min-h-0 flex-1" />
      <div className="mx-4 flex shrink-0 items-center justify-between gap-2 border-t border-border py-4 text-xs text-muted-foreground">
        <Link href={routes.blogRandom} className="inline-flex items-center gap-1.5 hover:text-primary"><Shuffle className="size-3.5" />{t.randomPost}</Link>
        <a href={routes.feed} className="inline-flex items-center gap-1.5 hover:text-primary"><Rss className="size-3.5" />{t.subscribe}</a>
      </div>
    </>
  )

  return (
    <div data-blog-variant={variant} className={cn(blogThemeClassName(variant), "relative min-h-[calc(100dvh-8rem)] border-y border-border/70")}>
      <div className="mx-auto w-full max-w-7xl px-4 py-6 sm:px-8 sm:py-8">
        {BLOG_PREVIEW ? (
          <div className="mb-7 flex flex-wrap items-center justify-end gap-3 border-b border-border/70 pb-5">
            <span className="text-[11px] tracking-wider text-muted-foreground">{t.designPreview}</span>
            <div role="group" aria-label={t.designPreview} className="flex max-w-full flex-wrap gap-1 rounded-lg border border-border bg-card/80 p-1">
              {BLOG_VARIANT_OPTIONS.map((option) => {
                const OptionIcon = variantIcons[option.id]
                return <button key={option.id} type="button" aria-pressed={variant === option.id} onClick={() => switchVariant(option.id)}
                  className={cn("flex items-center gap-1.5 rounded-md px-2.5 py-2 text-xs transition-colors sm:px-3",
                    variant === option.id ? "bg-primary font-medium text-primary-foreground shadow-sm" : "text-muted-foreground hover:bg-muted hover:text-foreground")}>
                  <OptionIcon className="hidden size-3.5 sm:block" />{locale === "zh" ? option.labelZh : option.labelEn}
                </button>
              })}
            </div>
          </div>
        ) : null}
        <div className="grid items-start gap-6 lg:grid-cols-[16rem_minmax(0,1fr)] lg:gap-8">
          <aside className={cn(sidebarClassName, "sticky top-24 hidden max-h-[calc(100dvh-13rem)] lg:flex")}>
            <div className="mx-4 flex shrink-0 items-center gap-3 border-b border-border py-5">
              <div className={cn("flex size-10 items-center justify-center rounded-xl bg-accent text-primary", variant === "editorial" && "rounded-none bg-transparent", variant === "studio" && "rounded-md border border-primary/20")}><Icon className="size-5" /></div>
              <div><p className={cn("text-sm font-semibold", variant === "editorial" && "font-heading text-lg")}>{variant === "library" ? t.knowledgeLibrary : variant === "editorial" ? t.editorialJournal : t.studioLog}</p>
                <p className="mt-1 text-[10px] tracking-widest text-muted-foreground">STARBLOG / {t.index}</p></div>
            </div>
            {sidebar()}
          </aside>
          <div className="min-w-0">
            <div className="mb-5 lg:hidden">
              <Dialog.Root open={drawerOpen} onOpenChange={setDrawerOpen}>
                <Dialog.Trigger asChild>
                  <Button variant="outline" className="h-11 max-w-full gap-2 bg-card px-3" aria-label={t.openCategories}><Menu className="size-4 shrink-0" /><span className="truncate">{selectedName}</span><ChevronRight className="size-3.5 shrink-0" /></Button>
                </Dialog.Trigger>
                <Dialog.Portal>
                  <Dialog.Overlay className="fixed inset-0 z-50 bg-black/40 backdrop-blur-sm" />
                  <Dialog.Content aria-describedby={undefined} className={cn(blogThemeClassName(variant), "fixed inset-y-0 left-0 z-50 flex w-80 max-w-[calc(100vw-2rem)] flex-col border-r border-border bg-card shadow-2xl")}>
                    <div className="flex shrink-0 items-center justify-between gap-3 border-b border-border px-5 py-4">
                      <Dialog.Title className="flex items-center gap-2 text-base font-semibold"><Icon className="size-5 text-primary" />{t.categories}</Dialog.Title>
                      <Dialog.Close asChild><Button variant="ghost" size="icon" aria-label={t.close}><X /></Button></Dialog.Close>
                    </div>
                    {sidebar(true)}
                  </Dialog.Content>
                </Dialog.Portal>
              </Dialog.Root>
            </div>
            <header className={cn("relative mb-7 overflow-hidden",
              variant === "library" && "py-3 sm:py-5",
              variant === "editorial" && "border-t-2 border-foreground pt-6 pb-2",
              variant === "studio" && "rounded-lg border border-border bg-linear-to-br from-card via-card to-accent/50 p-5 sm:p-7")}>
              <nav aria-label={t.categoryPath} className="relative z-10 mb-5 flex flex-wrap items-center gap-x-1.5 gap-y-2 text-xs text-muted-foreground">
                <BlogLink href={href({ categoryId: undefined })} className="inline-flex items-center gap-1.5 hover:text-primary"><FolderOpen className="size-3.5" />{t.posts}</BlogLink>
                {path.map((node) => <span key={node.id} className="inline-flex min-w-0 items-center gap-1.5"><ChevronRight className="size-3 shrink-0 opacity-50" /><BlogLink href={href({ categoryId: node.id })} aria-current={node.id === query.categoryId ? "page" : undefined} className="max-w-52 truncate hover:text-primary">{node.text}</BlogLink></span>)}
              </nav>
              <div className="relative z-10 flex items-start justify-between gap-5">
                <div className="min-w-0">
                  <p className={cn("mb-3 text-[10px] font-medium tracking-[0.2em] text-primary uppercase", variant === "studio" && "font-mono")}>{variant === "library" ? t.knowledgeLibrary : variant === "editorial" ? t.editorialJournal : t.studioLog}</p>
                  <h1 className={cn("text-3xl leading-tight font-semibold tracking-tight break-words sm:text-4xl", variant === "editorial" && "font-heading text-4xl sm:text-5xl", variant === "studio" && "sm:text-3xl")}>{title}</h1>
                  <p className="mt-4 max-w-lg text-sm leading-7 text-muted-foreground">{variant === "library" ? t.libraryIntro : variant === "editorial" ? t.editorialIntro : t.studioIntro}</p>
                </div>
                <span className={cn("shrink-0 rounded-full border border-border bg-card px-3 py-1.5 text-xs text-muted-foreground tabular-nums", variant === "editorial" && "rounded-none border-x-0", variant === "studio" && "rounded-md border-primary/20 bg-accent/50 font-mono")}>{t.postCount(posts.totalCount)}</span>
              </div>
              {variant === "studio" ? <div aria-hidden="true" className="pointer-events-none absolute -right-12 -bottom-28 size-72 rounded-full border border-primary/10 p-8"><div className="size-full rounded-full border border-primary/10 p-8"><div className="size-full rounded-full border border-primary/15" /></div></div> : null}
            </header>
            <ControlPanel />
            {filtered ? <div className="mt-4 flex flex-wrap items-center gap-2 text-xs">
              {query.categoryId ? <BlogLink href={href({ categoryId: undefined })} aria-label={`${t.removeCategory}: ${selectedName}`} className="inline-flex max-w-full items-center gap-2 rounded-full border border-primary/20 bg-accent/60 px-3 py-2 text-primary"><FolderOpen className="size-3 shrink-0" /><span className="truncate">{selectedName}</span><X className="size-3 shrink-0" /></BlogLink> : null}
              {query.search ? <BlogLink href={href({ search: undefined })} aria-label={`${t.removeSearch}: ${query.search}`} className="inline-flex max-w-full items-center gap-2 rounded-full border border-border bg-card px-3 py-2"><span className="truncate">“{query.search}”</span><X className="size-3 shrink-0" /></BlogLink> : null}
              <BlogLink href={href(clearPatch)} className="px-2 py-2 text-muted-foreground underline-offset-4 hover:text-primary hover:underline">{t.clearFilters}</BlogLink>
            </div> : null}
            <div role="status" aria-live="polite" className="mt-2 h-5 text-xs text-primary">{pending ? t.loadingPosts : <span className="sr-only">{t.postCount(posts.totalCount)}</span>}</div>
            <section aria-label={t.posts} aria-busy={pending} className={cn("transition-opacity motion-reduce:transition-none", pending && "opacity-50")}>
              {posts.items.length ? (
                <div className={cn(query.view === "grid" ? "grid grid-cols-1 gap-5 md:grid-cols-2" : "flex flex-col gap-3", variant === "editorial" && (query.view === "grid" ? "gap-x-9 gap-y-4" : "gap-0"))}>
                  {posts.items.map((post, index) => <BlogArticleCard key={post.id} post={post} index={index} />)}
                </div>
              ) : (
                <div className="flex min-h-80 flex-col items-center justify-center rounded-xl border border-dashed border-border px-6 py-14 text-center">
                  <div className="mb-5 flex size-14 items-center justify-center rounded-2xl bg-accent text-primary">{filtered ? <SearchX className="size-6" /> : <BookOpen className="size-6" />}</div>
                  <h2 className="text-lg font-semibold">{pageUnavailable ? t.pageUnavailable : filtered ? t.noMatchingPosts : t.noPosts}</h2>
                  {!filtered && !pageUnavailable ? <p className="mt-2 text-sm text-muted-foreground">{t.noPublishedPosts}</p> : null}
                  {filtered || pageUnavailable ? <Button variant="outline" className="mt-5" asChild><BlogLink href={href(pageUnavailable ? { page: 1 } : clearPatch)}>{pageUnavailable ? t.backToFirstPage : t.clearFilters}<MoveUpRight className="size-3.5" /></BlogLink></Button> : null}
                </div>
              )}
              <PaginationNav totalPages={posts.totalPages} totalCount={posts.totalCount} itemCount={posts.items.length} />
            </section>
          </div>
        </div>
      </div>
    </div>
  )
}
