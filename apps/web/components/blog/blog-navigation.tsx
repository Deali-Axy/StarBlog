"use client"

import Link from "next/link"
import { useRouter } from "next/navigation"
import { createContext, useContext, useMemo, useTransition, type ComponentProps, type ReactNode } from "react"

import { blogQueryHref, type BlogQuery } from "@/lib/blog-query"
import type { BlogVariant } from "@/lib/blog-variant"

interface BlogNavigation {
  query: BlogQuery
  variant: BlogVariant
  pending: boolean
  href: (patch?: Partial<BlogQuery>) => string
  navigate: (patch: Partial<BlogQuery>) => void
  follow: (href: string) => void
}

const BlogContext = createContext<BlogNavigation | null>(null)

export function BlogNavigationProvider({ query, variant, children }: {
  query: BlogQuery
  variant: BlogVariant
  children: ReactNode
}) {
  const router = useRouter()
  const [pending, startTransition] = useTransition()
  const value = useMemo<BlogNavigation>(() => {
    const follow = (href: string) => startTransition(() => router.push(href, { scroll: false }))
    return {
      query, variant, pending, follow,
      href: (patch = {}) => blogQueryHref(query, patch),
      navigate: (patch) => follow(blogQueryHref(query, patch)),
    }
  }, [query, variant, pending, router])

  return <BlogContext.Provider value={value}>{children}</BlogContext.Provider>
}

export function useBlogNavigation() {
  const context = useContext(BlogContext)
  if (!context) throw new Error("Blog navigation requires BlogNavigationProvider")
  return context
}

/** Keep native link behavior (including new tabs), while exposing navigation progress. */
export function BlogLink(props: Omit<ComponentProps<typeof Link>, "onNavigate">) {
  const { follow } = useBlogNavigation()
  return <Link {...props} onNavigate={(event) => {
    event.preventDefault()
    follow(String(props.href))
  }} />
}
