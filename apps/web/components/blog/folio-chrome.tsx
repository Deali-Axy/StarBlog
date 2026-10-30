"use client"

import { useI18n } from "@/components/app-providers"
import { BlogBackToTop, BlogCopyright, BlogFooterNav, BlogHeaderTools, BlogSiteNav, BlogWordmark } from "@/components/blog/blog-chrome-shared"
import { site } from "@/lib/site"
import styles from "./blog-chrome.module.css"

export function FolioHeader() {
  const { t } = useI18n()
  return <header data-blog-header="editorial" className={styles.folioHeader}>
    <div className={styles.folioHeaderInner}>
      <div className={styles.folioBrandRow}>
        <p className={styles.folioByline}>{site.authorName}<span>{t.folioChromeLabel}</span></p>
        <BlogWordmark className={styles.folioWordmark} />
        <BlogHeaderTools menu={false} />
      </div>
      <BlogSiteNav className={styles.folioNav} />
    </div>
  </header>
}

export function FolioFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="editorial" className={styles.folioFooter}>
    <div className={styles.folioFooterInner}>
      <span className={styles.folioEndMark} aria-hidden="true">✳</span>
      <p className={styles.folioFooterTitle}>{t.folioFooterHeadline}</p>
      <p className={styles.folioFooterNote}>{t.folioFooterNote}</p>
      <BlogFooterNav className={styles.folioFooterNav} />
      <div className={styles.folioFooterMeta}><BlogWordmark /><BlogCopyright /><BlogBackToTop /></div>
    </div>
  </footer>
}
