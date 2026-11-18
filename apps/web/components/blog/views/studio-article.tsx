"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { formatDate } from "@/lib/format"
import { languageName } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { ArrowLeft, ArrowUpRight, Clock3, FileText } from "lucide-react"
import { ArticleInteractions } from "../article-interactions"
import type { ArticleLayoutProps } from "../article-props"
import { ArchiveDesktopTOC } from "./studio-desktop-toc"
import { ArchiveMobileTOC } from "./studio-mobile-toc"
const articleStickyClassName = "sticky top-[calc(var(--blog-header-height)+1.5rem)]"
export function ArchiveArticle({ post, headings, children, minutes, related = [], permalink, translations = [], lang, translationMissing = false }: ArticleLayoutProps) {
    const { locale, t } = useI18n()
    const site = getSite(locale)
    return <div className="w-full px-4 sm:px-6 lg:px-0">
        <div data-blog-article-detail data-article-layout="studio" className="w-full pb-12">
            <article lang={lang ?? "zh-CN"} className={"grid grid-cols-1 lg:grid-cols-12 lg:gap-x-6"}>
                <header className={"row-start-1 min-w-0 col-span-full border border-border bg-card px-5 py-6 sm:px-8 sm:py-8 lg:col-span-9 lg:col-start-4 lg:border-b-0 xl:col-span-8 xl:col-start-3 xl:px-10"}>
                    <Link href={routes.blog} className="mb-4 flex w-fit min-h-8 items-center gap-2 text-xs text-muted-foreground hover:text-primary lg:hidden"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>
                    <h1 className={"wrap-anywhere text-pretty tracking-tight text-3xl font-semibold leading-snug sm:text-4xl"}>{post.title}</h1>
                    {post.summary ? <p className={"mt-5 wrap-anywhere text-muted-foreground sm:mt-6 border-l-2 border-border ps-4 text-sm"}>{post.summary}</p> : null}
                    <div className={"mt-6 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-muted-foreground"}>
                        <span className={"size-6 place-items-center bg-primary font-mono text-xs font-semibold text-primary-foreground sm:size-7 hidden"} aria-hidden="true">{site.authorName.slice(0, 1).toUpperCase()}</span>
                        <span className="text-foreground">{site.authorName}</span><span className={"hidden sm:inline"} aria-hidden="true">/</span>
                        <Link href={`${routes.blog}?categoryId=${post.categoryId}`} className="inline-flex items-center gap-1.5 font-mono text-primary hover:underline hover:underline-offset-4">{post.categoryName || t.posts}<ArrowUpRight className="size-3 shrink-0" aria-hidden="true" /></Link>
                        <time dateTime={post.creationTime}>{formatDate(post.creationTime, "YYYY.MM.DD")}</time>
                        <span className="inline-flex items-center gap-2"><Clock3 className="size-3.5 shrink-0" aria-hidden="true" />{t.readingTime(minutes)}</span>
                    </div>
                    {translations.length > 0 ? <nav aria-label={t.articleLanguage} className={"mt-5 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs"}>
                        <span className="text-muted-foreground">{t.articleLanguage}</span>
                        <Link href={`${postHref(post)}?lang=original`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={!lang ? "page" : undefined}>{t.original}</Link>
                        {translations.map(item => <Link key={item.language} href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`} className="border-b border-transparent py-1 aria-[current=page]:border-current aria-[current=page]:text-primary" aria-current={lang === item.language ? "page" : undefined}>{languageName(item.language, locale)}</Link>)}
                    </nav> : null}
                    {translationMissing ? <p className="mt-4 text-sm text-muted-foreground">{t.translationMissing}</p> : null}
                </header>
                <aside className={"hidden min-w-0 lg:block lg:col-span-3 lg:col-start-1 lg:row-start-1 lg:row-span-2 lg:pl-6 xl:col-span-2"}><div className={articleStickyClassName}><Link href={routes.blog} className="flex min-h-11 items-center gap-2 border border-border bg-muted px-3 py-3 text-xs text-muted-foreground transition-colors hover:bg-accent hover:text-primary"><ArrowLeft className="size-3.5 shrink-0" aria-hidden="true" />{t.backToPosts}</Link>{headings.length > 0 ? <ArchiveDesktopTOC /> : null}<ArticleInteractions title={post.title} className="mt-4 xl:hidden" /></div></aside>
                {headings.length > 0 ? <ArchiveMobileTOC /> : null}
                <div className={"col-span-full row-start-3 min-w-0 wrap-anywhere lg:row-start-2 border border-t-0 border-border bg-card px-5 pt-7 pb-7 sm:px-8 sm:pb-8 xl:col-span-8 xl:col-start-3 xl:px-10 xl:pt-3 xl:pb-10 lg:col-span-9 lg:col-start-4"}>
                    <div data-article-body className={"min-w-0 text-sm sm:text-sm lg:border-t lg:border-border lg:pt-8 [&_:is(h1,h2,h3)]:font-sans"}>{children}</div>
                    <div className={"mt-12 text-center font-mono text-xs tracking-widest text-muted-foreground"} aria-hidden="true">— END OF DOCUMENT —</div>
                    <div className="min-w-0">
                        <PostCopyright title={post.title} permalink={permalink} appearance="studio">
                            <ArticleInteractions title={post.title} end />
                        </PostCopyright>
                        <CommentSection />
                        <RelatedPosts posts={related} />
                    </div>
                </div>
                <aside className={"hidden min-w-0 text-xs text-muted-foreground xl:col-span-2 xl:col-start-11 xl:row-start-1 xl:row-span-2 xl:block xl:pr-6"} aria-label={t.articleDetails}>
                    <div className={articleStickyClassName}>

                        <div className="flex items-center gap-2 border-b border-border py-4 text-xs text-foreground">
                            <FileText className="size-3.5 shrink-0" aria-hidden="true" />
                            {t.articleDetails}
                        </div>

                        <dl className="flex flex-col gap-6 pt-6">
                            {post.summary?.trim() ? <div><dt className="mb-2">{t.articleSummary}</dt><dd className="wrap-anywhere whitespace-pre-line leading-relaxed text-foreground">{post.summary}</dd></div> : null}
                            <div><dt className="mb-2">{t.author.replace(/[:：]\s*$/, "")}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{site.authorName}</dd></div>
                            <div><dt className="mb-2">{t.created}</dt><dd className="wrap-anywhere leading-relaxed text-foreground"><time dateTime={post.creationTime}>{formatDate(post.creationTime)}</time></dd></div>
                            <div><dt className="mb-2">{t.updated}</dt><dd className="wrap-anywhere leading-relaxed text-foreground"><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></dd></div>
                            <div><dt className="mb-2">{t.categories}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{post.categoryName || t.posts}</dd></div>
                            <div><dt className="mb-2">{t.previewReadingTime}</dt><dd className="wrap-anywhere leading-relaxed text-foreground">{t.readingTime(minutes)}</dd></div>
                        </dl>

                        <a className="mt-7 flex items-center justify-between gap-3 border-t border-border py-4 text-primary hover:underline hover:underline-offset-4"
                            href={permalink}>
                            {t.permalink.replace(/[:：]\s*$/, "")}
                            <ArrowUpRight className="size-3 shrink-0" aria-hidden="true" />
                        </a>

                        <ArticleInteractions title={post.title} />
                    </div>
                </aside>
            </article>
        </div>
    </div>
}
