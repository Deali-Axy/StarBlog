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
export function FolioHeaderWordmark() {
  return <Link href={routes.home} className="inline-flex shrink-0 items-center" aria-label={site.name}>
    <SiteLogo />
  </Link>
}
export function FolioFooterWordmark() {
  return <Link href={routes.home} className="inline-flex shrink-0 items-center max-[39.9375rem]:w-full max-[39.9375rem]:justify-center" aria-label={site.name}>
    <SiteLogo />
  </Link>
}
export function FolioSiteNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks()
  return <nav aria-label={t.siteNavigation} className={"flex items-center justify-center gap-12 min-h-11 [border-top:1px_solid_var(--border)] [border-bottom:1px_solid_var(--foreground)] [&_a]:relative [&_a]:py-3 [&_a]:text-xs [&_a[aria-current=page]::after]:[content:''] [&_a[aria-current=page]::after]:absolute [&_a[aria-current=page]::after]:bottom-1 [&_a[aria-current=page]::after]:left-[50%] [&_a[aria-current=page]::after]:w-[3px] [&_a[aria-current=page]::after]:h-[3px] [&_a[aria-current=page]::after]:rounded-[50%] [&_a[aria-current=page]::after]:bg-primary [&_a:hover]:text-primary max-[39.9375rem]:gap-6 max-[39.9375rem]:min-h-11 max-[39.9375rem]:justify-between max-[39.9375rem]:[&_a]:text-[.6875rem]"}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined}>
      <span>{label}</span>
    </BlogStyleLink>)}
  </nav>
}
export function FolioHeaderTools() {
  const { t } = useI18n()
  return <div className={"flex items-center gap-1 ml-auto shrink-0 max-[39.9375rem]:gap-px"}>
    <Link href={routes.search} className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} aria-label={t.searchPosts} title={t.searchPosts}><Search className="size-4" /></Link>

    <ThemeToggle iconOnly className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} />
    <LanguageToggle className={"inline-flex items-center justify-center shrink-0 min-w-13 w-auto h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] px-1 text-[.6875rem] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-12"} />

  </div>
}
export function FolioFooterNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks().filter(({ href }) => href !== routes.home)
  return <nav aria-label={t.footerNavigation} className={"flex justify-center flex-wrap gap-8 [margin-block:32px_40px] text-xs [&_a:hover]:text-primary [&_a:hover]:[text-decoration:underline] [&_a:hover]:[text-underline-offset:5px] max-[39.9375rem]:gap-6 max-[39.9375rem]:[margin-block:28px] max-[39.9375rem]:text-[.6875rem]"}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href}>{label}</BlogStyleLink>)}
    <Link href={routes.rss}>RSS</Link>
  </nav>
}
export function FolioBackToTop() {
  const { t } = useI18n()
  return <button type="button" className={"inline-flex items-center justify-center gap-3 min-h-9.5 cursor-pointer text-[.6875rem] [&:hover]:[text-decoration:underline] [&:hover]:[text-underline-offset:4px]"} onClick={() => window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })}>
    {t.backToTop}<ArrowUp className="size-4" aria-hidden="true" />
  </button>
}
export function FolioCopyright() {
  return <span>© {new Date().getFullYear()} {site.authorName}</span>
}
