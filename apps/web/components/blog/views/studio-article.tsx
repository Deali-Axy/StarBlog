"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { formatDate } from "@/lib/format"
import { languageName } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { ArrowLeft, ArrowUpRight, Clock3, FileText, Share2 } from "lucide-react"
import { useArticleActions } from "../article-actions"
import type { ArticleLayoutProps } from "../article-props"
import { ArchiveDesktopTOC } from "./studio-desktop-toc"
import { ArchiveMobileTOC } from "./studio-mobile-toc"
const articleStickyClassName = "sticky top-[calc(var(--blog-header-height)+1.5rem)]"
export function ArchiveArticle({ post, headings, children, minutes, related = [], permalink, translations = [], lang, translationMissing = false }: ArticleLayoutProps) {
  const { locale, t } = useI18n()
  const site = getSite(locale)
  const { share } = useArticleActions()
  return <div className={"px-4 sm:px-6"}>
    <div data-blog-article-detail data-article-layout="studio" className={"mx-auto w-full pb-12 max-w-blog-studio sm:pb-12"}>
      <article lang={lang ?? "zh-CN"} className={"grid grid-cols-1 lg:grid-cols-12 lg:gap-x-6"}>
        <div className={"col-span-full row-start-1 flex items-center justify-between gap-5 text-xs text-muted-foreground min-h-14 sm:min-h-16"}>
          <Link href={routes.blog} className="inline-flex shrink-0 items-center gap-2 hover:text-primary"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>
          <span className="truncate font-mono">{t.archiveWorkbench}<span aria-hidden="true"> / </span>{t.readArticle}</span>
        </div>
        <header className={"row-start-2 min-w-0 col-span-full border border-border bg-card px-5 py-6 sm:px-8 sm:py-8 lg:col-span-9 lg:col-start-4 lg:border-b-0 xl:col-span-8 xl:col-start-3 xl:px-10"}>
          <div className={"flex items-center justify-between gap-3 font-mono text-xs text-muted-foreground sm:gap-6 tracking-wide [&>span]:before:mr-2 [&>span]:before:inline-block [&>span]:before:size-1.5 [&>span]:before:bg-primary [&>span]:before:align-middle [&>span]:before:content-['']"}><span>{t.articleNotebook}</span><time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time></div>
          <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className={"inline-flex items-center gap-4 text-xs text-primary hover:underline hover:underline-offset-4 mt-7 font-mono sm:mt-8"}>{post.categoryName || t.posts}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></Link>
          <h1 className={"wrap-anywhere text-pretty tracking-tight mt-4 text-3xl font-semibold leading-snug sm:text-4xl"}>{post.title}</h1>
          {post.summary ? <p className={"mt-5 wrap-anywhere text-muted-foreground sm:mt-6 border-l-2 border-border ps-4 text-sm"}>{post.summary}</p> : null}
          <div className={"mt-6 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-muted-foreground"}>
            <span className={"size-6 place-items-center bg-primary font-mono text-xs font-semibold text-primary-foreground sm:size-7 hidden"} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
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
        {headings.length > 0 ? <aside className={"hidden min-w-0 lg:block lg:col-span-3 lg:col-start-1 lg:row-start-2 lg:row-span-2 xl:col-span-2"}><div className={articleStickyClassName}><ArchiveDesktopTOC /></div></aside> : null}
        {headings.length > 0 ? <ArchiveMobileTOC /> : null}
        <div className={"col-span-full row-start-4 min-w-0 wrap-anywhere lg:row-start-3 border border-t-0 border-border bg-card px-5 pt-7 pb-7 sm:px-8 sm:pb-8 xl:col-span-8 xl:col-start-3 xl:px-10 xl:pt-3 xl:pb-10 lg:col-span-9 lg:col-start-4"}>
          <div data-article-body className={"min-w-0 text-sm sm:text-sm lg:border-t lg:border-border lg:pt-8 [&_:is(h1,h2,h3)]:font-sans"}>{children}</div>
          <div className={"mt-12 text-center font-mono text-xs tracking-widest text-muted-foreground"} aria-hidden="true">— END OF DOCUMENT —</div>
          <div className="min-w-0 [&>section:first-child]:mt-8">
            <PostCopyright title={post.title} permalink={permalink} />
            <RelatedPosts posts={related} />
            <CommentSection />
          </div>
        </div>
        <aside className={"hidden min-w-0 text-xs text-muted-foreground xl:col-span-2 xl:col-start-11 xl:row-start-2 xl:row-span-2 xl:block"} aria-label={t.articleDetails}>
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
