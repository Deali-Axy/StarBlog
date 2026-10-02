"use client"

import Link from "next/link"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { FormEvent, Suspense } from "react"
import { Rss, Search } from "lucide-react"

import { ThemeToggle } from "@/components/theme-toggle"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"

const navItems = [
  { href: routes.home, label: "首页" },
  { href: routes.blog, label: "文章" },
  { href: routes.photography, label: "摄影" },
  { href: routes.search, label: "搜索" },
]

function SearchBox() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const defaultKeyword = searchParams.get("keyword") ?? searchParams.get("search") ?? ""

  function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const keyword = String(form.get("keyword") ?? "").trim()
    if (!keyword) {
      router.push(routes.search)
      return
    }
    router.push(routes.searchResults(keyword))
  }

  return (
    <form onSubmit={onSubmit} className="relative hidden md:block">
      <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        name="keyword"
        defaultValue={defaultKeyword}
        placeholder="搜索文章"
        className="w-48 pl-8 lg:w-64"
      />
    </form>
  )
}

export function SiteHeader() {
  const pathname = usePathname()

  return (
    <header className="sticky top-0 z-40 border-b border-border/60 bg-background/80 backdrop-blur-md">
      <div className="mx-auto flex h-16 w-full max-w-6xl items-center gap-4 px-4">
        <Link href={routes.home} className="font-heading text-lg font-semibold tracking-tight">
          {site.name}
        </Link>
        <nav className="flex items-center gap-1">
          {navItems.map((item) => {
            const active = item.href === routes.home ? pathname === item.href : pathname.startsWith(item.href)
            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground",
                  active && "bg-muted text-foreground",
                )}
              >
                {item.label}
              </Link>
            )
          })}
        </nav>
        <div className="ml-auto flex items-center gap-2">
          <Suspense fallback={null}>
            <SearchBox />
          </Suspense>
          <Button variant="ghost" size="icon" asChild>
            <a href={routes.feed} aria-label="RSS">
              <Rss />
            </a>
          </Button>
          <ThemeToggle />
        </div>
      </div>
    </header>
  )
}
