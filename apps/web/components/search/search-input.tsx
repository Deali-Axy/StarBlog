"use client"

import { useI18n } from "@/components/app-providers"
import type { SearchController } from "@/components/search/use-article-search"

export function SearchInput({ search, placeholder }: { search: SearchController; placeholder: string }) {
  const { t } = useI18n()
  const { bindInput, value, setValue, clear } = search
  return (
    <>
      <label htmlFor="article-search" className="sr-only">{t.searchPosts}</label>
      <input
        ref={bindInput}
        id="article-search"
        type="search"
        name="keyword"
        value={value}
        onChange={(event) => setValue(event.target.value)}
        onKeyDown={(event) => { if (event.key === "Escape") clear() }}
        placeholder={placeholder}
        aria-describedby="search-help"
        autoComplete="off"
        enterKeyHint="search"
        required
        className="h-11 min-w-0 flex-1 bg-transparent text-base outline-none placeholder:text-muted-foreground/70 [&::-webkit-search-cancel-button]:appearance-none"
      />
    </>
  )
}
