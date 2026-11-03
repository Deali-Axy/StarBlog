"use client"

import { ArrowRight, LoaderCircle, Search, X } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import { SearchInput } from "@/components/search/search-input"
import type { SearchFormProps } from "@/components/search/use-article-search"
import { routes } from "@/lib/routes"

export function SearchForm({ search, hiddenFields }: SearchFormProps) {
  const { t } = useI18n()

  return (
    <form action={routes.search} method="get" role="search" onSubmit={search.submit} aria-busy={search.pending}>
      {hiddenFields}
      <div className="flex items-center gap-3 rounded-lg border border-primary/35 bg-card p-2 pl-4 shadow-sm transition-shadow focus-within:ring-2 focus-within:ring-primary/30 sm:gap-4 sm:p-3 sm:pl-5">
        <Search className="size-5 shrink-0 text-primary" aria-hidden="true" />
        <SearchInput search={search} placeholder={t.searchPlaceholder} />
        {search.value && (
          <button type="button" aria-label={t.removeSearch} onClick={search.clear} className="rounded-sm p-1 text-muted-foreground hover:text-foreground focus-visible:outline-2 focus-visible:outline-ring">
            <X className="size-4" aria-hidden="true" />
          </button>
        )}
        <Button type="submit" disabled={search.pending || !search.value.trim()} className="h-11 gap-2 rounded-md px-4 sm:px-5">
          <span className="hidden sm:inline">{t.search}</span>
          {search.pending ? <LoaderCircle className="size-4 motion-safe:animate-spin" aria-hidden="true" /> : <ArrowRight className="size-4" aria-hidden="true" />}
          <span className="sr-only sm:hidden">{t.search}</span>
        </Button>
      </div>
      <div id="search-help" className="mt-4 flex flex-wrap items-center justify-between gap-x-4 gap-y-2 text-xs text-muted-foreground">
        <span>{t.searchScope}</span>
        <span className="hidden items-center gap-1.5 sm:inline-flex">{t.searchShortcutBefore}<kbd className="rounded border border-border px-1.5 py-0.5 font-mono text-[10px]">/</kbd>{t.searchShortcutAfter}</span>
      </div>
      <p role="status" className="sr-only">{search.pending ? t.loadingPosts : ""}</p>
    </form>
  )
}
