"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { formatDate } from "@/lib/format"
import { languageName } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { ArrowLeft, ArrowUpRight, Clock3, Share2 } from "lucide-react"
import { useArticleActions } from "../article-actions"
import type { ArticleLayoutProps } from "../article-props"
import { IndexDesktopTOC } from "./library-desktop-toc"
import { IndexMobileTOC } from "./library-mobile-toc"
const articleStickyClassName = "sticky top-[calc(var(--blog-header-height)+1.5rem)]"
export function IndexArticle({ post, headings, children, minutes, related = [], permalink, translations = [], lang, translationMissing = false }: ArticleLayoutProps) {
  const { locale, t } = useI18n()
  const site = getSite(locale)
  const { share } = useArticleActions()
  return <div className={"px-4 sm:px-6 lg:px-8"}>
    <div data-blog-article-detail data-article-layout="library" className={"mx-auto w-full pb-12 sm:pb-20 max-w-7xl"}>
      <article lang={lang ?? "zh-CN"} className={"grid grid-cols-1 lg:grid-cols-12 lg:gap-x-12 xl:gap-x-16"}>
        <div className={"col-span-full row-start-1 flex min-h-14 items-center justify-between gap-5 text-xs text-muted-foreground sm:min-h-18"}>
          <Link href={routes.blog} className="inline-flex shrink-0 items-center gap-2 hover:text-primary"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>
          <span className="truncate font-mono">{t.typeIndex}<span aria-hidden="true"> / </span>{t.readArticle}</span>
        </div>
        <header className={"row-start-2 min-w-0 col-span-full border-b-2 border-foreground pt-3 pb-7 sm:pt-6 sm:pb-10"}>
          <div className={"flex items-center justify-between gap-3 font-mono text-xs tracking-wider text-muted-foreground sm:gap-6 border-b border-border pb-5"}><span>{t.articleNotebook}</span><time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time></div>
          <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className={"inline-flex items-center gap-4 text-xs hover:underline hover:underline-offset-4 mt-7 bg-highlight px-2 py-1 text-lime-950 sm:mt-9"}>{post.categoryName || t.posts}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></Link>
          <h1 className={"wrap-anywhere text-pretty font-bold tracking-tight mt-5 text-4xl sm:mt-6 sm:text-5xl xl:text-6xl"}>{post.title}</h1>
          {post.summary ? <p className={"mt-5 wrap-anywhere text-muted-foreground sm:mt-6 max-w-3xl text-sm sm:text-lg"}>{post.summary}</p> : null}
          <div className={"mt-6 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-muted-foreground sm:mt-9"}>
            <span className={"inline-grid size-6 place-items-center bg-primary font-mono text-xs font-semibold text-primary-foreground sm:size-7"} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
            <span className="text-foreground">{site.authorName}</span><span className={"hidden sm:inline"} aria-hidden="true">/</span>
            <time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time>
            <span className="inline-flex items-center gap-2"><Clock3 className="size-3.5 shrink-0" aria-hidden="true" />{t.readingTime(minutes)}</span>
            <button type="button" className={"ml-auto inline-flex cursor-pointer items-center gap-2 py-2 text-foreground hover:text-primary"} onClick={() => void share(post.title)}><Share2 className="size-3.5 shrink-0" aria-hidden="true" />{t.share}</button>
          </div>
          {translations.length > 0 ? <nav aria-label={t.articleLanguage} className={"mt-5 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs"}>
            <span className="text-muted-foreground">{t.articleLanguage}</span>
            <Link href={`${postHref(post)}?lang=original`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={!lang ? "page" : undefined}>{t.original}</Link>
            {translations.map(item => <Link key={item.language} href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={lang === item.language ? "page" : undefined}>{languageName(item.language, locale)}</Link>)}
          </nav> : null}
          {translationMissing ? <p className="mt-4 text-sm text-muted-foreground">{t.translationMissing}</p> : null}
        </header>
        {headings.length > 0 ? <aside className={"hidden min-w-0 lg:block pt-12 lg:col-span-3 lg:col-start-1 lg:row-start-3"}><div className={articleStickyClassName}><IndexDesktopTOC /></div></aside> : null}
        {headings.length > 0 ? <IndexMobileTOC /> : null}
        <div className={"col-span-full row-start-4 min-w-0 pt-8 wrap-anywhere lg:row-start-3 lg:col-span-8 lg:col-start-5 lg:pt-12"}>
          <div data-article-body className={"min-w-0 text-sm sm:text-base [&_h2]:border-b [&_h2]:border-border [&_h2]:pb-2"}>{children}</div>
          <div className={"mt-12 text-lg text-primary"} aria-hidden="true">■</div>
          <div className="min-w-0 [&>section:first-child]:mt-8">
            <PostCopyright title={post.title} permalink={permalink} />
            <RelatedPosts posts={related} />
            <CommentSection />
          </div>
        </div>

      </article>
    </div>
  </div>
}
