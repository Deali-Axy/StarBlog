"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink } from "@/components/blog/blog-layout-context"
import { StarMark } from "@/components/brand/star-mark"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { ThemeToggle } from "@/components/theme-toggle"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { ArrowUp, Rss, Search } from "lucide-react"
import Link from "next/link"
import { useBlogSiteLinks } from "../blog-site-links"
import { IndexMobileNav } from "./index-mobile-nav"
export function IndexHeaderWordmark() {
  return <Link href={routes.home} className={"inline-flex items-center gap-2.5 whitespace-nowrap text-[1.75rem] [line-height:1] font-bold [letter-spacing:-.05em] max-[39.9375rem]:text-[1.375rem] max-[39.9375rem]:gap-2"} aria-label={site.name}>
    <StarMark className={"w-5.5 h-5.5 shrink-0 max-[39.9375rem]:w-[17px] max-[39.9375rem]:h-[17px]"} /><span>{site.name}</span>
  </Link>
}
export function IndexFooterWordmark() {
  return <Link href={routes.home} className={"inline-flex items-center gap-2.5 whitespace-nowrap text-[1.75rem] [line-height:1] font-bold [letter-spacing:-.05em] [&&]:text-xl max-[39.9375rem]:text-[1.375rem] max-[39.9375rem]:gap-2 max-[39.9375rem]:[&&]:text-lg"} aria-label={site.name}>
    <StarMark className={"w-5.5 h-5.5 shrink-0 [&&]:w-4 [&&]:h-4 max-[39.9375rem]:w-[17px] max-[39.9375rem]:h-[17px]"} /><span>{site.name}</span>
  </Link>
}
export function IndexSiteNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks()
  return <nav aria-label={t.siteNavigation} className={"flex items-center justify-center [flex:1] gap-6.5 [&_a]:flex [&_a]:items-baseline [&_a]:gap-1.5 [&_a]:py-3 [&_a]:[border-bottom:2px_solid_transparent] [&_a]:text-xs [&_a]:whitespace-nowrap [&_small]:[font:8px_ui-monospace,_monospace] [&_small]:text-muted-foreground [&_a[aria-current=page]]:[border-color:var(--foreground)] [&_a[aria-current=page]_span]:bg-highlight [&_a[aria-current=page]_span]:[color:#202719] [&_a[aria-current=page]_span]:[padding:3px_6px] [&_a:hover]:[border-color:var(--border)] max-[74.9375rem]:gap-4.5 max-[63.9375rem]:hidden"}>
    {links.map(({ href, label }, index) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined}>
      {<small aria-hidden="true">0{index + 1}</small>}<span>{label}</span>
    </BlogStyleLink>)}
  </nav>
}
export function IndexHeaderTools() {
  const { t } = useI18n()
  return <div className={"flex items-center gap-1 ml-auto shrink-0 max-[39.9375rem]:gap-px"}>
    <Link href={routes.search} className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} aria-label={t.searchPosts} title={t.searchPosts}><Search className="size-4" /></Link>
    {<a href={routes.feed} className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5 max-[39.9375rem]:hidden"} aria-label="RSS" title="RSS"><Rss className="size-4" /></a>}
    <ThemeToggle iconOnly className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"} />
    <LanguageToggle className={"inline-flex items-center justify-center shrink-0 min-w-13 w-auto h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] px-1 text-[.6875rem] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-12"} />
    {<IndexMobileNav />}
  </div>
}
export function IndexFooterNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks().filter(({ href }) => href !== routes.home)
  return <nav aria-label={t.footerNavigation} className={"flex flex-col items-stretch gap-0 [&_a]:py-3 [&_a]:[border-bottom:1px_solid_color-mix(in_srgb,_var(--footer-foreground)_25%,_transparent)] [&_a]:text-[.8125rem] [&_a:hover]:text-highlight max-[39.9375rem]:grid max-[39.9375rem]:[grid-template-columns:1fr_1fr] max-[39.9375rem]:gap-x-6 max-[39.9375rem]:[&_a]:text-xs"}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href}>{label}</BlogStyleLink>)}
    <Link href={routes.rss}>RSS</Link>
  </nav>
}
export function IndexBackToTop() {
  const { t } = useI18n()
  return <button type="button" className={"[&&]:ml-auto inline-flex items-center justify-center gap-3 min-h-9.5 cursor-pointer text-[.6875rem] [&:hover]:[text-decoration:underline] [&:hover]:[text-underline-offset:4px] max-[39.9375rem]:[&&]:w-full max-[39.9375rem]:[&&]:ml-0 max-[39.9375rem]:[&&]:justify-start"} onClick={() => window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })}>
    {t.backToTop}<ArrowUp className="size-4" aria-hidden="true" />
  </button>
}
export function IndexCopyright() {
  return <span>© {new Date().getFullYear()} {site.authorName}</span>
}
