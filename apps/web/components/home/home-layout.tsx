"use client"

import Link from "next/link"
import { useSyncExternalStore, type ReactNode } from "react"
import { ArrowRight, ArrowUp, ArrowUpRight, Moon, Rss, Search, Sun } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { StarMark } from "@/components/brand/star-mark"
import { useHomeDesign } from "@/components/home/home-design-context"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { routes } from "@/lib/routes"
import { getSite, site } from "@/lib/site"
import { cn } from "@/lib/utils"
import styles from "./home-design.module.css"

function useEntries() {
  const { t } = useI18n()
  return [
    { href: routes.home, label: t.home, caption: "COVER" },
    { href: routes.blog, label: t.posts, caption: "JOURNAL" },
    { href: routes.photography, label: t.photography, caption: "GALLERY" },
    { href: routes.links, label: t.links, caption: "CONNECTIONS" },
    { href: routes.about, label: t.about, caption: "PROFILE" },
  ]
}
function Brand() {
  const { t } = useI18n()
  return <Link href={routes.home} className="inline-flex min-w-0 items-start gap-2.5 font-heading text-xl leading-none font-semibold tracking-[-.04em] wrap-anywhere" aria-label={`${site.name} · ${t.home}`}>
    <StarMark className="size-7 shrink-0 text-primary" />
    <span>{site.name}<span className="mt-3 block font-sans text-[9px] font-normal tracking-[.2em] text-muted-foreground">{t.homeCoverLabel}</span></span>
  </Link>
}
const subscribeMounted = () => () => {}
function Tools({ sidebar = false }: { sidebar?: boolean }) {
  const { t } = useI18n()
  const { effectiveMode, changeMode } = useHomeDesign()
  const mounted = useSyncExternalStore(subscribeMounted, () => true, () => false)
  return <div className={cn("flex items-center gap-0 sm:gap-2", sidebar && "min-[70rem]:mt-auto min-[70rem]:shrink-0 min-[70rem]:flex-col min-[70rem]:items-stretch min-[70rem]:gap-3 min-[70rem]:pt-8")}>
    <Link href={routes.search} className={cn("inline-flex min-h-11 min-w-11 items-center justify-center text-muted-foreground hover:text-primary", sidebar && "min-[70rem]:justify-start min-[70rem]:gap-2.5 min-[70rem]:border-b min-[70rem]:border-border min-[70rem]:text-xs")} aria-label={t.searchPosts}>
      <Search aria-hidden="true" className="size-4" /><span className={sidebar ? "hidden min-[70rem]:block" : "hidden"}>{t.searchPosts}</span>
      {sidebar ? <ArrowRight aria-hidden="true" className="ml-auto hidden size-3.5 min-[70rem]:block" /> : null}
    </Link>
    <div className="flex items-center min-[70rem]:justify-between">
      <button type="button" className="grid size-11 cursor-pointer place-items-center hover:text-primary" aria-label={t.switchTheme} title={t.switchTheme} onClick={() => changeMode((effectiveMode ?? (document.documentElement.classList.contains("dark") ? "dark" : "light")) === "dark" ? "light" : "dark")}>
        {mounted && effectiveMode === "light" ? <Moon aria-hidden="true" className="size-5" /> : <Sun aria-hidden="true" className="size-5" />}
      </button>
      <LanguageToggle className="min-h-11 min-w-11" />
      <Link href={routes.rss} aria-label={t.subscribe} title={t.subscribe} className="hidden size-11 items-center justify-center sm:inline-flex"><Rss aria-hidden="true" className="size-4" /></Link>
    </div>
  </div>
}
function NumberedNavigation() {
  const { t } = useI18n()
  const entries = useEntries()
  return <header data-home-navigation="numbered" className="flex flex-wrap items-center justify-between gap-x-2 gap-y-6 px-4 pt-7 sm:gap-x-6 sm:px-0 min-[70rem]:sticky min-[70rem]:top-0 min-[70rem]:h-dvh min-[70rem]:flex-col min-[70rem]:flex-nowrap min-[70rem]:items-stretch min-[70rem]:justify-start min-[70rem]:gap-0 min-[70rem]:self-start min-[70rem]:overflow-y-auto min-[70rem]:border-r min-[70rem]:border-border min-[70rem]:pt-13 min-[70rem]:pr-7 min-[70rem]:pb-6 [@media(min-width:1120px)_and_(max-height:740px)]:pt-6">
    <Brand />
    <nav aria-label={t.siteNavigation} className="order-3 w-full min-[70rem]:order-none min-[70rem]:mt-16 min-[70rem]:shrink-0 [@media(min-width:1120px)_and_(max-height:740px)]:mt-6">
      <p className="mb-4 hidden justify-between text-[10px] tracking-[.2em] text-muted-foreground min-[70rem]:flex">{t.index}<span aria-hidden="true">↙</span></p>
      <ul className="grid grid-cols-2 gap-x-6 sm:grid-cols-4 min-[70rem]:block">
        {entries.map((entry, index) => <li key={entry.href} className={index === 0 ? "hidden min-[70rem]:block" : undefined}>
          <Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className="group flex min-h-14 items-center gap-3 border-t border-border py-3 hover:text-primary min-[70rem]:min-h-15.5 aria-[current=page]:text-primary">
            <span className="mt-1 self-start font-mono text-[10px] text-muted-foreground" aria-hidden="true">{String(index).padStart(2, "0")}</span>
            <span className="min-w-0"><span className="font-heading text-lg wrap-anywhere min-[70rem]:text-base">{entry.label}</span><span className="mt-1.5 hidden font-mono text-[8px] tracking-[.12em] text-muted-foreground min-[70rem]:block" aria-hidden="true">{entry.caption}</span></span>
            <ArrowUpRight aria-hidden="true" className="ml-auto size-4 shrink-0 text-muted-foreground max-[359px]:hidden" />
          </Link>
        </li>)}
      </ul>
    </nav>
    <Tools sidebar />
  </header>
}
function CompactNavigation() {
  const { t } = useI18n()
  const entries = useEntries()
  return <header data-home-navigation="compact" className={styles.compactHeader}>
    <Brand />
    <nav aria-label={t.siteNavigation} className={styles.compactNav}><ul>{entries.map((entry, index) => <li key={entry.href}><Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className="hover:text-primary aria-[current=page]:text-primary">{entry.label}</Link></li>)}</ul></nav>
    <Tools />
  </header>
}
function HomeFooter() {
  const { t, locale } = useI18n()
  const { config } = useHomeDesign()
  const author = getSite(locale)
  const contact = config.footer === "contact"
  return <footer data-home-footer={config.footer} className={cn(styles.container, "pt-12 pb-24 sm:pb-12")}>
    <div data-home-surface={contact ? "" : undefined} data-home-padding={contact ? "" : undefined} className={cn("flex flex-wrap items-end justify-between gap-8 border-t border-foreground pt-8 pb-2", contact && "border border-border bg-card")}>
      <div className="min-w-0">
        <p className="font-heading text-[clamp(1.5rem,3vw,2rem)] tracking-[-.04em]">{contact ? author.authorName : t.homeFooterHeadline}<span className="ml-3 text-primary" aria-hidden="true">＊</span></p>
        <p className="mt-3 text-xs text-muted-foreground">{contact ? author.authorBio : `${author.name} / ${t.hero}`}</p>
        {contact ? <div className="mt-4 flex flex-wrap gap-x-6 text-xs [&_a]:inline-flex [&_a]:min-h-11 [&_a]:items-center [&_a]:gap-1 [&_a]:hover:text-primary">
          <a href={author.githubUrl} target="_blank" rel="noreferrer">GitHub<ArrowUpRight size={14} aria-hidden="true" /></a>
          <a href={author.zhihuUrl} target="_blank" rel="noreferrer">{t.zhihu}<ArrowUpRight size={14} aria-hidden="true" /></a>
          <a href={author.cnblogsUrl} target="_blank" rel="noreferrer">{t.cnblogs}<ArrowUpRight size={14} aria-hidden="true" /></a>
          <Link href={routes.about}>{t.aboutMe}</Link>
        </div> : null}
      </div>
      <a href="#home-top" className="inline-flex min-h-11 items-center gap-4 text-xs text-muted-foreground hover:text-primary">{t.backToTop}<ArrowUp aria-hidden="true" className="size-4" /></a>
    </div>
  </footer>
}
const navigations = { numbered: NumberedNavigation, compact: CompactNavigation }
export function HomeLayout({ children }: { children: ReactNode }) {
  const { t } = useI18n()
  const { config } = useHomeDesign()
  const Navigation = navigations[config.navigation]
  return <div data-home-layout={config.frame} id="home-top">
    <a href="#home-content" className="fixed top-4 left-4 z-50 -translate-y-[calc(100%+2rem)] bg-foreground px-4 py-3 text-background focus:translate-y-0">{t.skipToContent}</a>
    <div className={cn(styles.frame, config.frame === "sidebar" && styles.sidebar)}>
      <Navigation />
      <div className={styles.body}>
        <main id="home-content" tabIndex={-1}>{children}</main>
        <HomeFooter />
      </div>
    </div>
  </div>
}
