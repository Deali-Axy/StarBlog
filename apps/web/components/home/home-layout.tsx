"use client"

import Link from "next/link"
import type { ReactNode } from "react"
import { ArrowRight, ArrowUp, ArrowUpRight, Rss, Search } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { StarMark } from "@/components/brand/star-mark"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { ThemeToggle } from "@/components/theme-toggle"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { homeThemeClassName, pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function HomeLayout({ children }: { children: ReactNode }) {
  const { t } = useI18n()
  const entries = [
    { href: routes.home, label: t.home, caption: "COVER" },
    { href: routes.blog, label: t.posts, caption: "JOURNAL" },
    { href: routes.photography, label: t.photography, caption: "GALLERY" },
    { href: routes.links, label: t.links, caption: "CONNECTIONS" },
    { href: routes.about, label: t.about, caption: "PROFILE" },
  ]

  return (
    <div data-home-layout className={cn(homeThemeClassName, "min-h-dvh bg-background text-foreground [&_:is(a,button):focus-visible]:outline-2 [&_:is(a,button):focus-visible]:outline-solid [&_:is(a,button):focus-visible]:outline-offset-4 [&_:is(a,button):focus-visible]:outline-primary")} id="home-top">
      <a href="#home-content" className="fixed top-4 left-4 z-50 -translate-y-[calc(100%+2rem)] bg-foreground px-4 py-3 text-background focus:translate-y-0">{t.skipToContent}</a>
      <div className="mx-auto max-w-[96rem] px-2 sm:px-6 min-[1120px]:grid min-[1120px]:grid-cols-[12rem_minmax(0,1fr)] min-[1120px]:gap-10 min-[1120px]:px-8 min-[1440px]:gap-14 min-[1440px]:px-12">
        <header className="flex flex-wrap items-center justify-between gap-x-2 gap-y-6 px-4 pt-7 sm:gap-x-6 sm:px-0 min-[1120px]:sticky min-[1120px]:top-0 min-[1120px]:h-dvh min-[1120px]:flex-col min-[1120px]:flex-nowrap min-[1120px]:items-stretch min-[1120px]:justify-start min-[1120px]:gap-0 min-[1120px]:self-start min-[1120px]:overflow-y-auto min-[1120px]:border-r min-[1120px]:border-border min-[1120px]:pt-13 min-[1120px]:pr-7 min-[1120px]:pb-6 [@media(min-width:1120px)_and_(max-height:740px)]:pt-6 [@media(min-width:1120px)_and_(max-height:740px)]:pb-4">
          <Link href={routes.home} className="inline-flex min-w-0 items-center gap-2 font-heading text-xl leading-none font-semibold tracking-[-.04em] wrap-anywhere sm:gap-3 sm:text-2xl min-[1120px]:shrink-0 min-[1120px]:items-start min-[1120px]:gap-2.5" aria-label={`${site.name} · ${t.home}`}>
            <StarMark className="size-6 shrink-0 text-primary sm:size-8 min-[1120px]:size-7" />
            <span>{site.name}<span className="mt-2 block font-sans text-[10px] font-normal tracking-[.2em] text-muted-foreground min-[1120px]:mt-3 min-[1120px]:text-[9px]">{t.homeCoverLabel}</span></span>
          </Link>

          <nav aria-label={t.siteNavigation} className="order-3 w-full min-[1120px]:order-none min-[1120px]:mt-16 min-[1120px]:shrink-0 [@media(min-width:1120px)_and_(max-height:740px)]:mt-6">
            <p className="mb-4 hidden justify-between text-[10px] tracking-[.2em] text-muted-foreground min-[1120px]:flex">{t.index}<span aria-hidden="true">↙</span></p>
            <ul className="grid grid-cols-2 gap-x-6 sm:grid-cols-4 min-[1120px]:block">
              {entries.map((entry, index) => (
                <li key={entry.href} className={index === 0 ? "hidden min-[1120px]:block" : undefined}>
                  <Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className="group flex min-h-14 items-center gap-3 border-t border-border py-3 hover:text-primary min-[1120px]:min-h-15.5 aria-[current=page]:text-primary max-[359px]:gap-2 [@media(min-width:1120px)_and_(max-height:740px)]:min-h-13">
                    <span className="mt-1 self-start font-mono text-[10px] text-muted-foreground group-aria-[current=page]:text-primary" aria-hidden="true">{String(index).padStart(2, "0")}</span>
                    <span className="min-w-0">
                      <span className="font-heading text-lg wrap-anywhere min-[1120px]:text-base max-[359px]:text-base">{entry.label}</span>
                      <span className="mt-1.5 hidden font-mono text-[8px] tracking-[.12em] text-muted-foreground min-[1120px]:block [@media(min-width:1120px)_and_(max-height:740px)]:hidden" aria-hidden="true">{entry.caption}</span>
                    </span>
                    <ArrowUpRight aria-hidden="true" className="ml-auto size-4 shrink-0 text-muted-foreground transition-[transform,color] duration-180 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 group-hover:text-primary group-aria-[current=page]:size-1.5 group-aria-[current=page]:rounded-full group-aria-[current=page]:bg-primary group-aria-[current=page]:text-transparent motion-reduce:transition-none max-[359px]:hidden" />
                  </Link>
                </li>
              ))}
            </ul>
          </nav>

          <div className="flex items-center gap-0 sm:gap-2 min-[1120px]:mt-auto min-[1120px]:shrink-0 min-[1120px]:flex-col min-[1120px]:items-stretch min-[1120px]:gap-3 min-[1120px]:pt-8 [@media(min-width:1120px)_and_(max-height:740px)]:pt-4">
            <Link href={routes.search} className="inline-flex min-h-11 min-w-11 items-center justify-center text-muted-foreground hover:text-primary min-[1120px]:justify-start min-[1120px]:gap-2.5 min-[1120px]:border-b min-[1120px]:border-border min-[1120px]:text-xs" aria-label={t.searchPosts}>
              <Search aria-hidden="true" className="size-4" />
              <span className="hidden min-[1120px]:block">{t.searchPosts}</span>
              <ArrowRight aria-hidden="true" className="ml-auto hidden size-3.5 min-[1120px]:block" />
            </Link>
            <div className="flex items-center gap-0 sm:gap-0.5 min-[1120px]:justify-between">
              <ThemeToggle iconOnly />
              <LanguageToggle />
              <Link href={routes.rss} aria-label={t.subscribe} title={t.subscribe} className="hidden size-11 items-center justify-center sm:inline-flex">
                <Rss aria-hidden="true" className="size-4" />
              </Link>
            </div>
          </div>
        </header>

        <div className="min-w-0 min-[1120px]:@container min-[1120px]:[&_:is(#home-hero-name:not([data-long-name=true]))]:text-[clamp(5.5rem,13cqw,11.5rem)]">
          <main id="home-content" tabIndex={-1}>{children}</main>
          <footer className={cn(pageContainerClassName, "pb-8 min-[1120px]:pb-10")}>
            <div className="flex flex-wrap items-end justify-between gap-8 border-t border-foreground pt-8 pb-2">
              <div>
                <p className="font-heading text-[clamp(1.5rem,3vw,2rem)] tracking-[-.04em]">{t.homeFooterHeadline}<span className="ml-3 text-primary" aria-hidden="true">＊</span></p>
                <p className="mt-3 text-[11px] text-muted-foreground">{site.name} / {t.hero}</p>
              </div>
              <a href="#home-top" className="inline-flex min-h-11 items-center gap-4 text-xs text-muted-foreground hover:text-primary">{t.backToTop}<ArrowUp aria-hidden="true" className="size-4" /></a>
            </div>
          </footer>
        </div>
      </div>
    </div>
  )
}
