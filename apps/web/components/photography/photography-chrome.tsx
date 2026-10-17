"use client"

import { Menu, Search } from "lucide-react"
import Link from "next/link"
import { DropdownMenu } from "radix-ui"

import { useI18n } from "@/components/app-providers"
import { StarMark } from "@/components/brand/star-mark"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { ThemeToggle } from "@/components/theme-toggle"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"

export function PhotographyHeader() {
  const { t } = useI18n()
  const links = [
    { href: routes.home, label: t.home },
    { href: routes.blog, label: t.posts },
    { href: routes.photography, label: t.photography },
    { href: routes.links, label: t.links },
    { href: routes.about, label: t.about },
  ]

  return (
    <header className="photo-header sticky top-0 z-40 border-b border-border bg-background/95 backdrop-blur-md">
      <div className="photo-inset flex h-16 items-center gap-2 sm:h-[72px] lg:gap-8">
        <Link href={routes.home} className="flex shrink-0 items-center gap-2.5 text-xl font-semibold tracking-tight">
          <StarMark className="size-5" />
          {site.name}
        </Link>
        <nav aria-label={t.siteNavigation} className="hidden items-center gap-1 lg:flex">
          {links.map(({ href, label }) => (
            <Link key={href} href={href} aria-current={href === routes.photography ? "page" : undefined} className="rounded-xl px-4 py-2.5 text-sm font-medium text-muted-foreground transition hover:bg-muted hover:text-foreground aria-[current=page]:bg-muted aria-[current=page]:text-foreground">
              {label}
            </Link>
          ))}
        </nav>
        <div className="ml-auto flex items-center gap-0.5 sm:gap-1">
          <Link href={routes.search} aria-label={t.searchPosts} title={t.searchPosts} className="photo-icon">
            <Search className="size-5" />
          </Link>
          <ThemeToggle iconOnly className="size-11 rounded-xl" />
          <LanguageToggle className="h-11 min-w-11 rounded-xl px-2 text-xs" />
          <DropdownMenu.Root>
            <DropdownMenu.Trigger className="photo-icon lg:hidden" aria-label={t.siteNavigation}>
              <Menu className="size-5" />
            </DropdownMenu.Trigger>
            <DropdownMenu.Portal>
              <DropdownMenu.Content align="end" sideOffset={8} className="photography-theme z-50 min-w-44 rounded-xl border border-border bg-background p-1.5 text-foreground shadow-lg">
                {links.map(({ href, label }) => (
                  <DropdownMenu.Item key={href} asChild>
                    <Link href={href} aria-current={href === routes.photography ? "page" : undefined} className="block rounded-lg px-4 py-3 text-sm outline-none data-highlighted:bg-muted aria-[current=page]:font-semibold">
                      {label}
                    </Link>
                  </DropdownMenu.Item>
                ))}
              </DropdownMenu.Content>
            </DropdownMenu.Portal>
          </DropdownMenu.Root>
        </div>
      </div>
    </header>
  )
}

export function PhotographyFooter() {
  const { t } = useI18n()
  return (
    <footer className="photo-inset border-t border-border bg-background py-6 text-xs text-muted-foreground">
      <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-center">
        <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
          <Link href={routes.home} className="flex items-center gap-2 font-semibold text-foreground">
            <StarMark className="size-3.5" />{site.name}
          </Link>
          <span>© {new Date().getFullYear()} {site.authorName}</span>
        </div>
        <nav aria-label={t.footerNavigation} className="flex flex-wrap gap-x-5 gap-y-3">
          <Link href={routes.blog} className="hover:text-foreground">{t.posts}</Link>
          <Link href={routes.photography} className="hover:text-foreground">{t.photography}</Link>
          <Link href={routes.links} className="hover:text-foreground">{t.links}</Link>
          <Link href={routes.about} className="hover:text-foreground">{t.about}</Link>
          <Link href={routes.rss} className="hover:text-foreground">RSS</Link>
        </nav>
      </div>
    </footer>
  )
}
