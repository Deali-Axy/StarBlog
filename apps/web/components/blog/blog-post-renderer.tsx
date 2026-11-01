"use client"

import type { ReactNode } from "react"
import { ArrowLeft, ArrowUpRight, Clock3, FileText, Share2 } from "lucide-react"
import { toast } from "sonner"

import { BlogStyleLink as Link, useBlogLayout } from "@/components/blog/blog-layout-context"
import { SidebarTOC } from "@/components/blog/sidebar-toc"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { useI18n } from "@/components/app-providers"
import type { PostResponse, PostTranslationResponse } from "@/lib/api/types"
import type { ArticleHeading } from "@/lib/markdown"
import { postHref, routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { formatDate } from "@/lib/format"
import { languageName } from "@/lib/i18n"
import { cn } from "@/lib/utils"
import { articleDesigns, articleStickyClassName } from "./article-designs"

type ArticlePost = Omit<PostResponse, "content" | "toc">

export function BlogPostRenderer({ post, headings, children, minutes, related = [], permalink, translations = [], lang, translationMissing = false }: {
  post: ArticlePost
  headings: ArticleHeading[]
  children: ReactNode
  minutes: number
  related?: PostResponse[]
  permalink: string
  translations?: Pick<PostTranslationResponse, "language">[]
  lang?: string
  translationMissing?: boolean
}) {
  const { locale, t } = useI18n()
  const { variant } = useBlogLayout()
  const site = getSite(locale)
  const design = articleDesigns[variant]

  async function share() {
    const url = window.location.href
    try {
      if (navigator.share) await navigator.share({ title: post.title, url })
      else {
        await navigator.clipboard.writeText(url)
        toast.success(t.linkCopied)
      }
    } catch (error) {
      if (!(error instanceof DOMException && error.name === "AbortError")) toast.error(t.shareFailed)
    }
  }

  return <div className={cn("px-4 sm:px-6", design.gutter)}>
    <div data-blog-article-detail data-article-layout={variant} className={cn("mx-auto w-full pb-12 sm:pb-20", design.page)}>
      <article lang={lang ?? "zh-CN"} className={cn("grid grid-cols-1 lg:grid-cols-12", design.grid)}>
        <div className={cn("col-span-full row-start-1 flex min-h-14 items-center justify-between gap-5 text-xs text-muted-foreground sm:min-h-18", design.trail)}>
          <Link href={routes.blog} className="inline-flex shrink-0 items-center gap-2 hover:text-primary"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>
          <span className="truncate font-mono">{variant === "library" ? t.typeIndex : variant === "editorial" ? t.fieldNotes : t.archiveWorkbench}<span aria-hidden="true"> / </span>{t.readArticle}</span>
        </div>
        <header className={cn("row-start-2 min-w-0", design.hero)}>
          <div className={cn("flex items-center justify-between gap-3 font-mono text-xs tracking-wider text-muted-foreground sm:gap-6", design.runningHead)}><span>{t.articleNotebook}</span><time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time></div>
          <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className={cn("inline-flex items-center gap-4 text-xs text-primary hover:underline hover:underline-offset-4", design.category)}>{post.categoryName || t.posts}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></Link>
          <h1 className={cn("wrap-anywhere text-pretty font-bold leading-snug tracking-tight", design.title)}>{post.title}</h1>
          {post.summary ? <p className={cn("mt-5 wrap-anywhere leading-loose text-muted-foreground sm:mt-6", design.summary)}>{post.summary}</p> : null}
          <div className={cn("mt-6 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-muted-foreground", design.byline)}>
            <span className={cn("inline-grid size-6 place-items-center bg-primary font-mono text-xs font-semibold text-primary-foreground sm:size-7", design.authorMark)} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
            <span className="text-foreground">{site.authorName}</span><span className={cn("hidden sm:inline", design.metaDivider)} aria-hidden="true">/</span>
            <time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time>
            <span className="inline-flex items-center gap-2"><Clock3 className="size-3.5 shrink-0" aria-hidden="true" />{t.readingTime(minutes)}</span>
            <button type="button" className={cn("ml-auto inline-flex cursor-pointer items-center gap-2 py-2 text-foreground hover:text-primary", design.share)} onClick={share}><Share2 className="size-3.5 shrink-0" aria-hidden="true" />{t.share}</button>
          </div>
          {translations.length > 0 ? <nav aria-label={t.articleLanguage} className={cn("mt-5 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs", design.languages)}>
            <span className="text-muted-foreground">{t.articleLanguage}</span>
            <Link href={`${postHref(post)}?lang=original`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={!lang ? "page" : undefined}>{t.original}</Link>
            {translations.map(item => <Link key={item.language} href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`}
              className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={lang === item.language ? "page" : undefined}>{languageName(item.language, locale)}</Link>)}
          </nav> : null}
          {translationMissing ? <p className="mt-4 text-sm text-muted-foreground">{t.translationMissing}</p> : null}
        </header>
        {headings.length > 0 ? <aside className={cn("hidden min-w-0 lg:block", design.tocPane)}><div className={articleStickyClassName}><SidebarTOC items={headings} /></div></aside> : null}
        {headings.length > 0 ? <SidebarTOC items={headings} mobile /> : null}
        <div className={cn("col-span-full row-start-4 min-w-0 pt-8 wrap-anywhere lg:row-start-3", design.readingColumn, variant === "editorial" && headings.length > 0 && "border-t-0 lg:border-t")}>
          <div data-article-body className={cn("min-w-0 text-sm sm:text-base", design.body)}>{children}</div>
          <div className={cn("mt-12 text-lg text-primary", design.endMark)} aria-hidden="true">{variant === "editorial" ? "✳" : variant === "studio" ? "— END OF DOCUMENT —" : "■"}</div>
          <div className="min-w-0 [&>section:first-child]:mt-8">
            <PostCopyright title={post.title} permalink={permalink} />
            <RelatedPosts posts={related} />
            <CommentSection postId={post.id} />
          </div>
        </div>
        <aside className={cn("hidden min-w-0 text-xs text-muted-foreground", design.recordPane)} aria-label={t.articleDetails}>
          <div className={articleStickyClassName}>
            <div className="flex items-center gap-2 border-b border-border py-4 text-xs text-foreground"><FileText className="size-3.5 shrink-0" aria-hidden="true" />{t.articleDetails}</div>
            <dl className="flex flex-col gap-6 pt-6">
              <div><dt className="mb-2">{t.author.replace(/[:：]\s*$/, "")}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{site.authorName}</dd></div>
              <div><dt className="mb-2">{t.created}</dt><dd className="wrap-anywhere leading-relaxed text-foreground"><time dateTime={post.creationTime}>{formatDate(post.creationTime)}</time></dd></div>
              <div><dt className="mb-2">{t.updated}</dt><dd className="wrap-anywhere leading-relaxed text-foreground"><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
              <div><dt className="mb-2">{t.categories}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{post.categoryName || t.posts}</dd></div>
              <div><dt className="mb-2">{t.previewReadingTime}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{t.readingTime(minutes)}</dd></div>
            </dl>
            <a className="mt-7 flex items-center justify-between gap-3 border-t border-border py-4 text-primary hover:underline hover:underline-offset-4" href={permalink}>{t.permalink.replace(/[:：]\s*$/, "")}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></a>
          </div>
        </aside>
      </article>
    </div>
  </div>
}
