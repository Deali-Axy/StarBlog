"use client"

import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useEffect, useRef, useState, type KeyboardEvent } from "react"
import { Dialog } from "radix-ui"
import { Archive, ArrowUpRight, FileText, X } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { BlogBreadcrumb, BlogDirectory, BlogDirectoryFooter, BlogFilters, BlogResults, type BlogDesignProps } from "@/components/blog/blog-design-shared"
import { CategoryTree } from "@/components/blog/category-tree"
import { ControlPanel } from "@/components/blog/control-panel"
import type { PostResponse } from "@/lib/api/types"
import { formatDate, readingMinutes } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { blogThemeClassName } from "@/lib/blog-theme"
import { cn } from "@/lib/utils"
import styles from "./blog-designs.module.css"

function ArticlePreview({ post, modal = false }: { post: PostResponse; modal?: boolean }) {
  const { t } = useI18n()
  const heading = <h2 className={styles.previewTitle}>{post.title}</h2>
  return <div className={styles.previewDocument}>
    <div className={styles.previewTop}><span>{t.articlePreview}</span><FileText className="size-4" aria-hidden="true" /></div>
    <span className={styles.previewCategory}>{post.categoryName || t.posts}</span>
    {modal ? <Dialog.Title asChild>{heading}</Dialog.Title> : heading}
    <dl className={styles.previewMetadata}>
      <div><dt>{t.updated}</dt><dd><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
      <div><dt>{t.previewReadingTime}</dt><dd>{t.readingTime(readingMinutes(post.content ?? post.summary))}</dd></div>
    </dl>
    <p className={styles.previewSummary}>{post.summary || t.noArticleSummary}</p>
    <Link href={postHref(post)} className={styles.previewRead}>{t.readArticle}<ArrowUpRight className="size-4" aria-hidden="true" /></Link>
    <p className={styles.previewHint}>{t.archivePreviewHint}</p>
  </div>
}

export function ArchiveDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query, variant } = useBlogNavigation()
  const { posts, title, path, categories, treeState } = props
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [previewOpen, setPreviewOpen] = useState(false)
  const rowRefs = useRef(new Map<string, HTMLButtonElement>())
  const previewTrigger = useRef<HTMLButtonElement | null>(null)
  const selected = posts.items.find((post) => post.id === selectedId) ?? posts.items[0]

  useEffect(() => {
    const desktop = window.matchMedia("(min-width: 1024px)")
    const close = () => { if (desktop.matches) setPreviewOpen(false) }
    desktop.addEventListener("change", close)
    return () => desktop.removeEventListener("change", close)
  }, [])

  function select(post: PostResponse, trigger: HTMLButtonElement) {
    setSelectedId(post.id)
    previewTrigger.current = trigger
    if (window.matchMedia("(max-width: 1023px)").matches) setPreviewOpen(true)
  }

  function keyboardSelect(event: KeyboardEvent<HTMLButtonElement>, index: number) {
    if (event.key !== "ArrowUp" && event.key !== "ArrowDown" && event.key !== "Home" && event.key !== "End") return
    event.preventDefault()
    const next = event.key === "Home" ? 0 : event.key === "End" ? posts.items.length - 1 : Math.max(0, Math.min(posts.items.length - 1, index + (event.key === "ArrowDown" ? 1 : -1)))
    const post = posts.items[next]
    setSelectedId(post.id)
    rowRefs.current.get(post.id)?.focus()
  }

  return <div data-blog-layout="archive">
    <header className={styles.archiveMasthead}>
      <div className={styles.archiveTitle}><span className={styles.archiveGlyph}><Archive className="size-5" aria-hidden="true" /></span><div>
        <h1>{query.categoryId ? title : t.archiveWorkbench}</h1><small>{t.archiveEyebrow}</small>
      </div></div>
      <div className={styles.archiveCount}><strong>{posts.totalCount}</strong>{t.archiveCountLabel}</div>
      <BlogDirectory {...props} mobileOnly />
    </header>
    <div className={styles.archiveWorkbench}>
      <aside className={styles.archiveDirectory}>
        <div className={styles.archiveDirectoryLabel}><span>{t.topicDirectory}</span><span aria-hidden="true">⌘</span></div>
        <CategoryTree categoryNodes={categories} state={treeState} className={styles.directoryTree} />
        <BlogDirectoryFooter />
      </aside>
      <div className={styles.archiveCenter}>
        <div className={styles.archiveCenterHeader}><BlogBreadcrumb path={path} /><ControlPanel /></div>
        <BlogFilters title={title} />
        <BlogResults posts={posts}>
          <div className={cn(query.view === "list" && styles.archiveCompact)}>
            {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} data-selected={selected?.id === post.id} className={styles.archiveRow}>
              <span className={styles.archiveRowNumber}>{String((query.page - 1) * query.pageSize + index + 1).padStart(3, "0")}</span>
              <div><h2 className={styles.articleTitle}><button type="button" ref={(element) => { if (element) rowRefs.current.set(post.id, element); else rowRefs.current.delete(post.id) }}
                aria-label={`${t.previewArticle}: ${post.title}`} aria-pressed={selected?.id === post.id} aria-controls="blog-archive-preview"
                onClick={(event) => select(post, event.currentTarget)} onKeyDown={(event) => keyboardSelect(event, index)}>
                {post.title}
              </button></h2>
                  <div className={styles.archiveRowMeta}><span>{post.categoryName || t.posts}</span><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></div>
                  {post.summary ? <p>{post.summary}</p> : null}
              </div>
              <Link href={postHref(post)} className={styles.archiveRowLink} aria-label={`${t.readArticle}: ${post.title}`} title={t.readArticle}><ArrowUpRight className="size-4" /></Link>
            </article>)}
          </div>
        </BlogResults>
      </div>
      <aside id="blog-archive-preview" aria-label={t.articlePreview} className={styles.archivePreview}>
        {selected ? <ArticlePreview post={selected} /> : <div className={styles.previewDocument}><p className={styles.previewHint}>{t.previewEmpty}</p></div>}
      </aside>
    </div>
    <Dialog.Root open={previewOpen && Boolean(selected)} onOpenChange={setPreviewOpen}>
      <Dialog.Portal>
        <Dialog.Overlay className={styles.directoryOverlay} />
        <Dialog.Content aria-describedby={undefined} className={cn(blogThemeClassName(variant), styles.previewDialog)}
          onCloseAutoFocus={(event) => { event.preventDefault(); previewTrigger.current?.focus() }}>
          <div className={styles.previewDialogClose}><Dialog.Close asChild><button type="button" className={styles.closeButton} aria-label={t.close}><X className="size-5" /></button></Dialog.Close></div>
          {selected ? <ArticlePreview post={selected} modal /> : null}
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  </div>
}
