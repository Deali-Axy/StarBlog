"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink } from "@/components/blog/blog-layout-context"
import { SiteLogo } from "@/components/brand/site-logo"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { ThemeToggle } from "@/components/theme-toggle"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { ArrowUp, Search } from "lucide-react"
import Link from "next/link"
import { useBlogSiteLinks } from "../blog-site-links"
import { ArchiveMobileNav } from "./archive-mobile-nav"
export function ArchiveHeaderWordmark() {
  return <Link href={routes.home} className="inline-flex shrink-0 items-center" aria-label={site.name}>
    <SiteLogo />
  </Link>
}
export function ArchiveFooterWordmark() {
  return <Link href={routes.home} className="inline-flex shrink-0 items-center" aria-label={site.name}>
    <SiteLogo />
  </Link>
}
export function ArchiveSiteNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks()
  return <nav aria-label={t.siteNavigation} className={"flex items-stretch justify-center ml-auto [&_a]:flex [&_a]:items-center [&_a]:px-4.5 [&_a]:text-[.6875rem] [&_a]:[border-inline:1px_solid_transparent] [&_a[aria-current=page]]:bg-accent [&_a[aria-current=page]]:[border-color:var(--border)] [&_a[aria-current=page]]:[box-shadow:inset_0_-2px_0_var(--primary)] [&_a:hover]:bg-muted max-[74.9375rem]:[&_a]:px-3.5 max-[63.9375rem]:hidden"}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined}>
      <span>{label}</span>
    </BlogStyleLink>)}
  </nav>
}
export function ArchiveHeaderTools() {
  const { t } = useI18n()
  return <div className={"flex items-center gap-1 ml-auto shrink-0 [&&]:ml-0 max-[63.9375rem]:[&&]:ml-auto max-[39.9375rem]:gap-px"}>
    <Link href={routes.search} className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} aria-label={t.searchPosts} title={t.searchPosts}><Search className="size-4" /></Link>

    <ThemeToggle iconOnly className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} />
    <LanguageToggle className={"inline-flex items-center justify-center shrink-0 min-w-13 w-auto h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] px-1 text-[.6875rem] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-12"} />
    {<ArchiveMobileNav />}
  </div>
}
export function ArchiveFooterNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks().filter(({ href }) => href !== routes.home)
  return <nav aria-label={t.footerNavigation} className={"flex flex-wrap items-center gap-5 text-muted-foreground [&_a:hover]:text-foreground max-[39.9375rem]:gap-4"}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href}>{label}</BlogStyleLink>)}
    <Link href={routes.rss}>RSS</Link>
  </nav>
}
export function ArchiveBackToTop() {
  const { t } = useI18n()
  return <button type="button" className={"inline-flex items-center justify-center gap-3 min-h-9.5 cursor-pointer text-[.6875rem] [&:hover]:[text-decoration:underline] [&:hover]:[text-underline-offset:4px] [&&]:[font:inherit] max-[39.9375rem]:[&&]:text-[.5625rem] max-[39.9375rem]:[&&]:min-h-7.5"} onClick={() => window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })}>
    {t.backToTop}<ArrowUp className="size-4" aria-hidden="true" />
  </button>
}
export function ArchiveCopyright() {
  return <span>© {new Date().getFullYear()} {site.authorName}</span>
}
