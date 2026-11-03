import Link from "next/link"
import { ChevronLeft, ChevronRight } from "lucide-react"
import { HighlightedText, snippetText } from "@/components/search/search-text"
import type { SearchViewProps } from "@/components/search/search-view-props"
import { formatDate } from "@/lib/format"
import { postHref, routes } from "@/lib/routes"
import { SEARCH_PAGE_SIZE } from "@/lib/search-query"
import { site } from "@/lib/site"
import { pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function FlatSearch({ keyword, page, results, searchFailed, topics, t, form, pageHref, keywordHref }: SearchViewProps) {
  if (!keyword) {
    return (
      <div className={cn(pageContainerClassName, "flex min-h-[60svh] flex-col items-center justify-center py-16 sm:py-24")}>
        <div className="w-full max-w-2xl text-center">
          <h1 className="text-4xl font-semibold tracking-tight wrap-anywhere sm:text-5xl">{site.name}{" "}<span className="ml-3 text-2xl font-normal text-muted-foreground sm:text-3xl">{t.search}</span></h1>
          <p className="mt-4 text-sm leading-7 text-muted-foreground">{t.searchHint}</p>
          <div className="mt-7 text-left">{form}</div>
          {topics.length > 0 ? (
            <nav aria-label={t.searchTopics} className="mt-6 flex flex-wrap items-center justify-center gap-x-5 gap-y-1 text-xs text-muted-foreground">
              {topics.slice(0, 5).map((topic) => <Link key={topic.id} href={keywordHref(topic.name)} className="inline-flex min-h-10 items-center underline decoration-transparent underline-offset-4 hover:text-foreground hover:decoration-current">{topic.name}</Link>)}
            </nav>
          ) : null}
          <div className="mt-6 flex justify-center gap-6 text-xs text-muted-foreground">
            <Link href={routes.blog} className="inline-flex min-h-11 items-center hover:text-foreground hover:underline">{t.allPosts}</Link>
            <Link href={routes.blogRandom} prefetch={false} className="inline-flex min-h-11 items-center hover:text-foreground hover:underline">{t.randomPost}</Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className={cn(pageContainerClassName, "pt-8 pb-16 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-ring")}>
      <header className="border-b border-border pb-6">
        <h1 className="sr-only">{t.searchResultsLabel}: {keyword}</h1>
        <div className="max-w-3xl">{form}</div>
      </header>
      <section aria-label={t.searchResultsLabel} className="max-w-3xl pt-5">
        {!searchFailed && <p className="text-xs text-muted-foreground" role="status">{t.searchResultCount(results.length)}</p>}
        {searchFailed || results.length === 0 ? (
          <div className="py-10">
            <h2 className="text-lg font-medium wrap-anywhere">{searchFailed ? t.searchFailedTitle : page > 1 ? t.pageUnavailable : t.searchFlatEmptyTitle(keyword)}</h2>
            <p className="mt-3 text-sm leading-7 text-muted-foreground">{searchFailed ? t.searchFailedHint : t.searchFlatEmptyHint}</p>
            <div className="mt-5 flex flex-wrap gap-x-6 gap-y-2 text-sm">
              {searchFailed ? <a href={pageHref(page)} className="inline-flex min-h-11 items-center underline underline-offset-4">{t.retry}</a> : page > 1 ? <Link href={pageHref(1)} className="inline-flex min-h-11 items-center underline underline-offset-4">{t.backToFirstPage}</Link> : null}
              <Link href={routes.blog} className="inline-flex min-h-11 items-center underline underline-offset-4">{t.allPosts}</Link>
            </div>
          </div>
        ) : (
          <ol className="space-y-8 py-7">
            {results.map((item) => (
              <li key={item.id} className="min-w-0">
                <Link href={postHref(item)} className="group block">
                  <p className="truncate text-xs text-muted-foreground">{site.name}<span aria-hidden="true" className="px-2">›</span>{site.url}{postHref(item)}</p>
                  <h2 className="mt-1.5 text-xl leading-8 wrap-anywhere text-blue-800 group-hover:underline dark:text-blue-300"><HighlightedText text={item.highlightedTitle} /></h2>
                </Link>
                <p className="mt-1 line-clamp-3 text-sm leading-7 wrap-anywhere text-muted-foreground"><time dateTime={item.lastUpdateTime}>{formatDate(item.lastUpdateTime)}</time><span aria-hidden="true"> — </span><HighlightedText text={snippetText(item.highlightedSnippet)} /></p>
              </li>
            ))}
          </ol>
        )}
        {!searchFailed && (page > 1 || results.length === SEARCH_PAGE_SIZE) && (
          <nav aria-label={t.articlePagination} className="mt-3 flex flex-wrap items-center gap-5 border-t border-border pt-5 text-sm">
            {page > 1 ? <Link href={pageHref(page - 1)} className="inline-flex min-h-11 items-center gap-1.5 hover:underline"><ChevronLeft aria-hidden="true" className="size-4" />{t.previousPage}</Link> : null}
            <span className="text-muted-foreground" aria-current="page">{t.searchPageLabel(page)}</span>
            {results.length === SEARCH_PAGE_SIZE ? <Link href={pageHref(page + 1)} className="inline-flex min-h-11 items-center gap-1.5 hover:underline">{t.nextPage}<ChevronRight aria-hidden="true" className="size-4" /></Link> : null}
          </nav>
        )}
      </section>
    </div>
  )
}
