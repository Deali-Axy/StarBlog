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
import { pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"
import styles from "./home-layout.module.css"

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
    <div data-home-layout className={styles.layout} id="home-top">
      <a href="#home-content" className={styles.skipLink}>{t.skipToContent}</a>
      <div className={styles.frame}>
        <header className={styles.sidebar}>
          <Link href={routes.home} className={styles.brand} aria-label={`${site.name} · ${t.home}`}>
            <StarMark className={styles.mark} />
            <span>{site.name}<span className={styles.brandNote}>{t.homeCoverLabel}</span></span>
          </Link>

          <nav aria-label={t.siteNavigation} className={styles.navigation}>
            <p className={styles.indexLabel}>{t.index}<span aria-hidden="true">↙</span></p>
            <ul className={styles.entries}>
              {entries.map((entry, index) => (
                <li key={entry.href} data-cover={index === 0 || undefined}>
                  <Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className={styles.entry}>
                    <span className={styles.number} aria-hidden="true">{String(index).padStart(2, "0")}</span>
                    <span className={styles.entryText}>
                      <span className={styles.entryLabel}>{entry.label}</span>
                      <span className={styles.entryCaption} aria-hidden="true">{entry.caption}</span>
                    </span>
                    <ArrowUpRight aria-hidden="true" className={styles.entryArrow} />
                  </Link>
                </li>
              ))}
            </ul>
          </nav>

          <div className={styles.utilities}>
            <Link href={routes.search} className={styles.search} aria-label={t.searchPosts}>
              <Search aria-hidden="true" className="size-4" />
              <span>{t.searchPosts}</span>
              <ArrowRight aria-hidden="true" className="ml-auto size-3.5" />
            </Link>
            <div className={styles.preferences}>
              <ThemeToggle iconOnly />
              <LanguageToggle />
              <Link href={routes.rss} aria-label={t.subscribe} title={t.subscribe} className={styles.rss}>
                <Rss aria-hidden="true" className="size-4" />
              </Link>
            </div>
          </div>
        </header>

        <div className={styles.body}>
          <main id="home-content" tabIndex={-1}>{children}</main>
          <footer className={cn(pageContainerClassName, styles.footer)}>
            <div className={styles.footerInner}>
              <div>
                <p className={styles.footerTitle}>{t.homeFooterHeadline}<span aria-hidden="true">＊</span></p>
                <p className={styles.footerNote}>{site.name} / {t.hero}</p>
              </div>
              <a href="#home-top" className={styles.backToTop}>{t.backToTop}<ArrowUp aria-hidden="true" className="size-4" /></a>
            </div>
          </footer>
        </div>
      </div>
    </div>
  )
}
