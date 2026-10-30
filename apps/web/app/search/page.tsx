import type { Metadata } from "next"
import Link from "next/link"
import { ArrowRight, ArrowUpRight, BookOpen, ChevronLeft, ChevronRight, Compass, Search, Shuffle } from "lucide-react"
import { StarMark } from "@/components/brand/star-mark"
import { SearchForm } from "@/components/search/search-form"
import { Button } from "@/components/ui/button"
import { getCategoryWordCloud, listPosts, searchPosts } from "@/lib/api/queries"
import { formatDate } from "@/lib/format"
import { getI18n } from "@/lib/i18n-server"
import { postHref, routes } from "@/lib/routes"
import { headingClassName, pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const pageSize = 12

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.search, alternates: { canonical: routes.search }, robots: { index: false, follow: true } }
}

// Render API snippets as text, allowing only the search engine's mark delimiters.
function HighlightedText({ text }: { text: string }) {
  return text.split(/(<mark>[\s\S]*?<\/mark>)/g).map((part, index) => (
    part.startsWith("<mark>") && part.endsWith("</mark>")
      ? <mark key={index} className="rounded-sm bg-primary/15 px-0.5 text-inherit underline decoration-primary/60 underline-offset-4">{part.slice(6, -7)}</mark>
      : part
  ))
}

function snippetText(text: string) {
  return text
    .replace(/!\[([^\]]*)\]\([^)]*\)/g, "$1")
    .replace(/\[([^\]]+)\]\([^)]*\)/g, "$1")
    .replace(/^\s{0,3}(?:#{1,6}|>|[-*+])\s+/gm, "")
    .replace(/`{1,3}/g, "")
    .replace(/\s+/g, " ")
    .trim()
}

function SearchIllustration() {
  return (
    <div aria-hidden="true" className="pointer-events-none relative mx-auto hidden size-60 items-center justify-center lg:flex">
      <div className="absolute inset-2 rounded-full border border-primary/15" />
      <div className="absolute inset-10 rounded-full border border-dashed border-primary/20" />
      <div className="absolute inset-x-0 top-1/2 border-t border-primary/10" />
      <div className="absolute inset-y-0 left-1/2 border-l border-primary/10" />
      <Search className="relative size-20 -rotate-12 stroke-1 text-primary/70" />
      <StarMark className="absolute top-9 right-9 size-5 text-primary" />
      <StarMark className="absolute bottom-10 left-8 size-3 text-primary/60" />
      <span className="absolute top-1/2 left-1 size-1.5 rounded-full bg-primary/50" />
      <span className="absolute right-14 bottom-7 size-1 rounded-full bg-primary/50" />
      <span className="absolute -bottom-3 font-mono text-[9px] tracking-[0.3em] text-muted-foreground">SEEK / FIND / READ</span>
    </div>
  )
}

interface SearchPageProps {
  searchParams: Promise<{ keyword?: string | string[]; page?: string | string[] }>
}

export default async function SearchPage({ searchParams }: SearchPageProps) {
  const { locale, t } = await getI18n()
  const params = await searchParams
  const keyword = typeof params.keyword === "string" ? params.keyword.trim() : ""
  const requestedPage = typeof params.page === "string" ? Number(params.page) : 1
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 && requestedPage <= 1_000_000 ? requestedPage : 1
  const [searchResponse, topicsResponse, recentResponse] = await Promise.allSettled([
    keyword ? searchPosts({ keyword, page, pageSize }, locale) : Promise.resolve([]),
    getCategoryWordCloud(),
    keyword ? Promise.resolve(null) : listPosts({ pageSize: 4, sortBy: "lastUpdateTime", sortType: "desc" }, locale),
  ])
  const results = searchResponse.status === "fulfilled" ? searchResponse.value : []
  const searchFailed = searchResponse.status === "rejected"
  const topics = topicsResponse.status === "fulfilled"
    ? topicsResponse.value.filter((topic) => topic.count > 0).sort((a, b) => b.count - a.count).slice(0, 8)
    : []
  const recent = recentResponse.status === "fulfilled" ? recentResponse.value?.items ?? [] : []

  const topicLinks = topics.map((topic) => (
    <Link key={topic.id} href={routes.blogList({ categoryId: topic.id })} className="group inline-flex items-center gap-3 rounded-md border border-border/80 bg-card/40 px-3.5 py-2.5 text-sm transition-colors hover:border-primary/50 hover:bg-primary/5 focus-visible:outline-2 focus-visible:outline-ring">
      <span className="text-primary/60" aria-hidden="true">#</span><span>{topic.name}</span>
      <span className="font-mono text-[10px] text-muted-foreground group-hover:text-primary">{topic.count.toString().padStart(2, "0")}</span>
    </Link>
  ))

  const wander = (
    <div className="relative self-start overflow-hidden rounded-lg border border-primary/20 bg-primary/5 p-6 sm:p-7">
      <Compass className="mb-7 size-7 stroke-1 text-primary" aria-hidden="true" />
      <h2 className={cn(headingClassName, "text-xl")}>{t.searchWander}</h2>
      <p className="mt-3 max-w-64 text-sm leading-7 text-muted-foreground">{t.searchWanderHint}</p>
      <Link href={routes.blogRandom} prefetch={false} className="mt-7 inline-flex items-center gap-2 text-sm text-primary underline-offset-4 hover:underline">
        <Shuffle className="size-4" aria-hidden="true" />{t.randomPost}<ArrowUpRight className="ml-2 size-4" aria-hidden="true" />
      </Link>
      <StarMark className="absolute -right-5 -bottom-7 size-32 text-primary/5" />
    </div>
  )

  return (
    <div className={cn(pageContainerClassName, "px-5 pt-12 pb-16 sm:px-8 sm:pt-16 sm:pb-24")}>
      <section aria-labelledby="search-heading" className="grid items-center gap-10 pb-10 lg:grid-cols-[minmax(0,1fr)_16rem] lg:gap-16 lg:pb-14">
        <div>
          <p className="flex items-center gap-2.5 text-[10px] font-medium tracking-[0.2em] text-primary"><span className="h-px w-6 bg-primary/70" />{t.searchEyebrow}</p>
          <h1 id="search-heading" className={cn(headingClassName, "mt-5 text-3xl leading-tight sm:text-5xl sm:leading-tight")}>{t.searchHeadline}</h1>
          <p className="mt-4 text-sm leading-7 text-muted-foreground sm:text-base">{t.searchDescription}</p>
          <div className="mt-8 sm:mt-9"><SearchForm key={keyword} keyword={keyword} /></div>
        </div>
        <SearchIllustration />
      </section>
      {keyword ? (
        <div className="grid gap-10 border-t border-border/80 pt-8 lg:grid-cols-[minmax(0,1fr)_16rem] lg:gap-16 lg:pt-10">
          <section aria-labelledby="search-results-heading">
            <div className="flex flex-wrap items-end justify-between gap-3 border-b border-border/80 pb-5">
              <div className="min-w-0"><p className="text-[10px] tracking-[0.18em] text-muted-foreground">{t.searchResultsLabel}</p><h2 id="search-results-heading" className={cn(headingClassName, "mt-2 text-2xl wrap-anywhere")}>“{keyword}”</h2></div>
              {!searchFailed && <p className="text-xs text-muted-foreground">{t.searchResultCount(results.length)}</p>}
            </div>
            {searchFailed || results.length === 0 ? (
              <div className="py-14 sm:py-20">
                <Search className="mb-6 size-9 stroke-1 text-primary/70" aria-hidden="true" />
                <h3 className={cn(headingClassName, "text-xl")}>{searchFailed ? t.searchFailedTitle : page > 1 ? t.pageUnavailable : t.searchEmptyTitle}</h3>
                <p className="mt-3 text-sm leading-7 text-muted-foreground">{searchFailed ? t.searchFailedHint : t.searchEmptyHint}</p>
                <div className="mt-6 flex flex-wrap gap-3">
                  {searchFailed ? <Button asChild><a href={searchPageHref(keyword, page)}>{t.retry}<ArrowRight /></a></Button> : page > 1 ? <Button asChild><Link href={routes.searchResults(keyword)}>{t.backToFirstPage}</Link></Button> : null}
                  <Button variant="outline" asChild><Link href={routes.blog}>{t.allPosts}<ArrowUpRight /></Link></Button>
                </div>
              </div>
            ) : (
              <ol start={(page - 1) * pageSize + 1} className="divide-y divide-border/70">
                {results.map((item, index) => (
                  <li key={item.id}>
                    <Link href={postHref(item)} className="group grid grid-cols-[1.5rem_minmax(0,1fr)_1rem] gap-3 py-7 focus-visible:outline-2 focus-visible:outline-ring sm:gap-4">
                      <span className="pt-1 font-mono text-[10px] text-primary/70" aria-hidden="true">{String((page - 1) * pageSize + index + 1).padStart(2, "0")}</span>
                      <div className="min-w-0">
                        <h3 className={cn(headingClassName, "text-xl leading-relaxed wrap-anywhere transition-colors group-hover:text-primary")}><HighlightedText text={item.highlightedTitle} /></h3>
                        <p className="mt-3 line-clamp-3 text-sm leading-7 wrap-anywhere text-muted-foreground"><HighlightedText text={snippetText(item.highlightedSnippet)} /></p>
                        <p className="mt-4 flex items-center gap-2 text-[11px] text-muted-foreground"><span>{t.updated}</span><time dateTime={item.lastUpdateTime}>{formatDate(item.lastUpdateTime)}</time></p>
                      </div>
                      <ArrowUpRight className="mt-2 size-4 text-muted-foreground transition-colors group-hover:text-primary" aria-hidden="true" />
                    </Link>
                  </li>
                ))}
              </ol>
            )}
            {!searchFailed && (page > 1 || results.length === pageSize) && (
              <nav aria-label={t.articlePagination} className="flex items-center justify-between gap-3 border-t border-border/80 pt-5">
                {page > 1 ? <Button variant="outline" size="sm" asChild><Link href={searchPageHref(keyword, page - 1)}><ChevronLeft />{t.previousPage}</Link></Button> : <span />}
                <span className="text-xs text-muted-foreground">{t.searchPageLabel(page)}</span>
                {results.length === pageSize ? <Button variant="outline" size="sm" asChild><Link href={searchPageHref(keyword, page + 1)}>{t.nextPage}<ChevronRight /></Link></Button> : <span />}
              </nav>
            )}
          </section>
          <aside className="space-y-8">
            {topics.length > 0 && <section aria-labelledby="search-topics-heading"><h2 id="search-topics-heading" className="mb-4 flex items-center gap-2 text-sm"><BookOpen className="size-4 text-primary" aria-hidden="true" />{t.searchTopics}</h2><div className="flex flex-wrap gap-2">{topicLinks}</div></section>}
            {wander}
          </aside>
        </div>
      ) : (
        <>
          <section aria-labelledby="search-topics-heading" className="border-y border-border/80 py-7 sm:py-8">
            <div className="mb-5 flex flex-wrap items-center justify-between gap-2"><h2 id="search-topics-heading" className="flex items-center gap-2 text-sm font-medium"><BookOpen className="size-4 text-primary" aria-hidden="true" />{t.searchTopics}</h2><p className="text-xs text-muted-foreground">{t.searchTopicsHint}</p></div>
            <div className="flex flex-wrap gap-2.5">{topicLinks.length > 0 ? topicLinks : <Link href={routes.blog} className="inline-flex items-center gap-2 text-sm text-primary hover:underline">{t.browseCategories}<ArrowUpRight className="size-4" /></Link>}</div>
          </section>
          <div className="grid gap-10 pt-10 lg:grid-cols-[minmax(0,1fr)_16rem] lg:gap-16 lg:pt-12">
            <section aria-labelledby="search-recent-heading">
              <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
                <div><p className="mb-2 text-[10px] tracking-[0.18em] text-primary">RECENT ENTRIES</p><h2 id="search-recent-heading" className={cn(headingClassName, "text-2xl")}>{t.searchRecent}</h2></div>
                <Link href={routes.blog} className="flex items-center gap-1.5 text-xs text-muted-foreground hover:text-primary">{t.allPosts}<ArrowUpRight className="size-3.5" aria-hidden="true" /></Link>
              </div>
              <p className="mb-4 text-xs text-muted-foreground">{t.searchRecentHint}</p>
              {recent.length > 0 ? <ol className="divide-y divide-border/70 border-y border-border/70">{recent.map((post, index) => (
                <li key={post.id}><Link href={postHref(post)} className="group grid grid-cols-[1.5rem_minmax(0,1fr)_1rem] items-center gap-3 py-5 focus-visible:outline-2 focus-visible:outline-ring sm:grid-cols-[1.5rem_minmax(0,1fr)_auto_1rem] sm:gap-4">
                  <span className="font-mono text-[10px] text-primary/70" aria-hidden="true">{String(index + 1).padStart(2, "0")}</span>
                  <div className="min-w-0"><p className="font-heading text-base leading-relaxed wrap-anywhere transition-colors group-hover:text-primary">{post.title}</p>{post.categoryName && <p className="mt-1.5 text-[11px] text-muted-foreground">{post.categoryName}</p>}<time dateTime={post.lastUpdateTime} className="mt-1.5 block text-[11px] text-muted-foreground sm:hidden">{formatDate(post.lastUpdateTime)}</time></div>
                  <time dateTime={post.lastUpdateTime} className="hidden font-mono text-[10px] text-muted-foreground sm:block">{formatDate(post.lastUpdateTime)}</time>
                  <ArrowUpRight className="size-4 text-muted-foreground group-hover:text-primary" aria-hidden="true" />
                </Link></li>
              ))}</ol> : <p className="border-y border-border/70 py-8 text-sm leading-7 text-muted-foreground">{recentResponse.status === "rejected" ? t.searchDiscoveryUnavailable : t.noPublishedPosts}</p>}
            </section>
            {wander}
          </div>
        </>
      )}
    </div>
  )
}

function searchPageHref(keyword: string, page: number) {
  return `${routes.searchResults(keyword)}${page > 1 ? `&page=${page}` : ""}`
}
