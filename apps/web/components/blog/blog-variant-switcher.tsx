"use client"

import { useI18n } from "@/components/app-providers"
import { useBlogLayout } from "@/components/blog/blog-layout-context"
import { Check, Palette } from "lucide-react"
import { DropdownMenu } from "radix-ui"
import { BLOG_PREVIEW, BLOG_VARIANT_OPTIONS, type BlogVariant } from "@/lib/blog-variant"
import { blogThemeClassName } from "@/lib/blog-theme"
import { cn } from "@/lib/utils"
import styles from "./blog-preview.module.css"

export function BlogDesignSwitcher() {
  const { t, locale } = useI18n()
  const { variant } = useBlogLayout()
  if (!BLOG_PREVIEW) return null
  function switchVariant(next: BlogVariant) {
    const url = new URL(window.location.href)
    url.searchParams.set("bv", next)
    window.history.pushState(null, "", `${url.pathname}${url.search}${url.hash}`)
  }
  return <DropdownMenu.Root modal={false}>
    <DropdownMenu.Trigger asChild>
      <button type="button" aria-label={t.designPreview} title={t.designPreview}
        data-blog-preview-trigger data-variant={variant} className={styles.trigger}>
        <Palette size={20} aria-hidden="true" />
      </button>
    </DropdownMenu.Trigger>
    <DropdownMenu.Portal>
      <DropdownMenu.Content aria-label={t.designPreview} side="top" align="end" sideOffset={12} collisionPadding={16}
        data-blog-preview-menu data-variant={variant} className={cn(blogThemeClassName(variant), styles.menu)}>
        <DropdownMenu.Label className={styles.label}>{t.designPreview}</DropdownMenu.Label>
        <DropdownMenu.RadioGroup value={variant} onValueChange={(value) => switchVariant(value as BlogVariant)}>
          {BLOG_VARIANT_OPTIONS.map((option, index) => <DropdownMenu.RadioItem key={option.id} value={option.id} className={styles.option}>
            <small aria-hidden="true">0{index + 1}</small>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenu.ItemIndicator className={styles.indicator}><Check size={16} aria-hidden="true" /></DropdownMenu.ItemIndicator>
          </DropdownMenu.RadioItem>)}
        </DropdownMenu.RadioGroup>
      </DropdownMenu.Content>
    </DropdownMenu.Portal>
  </DropdownMenu.Root>
}
