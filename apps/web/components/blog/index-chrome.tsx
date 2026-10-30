"use client"

import { ArrowUpRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { BlogBackToTop, BlogCopyright, BlogFooterNav, BlogHeaderTools, BlogSiteNav, BlogWordmark } from "@/components/blog/blog-chrome-shared"
import { routes } from "@/lib/routes"
import styles from "./blog-chrome.module.css"

export function IndexHeader() {
  const { t } = useI18n()
  return <header data-blog-header="library" className={styles.indexHeader}>
    <div className={styles.indexHeaderInner}>
      <div className={styles.indexBrand}><BlogWordmark /><span>{t.indexChromeLabel}</span></div>
      <BlogSiteNav numbered className={styles.indexNav} />
      <BlogHeaderTools rss />
    </div>
  </header>
}

export function IndexFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="library" className={styles.indexFooter}>
    <div className={styles.indexFooterInner}>
      <div className={styles.indexFooterLead}><p>{t.indexFooterHeadline}</p><a href={routes.feed}>{t.subscribe}<ArrowUpRight className="size-6" aria-hidden="true" /></a></div>
      <BlogFooterNav className={styles.indexFooterNav} />
      <div className={styles.indexFooterMeta}><BlogWordmark /><BlogCopyright /><BlogBackToTop /></div>
    </div>
  </footer>
}
