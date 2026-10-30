"use client"

import { useI18n } from "@/components/app-providers"
import { useBlogLayout } from "@/components/blog/blog-layout-context"
import { BLOG_PREVIEW, BLOG_VARIANT_OPTIONS, type BlogVariant } from "@/lib/blog-variant"
import styles from "./blog-designs.module.css"

export function BlogDesignSwitcher() {
  const { t, locale } = useI18n()
  const { variant } = useBlogLayout()
  if (!BLOG_PREVIEW) return null
  function switchVariant(next: BlogVariant) {
    const url = new URL(window.location.href)
    url.searchParams.set("bv", next)
    window.history.pushState(null, "", `${url.pathname}${url.search}${url.hash}`)
  }
  return <div className={styles.previewBar}>
    <span className={styles.previewLabel}>{t.designPreview}</span>
    <div role="group" aria-label={t.designPreview} className={styles.previewOptions}>
      {BLOG_VARIANT_OPTIONS.map((option, index) => <button key={option.id} type="button" aria-pressed={variant === option.id}
        onClick={() => switchVariant(option.id)} className={styles.previewOption}>
        <small aria-hidden="true">0{index + 1}</small>{locale === "zh" ? option.labelZh : option.labelEn}
      </button>)}
    </div>
  </div>
}
