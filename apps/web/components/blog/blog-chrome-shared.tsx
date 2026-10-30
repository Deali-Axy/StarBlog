"use client"

import Link from "next/link"
import { useState } from "react"
import { Dialog } from "radix-ui"
import { ArrowUp, ArrowUpRight, Menu, Rss, Search, X } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { StarMark } from "@/components/brand/star-mark"
import { BlogStyleLink, useBlogLayout } from "@/components/blog/blog-layout-context"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { ThemeToggle } from "@/components/theme-toggle"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"
import styles from "./blog-chrome.module.css"

export function useBlogSiteLinks() {
  const { t } = useI18n()
  return [
    { href: routes.home, label: t.home },
    { href: routes.blog, label: t.posts },
    { href: routes.photography, label: t.photography },
    { href: routes.links, label: t.links },
    { href: routes.about, label: t.about },
  ]
}

export function BlogWordmark({ className }: { className?: string }) {
  return <Link href={routes.home} className={cn(styles.wordmark, className)} aria-label={site.name}>
    <StarMark className={styles.brandMark} /><span>{site.name}</span>
  </Link>
}

export function BlogSiteNav({ className, numbered = false }: { className?: string; numbered?: boolean }) {
  const { t } = useI18n()
  const links = useBlogSiteLinks()
  return <nav aria-label={t.siteNavigation} className={className}>
    {links.map(({ href, label }, index) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined}>
      {numbered ? <small aria-hidden="true">0{index + 1}</small> : null}<span>{label}</span>
    </BlogStyleLink>)}
  </nav>
}

export function BlogHeaderTools({ menu = true, rss = false }: { menu?: boolean; rss?: boolean }) {
  const { t } = useI18n()
  return <div className={styles.headerTools}>
    <Link href={routes.search} className={styles.toolButton} aria-label={t.searchPosts} title={t.searchPosts}><Search className="size-4" /></Link>
    {rss ? <a href={routes.feed} className={cn(styles.toolButton, styles.desktopRss)} aria-label="RSS" title="RSS"><Rss className="size-4" /></a> : null}
    <ThemeToggle iconOnly className={styles.toolButton} />
    <LanguageToggle className={styles.languageButton} />
    {menu ? <BlogMobileNav /> : null}
  </div>
}

function BlogMobileNav() {
  const { t } = useI18n()
  const { variant } = useBlogLayout()
  const links = useBlogSiteLinks()
  const [open, setOpen] = useState(false)
  return <Dialog.Root open={open} onOpenChange={setOpen}>
    <Dialog.Trigger asChild><button type="button" className={cn(styles.toolButton, styles.menuTrigger)} aria-label={t.siteNavigation}><Menu className="size-5" /></button></Dialog.Trigger>
    <Dialog.Portal>
      <Dialog.Overlay className={styles.menuOverlay} />
      <Dialog.Content aria-describedby={undefined} data-blog-menu={variant} className={cn(blogThemeClassName(variant), styles.mobileMenu)}>
        <div className={styles.menuHeading}><Dialog.Title>{t.siteNavigation}</Dialog.Title><Dialog.Close asChild><button type="button" aria-label={t.close} className={styles.toolButton}><X className="size-5" /></button></Dialog.Close></div>
        <nav aria-label={t.siteNavigation} className={styles.menuLinks}>
          {links.map(({ href, label }, index) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined} onClick={() => setOpen(false)}>
            <small aria-hidden="true">0{index + 1}</small><span>{label}</span><ArrowUpRight className="size-5" aria-hidden="true" />
          </BlogStyleLink>)}
        </nav>
        <p className={styles.menuNote}>{site.name} / {site.authorName}</p>
      </Dialog.Content>
    </Dialog.Portal>
  </Dialog.Root>
}

export function BlogFooterNav({ className }: { className?: string }) {
  const { t } = useI18n()
  const links = useBlogSiteLinks().filter(({ href }) => href !== routes.home)
  return <nav aria-label={t.footerNavigation} className={className}>
    {links.map(({ href, label }) => <BlogStyleLink key={href} href={href}>{label}</BlogStyleLink>)}
    <Link href={routes.rss}>RSS</Link>
  </nav>
}

export function BlogBackToTop({ className }: { className?: string }) {
  const { t } = useI18n()
  return <button type="button" className={cn(styles.backToTop, className)} onClick={() => window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })}>
    {t.backToTop}<ArrowUp className="size-4" aria-hidden="true" />
  </button>
}

export function BlogCopyright() {
  return <span>© {new Date().getFullYear()} {site.authorName}</span>
}
