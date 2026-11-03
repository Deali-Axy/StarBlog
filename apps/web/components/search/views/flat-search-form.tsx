import { LoaderCircle, Search, X } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { SearchInput } from "@/components/search/search-input"
import type { SearchFormProps } from "@/components/search/use-article-search"
import { routes } from "@/lib/routes"

export function FlatSearchForm({ search, hiddenFields }: SearchFormProps) {
  const { t } = useI18n()

  return (
    <form action={routes.search} method="get" role="search" onSubmit={search.submit} aria-busy={search.pending}>
      {hiddenFields}
      <div className="flex items-center gap-2 rounded-full border border-border bg-background py-1.5 pr-2 pl-5 transition-colors hover:border-foreground/30 focus-within:border-ring focus-within:ring-1 focus-within:ring-ring sm:gap-3">
        <Search className="size-5 shrink-0 text-muted-foreground" aria-hidden="true" />
        <SearchInput search={search} placeholder={t.searchFlatPlaceholder} />
        {search.value ? (
          <button type="button" aria-label={t.removeSearch} onClick={search.clear} className="grid size-11 shrink-0 place-items-center rounded-full text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline-2 focus-visible:outline-ring"><X className="size-4" aria-hidden="true" /></button>
        ) : null}
        <button type="submit" disabled={search.pending || !search.value.trim()} aria-label={t.search} className="grid size-11 shrink-0 place-items-center rounded-full bg-foreground text-background transition-opacity hover:opacity-80 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring disabled:opacity-40">
          {search.pending ? <LoaderCircle className="size-4 motion-safe:animate-spin" aria-hidden="true" /> : <Search className="size-4" aria-hidden="true" />}
        </button>
      </div>
      <p id="search-help" className="sr-only">{t.searchScope}</p>
      <p role="status" className="sr-only">{search.pending ? t.loadingPosts : ""}</p>
    </form>
  )
}
