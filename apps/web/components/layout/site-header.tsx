"use client"

import Link from "next/link"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { FormEvent, Suspense } from "react"
import { Rss, Search } from "lucide-react"

import { StarMark } from "@/components/brand/star-mark"
import { ThemeToggle } from "@/components/theme-toggle"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"
import { useI18n } from "@/components/app-providers"
import { localeCookie } from "@/lib/i18n"

function SearchBox() {
  const { t } = useI18n()
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
        placeholder={t.searchPosts}
        aria-label={t.searchPosts}
        className="h-8 w-44 bg-transparent pl-8 lg:w-56"
      />
    </form>
  )
}

export function SiteHeader() {
  const pathname = usePathname()
  const { locale, t } = useI18n()
  const navItems = [
    { href: routes.home, label: t.home },
    { href: routes.blog, label: t.posts },
    { href: routes.photography, label: t.photography },
    { href: routes.links, label: t.links },
    { href: routes.about, label: t.about },
  ]

  function switchLanguage() {
    const next = locale === "zh" ? "en" : "zh"
    document.cookie = `${localeCookie}=${next}; Path=/; Max-Age=31536000; SameSite=Lax${location.protocol === "https:" ? "; Secure" : ""}`
    const url = new URL(window.location.href)
    // An explicit article language must not override the new site preference.
    url.searchParams.delete("lang")
    window.location.assign(url.toString())
  }

  return (
    <header className="sticky top-0 z-40 border-b border-border/70 bg-background/85 backdrop-blur-md">
      <div className="page-wrap flex flex-wrap items-center gap-x-4 gap-y-3 py-3 lg:flex-nowrap">
        <Link href={routes.home} className="flex items-center gap-2 font-heading text-base tracking-tight">
          <StarMark className="size-3.5 text-primary" />
          {site.name}
        </Link>
        <nav className="order-last flex w-full items-center gap-5 overflow-x-auto whitespace-nowrap text-sm lg:order-none lg:w-auto">
          {navItems.map((item) => {
            const active = item.href === routes.home ? pathname === item.href : pathname.startsWith(item.href)
            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "text-muted-foreground transition-colors hover:text-foreground",
                  active && "text-foreground",
                )}
              >
                {item.label}
              </Link>
            )
          })}
        </nav>
        <div className="ml-auto flex items-center gap-1">
          <Suspense
            fallback={
              <form className="relative hidden md:block">
                <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  name="keyword"
                  placeholder={t.searchPosts}
                  aria-label={t.searchPosts}
                  className="h-8 w-44 bg-transparent pl-8 lg:w-56"
                />
              </form>
            }
          >
            <SearchBox />
          </Suspense>
          <Button variant="ghost" size="icon-sm" asChild>
            <a href={routes.feed} aria-label="RSS">
              <Rss />
            </a>
          </Button>
          <ThemeToggle />
          <Button
            variant="ghost"
            size="sm"
            onClick={switchLanguage}
            aria-label={t.switchLanguage}
            title={t.switchLanguage}
          >
            {locale === "zh" ? "EN" : "中文"}
          </Button>
        </div>
      </div>
    </header>
  )
}
