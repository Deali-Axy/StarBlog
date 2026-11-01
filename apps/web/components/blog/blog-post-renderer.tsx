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
import styles from "./article-designs.module.css"

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

  return <div data-blog-article-detail data-article-layout={variant} className={styles.page}>
    <article lang={lang ?? "zh-CN"} className={styles.article}>
      <div className={styles.trail}>
        <Link href={routes.blog}><ArrowLeft size={14} aria-hidden="true" />{t.backToPosts}</Link>
        <span>{variant === "library" ? t.typeIndex : variant === "editorial" ? t.fieldNotes : t.archiveWorkbench}<span aria-hidden="true"> / </span>{t.readArticle}</span>
      </div>
      <header className={styles.hero}>
        <div className={styles.runningHead}><span>{t.articleNotebook}</span><time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time></div>
        <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className={styles.category}>{post.categoryName || t.posts}<ArrowUpRight size={12} aria-hidden="true" /></Link>
        <h1>{post.title}</h1>
        {post.summary ? <p className={styles.summary}>{post.summary}</p> : null}
        <div className={styles.byline}>
          <span className={styles.authorMark} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
          <span>{site.authorName}</span><span className={styles.metaDivider} aria-hidden="true">/</span>
          <time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time>
          <span className={styles.readTime}><Clock3 size={13} aria-hidden="true" />{t.readingTime(minutes)}</span>
          <button type="button" className={styles.share} onClick={share}><Share2 size={13} aria-hidden="true" />{t.share}</button>
        </div>
        {translations.length > 0 ? <nav aria-label={t.articleLanguage} className={styles.languages}>
          <span>{t.articleLanguage}</span>
          <Link href={`${postHref(post)}?lang=original`} aria-current={!lang ? "page" : undefined}>{t.original}</Link>
          {translations.map(item => <Link key={item.language} href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`}
            aria-current={lang === item.language ? "page" : undefined}>{languageName(item.language, locale)}</Link>)}
        </nav> : null}
        {translationMissing ? <p className={styles.translationNote}>{t.translationMissing}</p> : null}
      </header>
      {headings.length > 0 ? <aside className={styles.tocPane}><div className={styles.sticky}><SidebarTOC items={headings} /></div></aside> : null}
      {headings.length > 0 ? <SidebarTOC items={headings} mobile /> : null}
      <div className={styles.readingColumn}>
        <div data-article-body className={styles.body}>{children}</div>
        <div className={styles.endMark} aria-hidden="true">{variant === "editorial" ? "✳" : variant === "studio" ? "— END OF DOCUMENT —" : "■"}</div>
        <div className={styles.afterword}>
          <PostCopyright title={post.title} permalink={permalink} />
          <RelatedPosts posts={related} />
          <CommentSection postId={post.id} />
        </div>
      </div>
      <aside className={styles.recordPane} aria-label={t.articleDetails}>
        <div className={styles.sticky}>
          <div className={styles.recordHeading}><FileText size={14} aria-hidden="true" />{t.articleDetails}</div>
          <dl>
            <div><dt>{t.author.replace(/[:：]\s*$/, "")}</dt><dd>{site.authorName}</dd></div>
            <div><dt>{t.created}</dt><dd><time dateTime={post.creationTime}>{formatDate(post.creationTime)}</time></dd></div>
            <div><dt>{t.updated}</dt><dd><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
            <div><dt>{t.categories}</dt><dd>{post.categoryName || t.posts}</dd></div>
            <div><dt>{t.previewReadingTime}</dt><dd>{t.readingTime(minutes)}</dd></div>
          </dl>
          <a className={styles.recordLink} href={permalink}>{t.permalink.replace(/[:：]\s*$/, "")}<ArrowUpRight size={12} aria-hidden="true" /></a>
        </div>
      </aside>
    </article>
  </div>
}
