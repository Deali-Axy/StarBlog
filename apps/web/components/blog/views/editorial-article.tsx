"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { formatDate } from "@/lib/format"
import { languageName } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { cn } from "@/lib/utils"
import { ArrowLeft, ArrowUpRight, Clock3, Share2 } from "lucide-react"
import { useArticleActions } from "../article-actions"
import type { ArticleLayoutProps } from "../article-props"
import { FolioDesktopTOC } from "./editorial-desktop-toc"
import { FolioMobileTOC } from "./editorial-mobile-toc"
const articleStickyClassName = "sticky top-[calc(var(--blog-header-height)+1.5rem)]"
export function FolioArticle({ post, headings, children, minutes, related = [], permalink, translations = [], lang, translationMissing = false }: ArticleLayoutProps) {
  const { locale, t } = useI18n()
  const site = getSite(locale)
  const { share } = useArticleActions()
  return <div className={"px-4 sm:px-6 lg:px-12"}>
    <div data-blog-article-detail data-article-layout="editorial" className={"mx-auto w-full pb-12 sm:pb-20 max-w-blog-folio"}>
      <article lang={lang ?? "zh-CN"} className={"grid grid-cols-1 lg:grid-cols-12 lg:gap-x-12"}>
        <div className={"col-span-full row-start-1 flex items-center justify-between gap-5 text-xs text-muted-foreground min-h-14 border-b border-border sm:min-h-16"}>
          <Link href={routes.blog} className="inline-flex shrink-0 items-center gap-2 hover:text-primary"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>
          <span className="truncate font-mono">{t.fieldNotes}<span aria-hidden="true"> / </span>{t.readArticle}</span>
        </div>
        <header className={"row-start-2 min-w-0 col-span-full py-6 sm:py-8 lg:col-span-9"}>
          <div className={"items-center justify-between gap-3 font-mono text-xs tracking-wider text-muted-foreground sm:gap-6 hidden"}><span>{t.articleNotebook}</span><time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time></div>
          <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className={"inline-flex items-center text-xs text-primary hover:underline hover:underline-offset-4 mt-0 gap-3 before:h-px before:w-6 before:bg-current before:content-['']"}>{post.categoryName || t.posts}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></Link>
          <h1 className={"wrap-anywhere text-pretty tracking-tight mt-4 font-heading text-3xl font-semibold leading-snug sm:text-4xl"}>{post.title}</h1>
          {post.summary ? <p className={"wrap-anywhere text-muted-foreground mt-4 max-w-none text-sm sm:mt-4 sm:text-base"}>{post.summary}</p> : null}
          <div className={"flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-muted-foreground mt-5 border-t border-border pt-4 sm:mt-6"}>
            <span className={"size-6 place-items-center bg-primary font-mono text-xs font-semibold text-primary-foreground sm:size-7 hidden"} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
            <span className="text-foreground">{site.authorName}</span><span className={"hidden sm:hidden"} aria-hidden="true">/</span>
            <time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time>
            <span className="inline-flex items-center gap-2"><Clock3 className="size-3.5 shrink-0" aria-hidden="true" />{t.readingTime(minutes)}</span>
            <button type="button" className={"ml-auto inline-flex cursor-pointer items-center gap-2 hover:text-primary py-1 text-muted-foreground"} onClick={() => void share(post.title)}><Share2 className="size-3.5 shrink-0" aria-hidden="true" />{t.share}</button>
          </div>
          {translations.length > 0 ? <nav aria-label={t.articleLanguage} className={"flex flex-wrap items-center gap-x-4 gap-y-2 text-xs mt-3"}>
            <span className="text-muted-foreground">{t.articleLanguage}</span>
            <Link href={`${postHref(post)}?lang=original`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={!lang ? "page" : undefined}>{t.original}</Link>
            {translations.map(item => <Link key={item.language} href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={lang === item.language ? "page" : undefined}>{languageName(item.language, locale)}</Link>)}
          </nav> : null}
          {translationMissing ? <p className="mt-4 text-sm text-muted-foreground">{t.translationMissing}</p> : null}
        </header>
        {headings.length > 0 ? <aside className={"hidden min-w-0 lg:block border-l border-border pt-8 pl-6 lg:col-span-3 lg:col-start-10 lg:row-start-2 lg:row-span-2"}><div className={articleStickyClassName}><FolioDesktopTOC /></div></aside> : null}
        {headings.length > 0 ? <FolioMobileTOC /> : null}
        <div className={cn("col-span-full row-start-4 min-w-0 pt-8 wrap-anywhere lg:row-start-3", "border-t border-border lg:col-span-9 lg:col-start-1", headings.length > 0 && "border-t-0 lg:border-t")}>
          <div data-article-body className={"min-w-0 sm:text-base text-base [&_:is(h1,h2,h3)]:font-heading [&_h2]:text-2xl [&_blockquote]:border-l [&_blockquote]:ps-5"}>{children}</div>
          <div className={"mt-12 text-primary text-2xl"} aria-hidden="true">✳</div>
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
