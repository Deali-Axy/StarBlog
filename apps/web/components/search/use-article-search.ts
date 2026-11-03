"use client"

import { useCallback, useEffect, useRef, useState, useTransition, type FormEvent, type ReactNode } from "react"
import { useRouter } from "next/navigation"
import { searchPageHref } from "@/lib/search-query"
import type { UtilityVariant } from "@/lib/utility-variant"

export function useArticleSearch(keyword: string, style: UtilityVariant) {
  const router = useRouter()
  const input = useRef<HTMLInputElement>(null)
  const bindInput = useCallback((element: HTMLInputElement | null) => { input.current = element }, [])
  const [value, setValue] = useState(keyword)
  const [pending, startTransition] = useTransition()

  useEffect(() => {
    function focusSearch(event: KeyboardEvent) {
      const target = event.target
      if (event.key !== "/" || event.metaKey || event.ctrlKey || event.altKey) return
      if (target instanceof HTMLElement && (target.closest("input, textarea, select, [role=menu]") || target.isContentEditable)) return
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
    startTransition(() => router.push(searchPageHref(next, 1, style)))
  }

  function clear() {
    setValue("")
    input.current?.focus()
  }

  return { bindInput, value, setValue, pending, submit, clear }
}

export type SearchController = ReturnType<typeof useArticleSearch>
export type SearchFormProps = { search: SearchController; hiddenFields: ReactNode }
