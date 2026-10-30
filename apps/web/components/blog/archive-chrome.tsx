"use client"

import { usePathname } from "next/navigation"
import { useI18n } from "@/components/app-providers"
import { BlogBackToTop, BlogCopyright, BlogFooterNav, BlogHeaderTools, BlogSiteNav, BlogWordmark } from "@/components/blog/blog-chrome-shared"
import styles from "./blog-chrome.module.css"

export function ArchiveHeader() {
  const { t } = useI18n()
  const pathname = usePathname()
  return <header data-blog-header="studio" className={styles.archiveHeader}>
    <div className={styles.archiveHeaderInner}>
      <div className={styles.archiveBrand}><BlogWordmark /><span>{pathname === "/blog" ? t.archiveChromeIndex : t.archiveChromeReading}</span></div>
      <BlogSiteNav className={styles.archiveNav} />
      <BlogHeaderTools />
    </div>
  </header>
}

export function ArchiveFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="studio" className={styles.archiveFooter}>
    <div className={styles.archiveFooterInner}>
      <div className={styles.archiveFooterStatus}><span aria-hidden="true" />{t.archiveWorkbench}<BlogCopyright /></div>
      <BlogFooterNav className={styles.archiveFooterNav} />
      <BlogBackToTop />
    </div>
  </footer>
}
