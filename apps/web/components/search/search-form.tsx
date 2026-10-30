"use client"

import { useEffect, useRef, useState, useTransition, type FormEvent } from "react"
import { useRouter } from "next/navigation"
import { ArrowRight, LoaderCircle, Search, X } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

export function SearchForm({ keyword }: { keyword: string }) {
  const { t } = useI18n()
  const router = useRouter()
  const input = useRef<HTMLInputElement>(null)
  const [value, setValue] = useState(keyword)
  const [pending, startTransition] = useTransition()

  useEffect(() => {
    function focusSearch(event: KeyboardEvent) {
      const target = event.target
      if (event.key !== "/" || event.metaKey || event.ctrlKey || event.altKey) return
      if (target instanceof HTMLElement && (target.closest("input, textarea, select") || target.isContentEditable)) return
      event.preventDefault()
      input.current?.focus()
    }
    document.addEventListener("keydown", focusSearch)
    return () => document.removeEventListener("keydown", focusSearch)
  }, [])

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const next = value.trim()
    if (!next || pending) return
    startTransition(() => router.push(routes.searchResults(next)))
  }

  return (
    <form action={routes.search} method="get" role="search" onSubmit={submit} aria-busy={pending}>
      <label htmlFor="article-search" className="sr-only">{t.searchPosts}</label>
      <div className="flex items-center gap-3 rounded-lg border border-primary/35 bg-card p-2 pl-4 shadow-sm transition-shadow focus-within:ring-2 focus-within:ring-primary/30 sm:gap-4 sm:p-3 sm:pl-5">
        <Search className="size-5 shrink-0 text-primary" aria-hidden="true" />
        <input
          ref={input}
          id="article-search"
          type="search"
          name="keyword"
          value={value}
          onChange={(event) => setValue(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === "Escape") {
              setValue("")
              input.current?.focus()
            }
          }}
          placeholder={t.searchPlaceholder}
          aria-describedby="search-help"
          autoComplete="off"
          enterKeyHint="search"
          required
          className="h-11 min-w-0 flex-1 bg-transparent text-base outline-none placeholder:text-muted-foreground/70 [&::-webkit-search-cancel-button]:appearance-none"
        />
        {value && (
          <button type="button" aria-label={t.removeSearch} onClick={() => { setValue(""); input.current?.focus() }} className="rounded-sm p-1 text-muted-foreground hover:text-foreground focus-visible:outline-2 focus-visible:outline-ring">
            <X className="size-4" aria-hidden="true" />
          </button>
        )}
        <Button type="submit" disabled={pending || !value.trim()} className="h-11 gap-2 rounded-md px-4 sm:px-5">
          <span className="hidden sm:inline">{t.search}</span>
          {pending ? <LoaderCircle className="size-4 motion-safe:animate-spin" aria-hidden="true" /> : <ArrowRight className="size-4" aria-hidden="true" />}
          <span className="sr-only sm:hidden">{t.search}</span>
        </Button>
      </div>
      <div id="search-help" className="mt-4 flex flex-wrap items-center justify-between gap-x-4 gap-y-2 text-xs text-muted-foreground">
        <span>{t.searchScope}</span>
        <span className="hidden items-center gap-1.5 sm:inline-flex">{t.searchShortcutBefore}<kbd className="rounded border border-border px-1.5 py-0.5 font-mono text-[10px]">/</kbd>{t.searchShortcutAfter}</span>
      </div>
      <p role="status" className="sr-only">{pending ? t.loadingPosts : ""}</p>
    </form>
  )
}
