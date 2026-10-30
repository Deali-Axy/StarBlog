"use client"

import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useState, type ReactNode } from "react"
import { Dialog } from "radix-ui"
import { ArrowDownRight, ArrowUpRight, ChevronRight, Rss, Shuffle, X } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { CategoryTree, type CategoryTreeState } from "@/components/blog/category-tree"
import { PaginationNav } from "@/components/blog/pagination-nav"
import type { CategoryNodeResponse, PageResult, PostResponse } from "@/lib/api/types"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import styles from "./blog-designs.module.css"

export interface BlogDesignProps {
  categories: CategoryNodeResponse[]
  posts: PageResult<PostResponse>
  path: CategoryNodeResponse[]
  title: string
  treeState: CategoryTreeState
}

export function BlogBreadcrumb({ path }: { path: CategoryNodeResponse[] }) {
  const { t } = useI18n()
  const { href, query } = useBlogNavigation()
  return <nav aria-label={t.categoryPath} className={styles.breadcrumb}>
    <BlogLink href={href({ categoryId: undefined })}>{t.posts}</BlogLink>
    {path.map((node) => <span key={node.id} className="inline-flex min-w-0 items-center gap-1">
      <ChevronRight className="size-3 shrink-0" />
      <BlogLink href={href({ categoryId: node.id })} aria-current={query.categoryId === node.id ? "page" : undefined}
        className="max-w-36 truncate">{node.text}</BlogLink>
    </span>)}
  </nav>
}

export function BlogDirectoryFooter() {
  const { t } = useI18n()
  return <div className={styles.directoryFooter}>
    <Link href={routes.blogRandom}><Shuffle className="size-3" />{t.randomPost}</Link>
    <a href={routes.feed}><Rss className="size-3" />{t.subscribe}</a>
  </div>
}

export function BlogDirectory({ categories, treeState, title, mobileOnly = false }: Pick<BlogDesignProps, "categories" | "treeState" | "title"> & { mobileOnly?: boolean }) {
  const { t } = useI18n()
  const { variant } = useBlogNavigation()
  const [open, setOpen] = useState(false)
  return <Dialog.Root open={open} onOpenChange={setOpen}>
    <Dialog.Trigger asChild>
      <button type="button" aria-label={t.openCategories} className={cn(styles.directoryButton, mobileOnly && styles.mobileDirectory)}>
        <span>{t.topicDirectory} / {title}</span><ArrowDownRight className="size-4 shrink-0" />
      </button>
    </Dialog.Trigger>
    <Dialog.Portal>
      <Dialog.Overlay className={styles.directoryOverlay} />
      <Dialog.Content aria-describedby={undefined} data-directory-layout={variant === "library" ? "wide" : variant === "editorial" ? "sheet" : "drawer"}
        className={cn(blogThemeClassName(variant), styles.directoryDialog)}>
        <div className={styles.directoryHeading}>
          <Dialog.Title>{t.topicDirectory}</Dialog.Title>
          <Dialog.Close asChild><button type="button" aria-label={t.close} className={styles.closeButton}><X className="size-5" /></button></Dialog.Close>
        </div>
        <CategoryTree categoryNodes={categories} state={treeState} onSelect={() => setOpen(false)} className={styles.directoryTree} />
        <BlogDirectoryFooter />
      </Dialog.Content>
    </Dialog.Portal>
  </Dialog.Root>
}

export function BlogFilters({ title }: { title: string }) {
  const { t } = useI18n()
  const { query, href } = useBlogNavigation()
  if (!query.categoryId && !query.search) return null
  return <div className={styles.filters}>
    {query.categoryId ? <BlogLink href={href({ categoryId: undefined })} aria-label={`${t.removeCategory}: ${title}`}><span>{title}</span><X className="size-3 shrink-0" /></BlogLink> : null}
    {query.search ? <BlogLink href={href({ search: undefined })} aria-label={`${t.removeSearch}: ${query.search}`}><span>“{query.search}”</span><X className="size-3 shrink-0" /></BlogLink> : null}
    <BlogLink href={href({ search: undefined, categoryId: undefined, page: 1 })}>{t.clearFilters}</BlogLink>
  </div>
}

export function BlogResults({ posts, children }: { posts: PageResult<PostResponse>; children: ReactNode }) {
  const { t } = useI18n()
  const { query, pending, href } = useBlogNavigation()
  const filtered = Boolean(query.categoryId || query.search)
  const unavailable = posts.totalCount > 0 && !posts.items.length && query.page > posts.totalPages
  return <>
    <div role="status" aria-live="polite" className={styles.status}>{pending ? t.loadingPosts : <span className="sr-only">{t.postCount(posts.totalCount)}</span>}</div>
    <section aria-label={t.posts} aria-busy={pending} className={styles.results}>
      <div className={styles.resultBody}>
        {posts.items.length ? children : <div className={styles.empty}>
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
