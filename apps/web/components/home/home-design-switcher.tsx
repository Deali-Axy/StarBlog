"use client"

import { Check, Palette } from "lucide-react"
import { DropdownMenu } from "radix-ui"

import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { FEATURED_CATEGORIES_VARIANT_OPTIONS, parseFeaturedCategoriesVariant } from "@/lib/featured-categories-variant"
import { HOME_VARIANT_OPTIONS, parseHomeVariant } from "@/lib/home-variant"
import { cn } from "@/lib/utils"
import theme from "./home-layout.module.css"
import styles from "./home-design-switcher.module.css"

export function HomeDesignSwitcher() {
  const { t, locale } = useI18n()
  const { variant, categories, update } = useHomeDesign()
  return <DropdownMenu.Root modal={false}>
    <DropdownMenu.Trigger asChild>
      <button type="button" data-home-design-trigger aria-label={t.homeCustomize} title={t.homeCustomize} className={styles.trigger}>
        <Palette size={20} aria-hidden="true" />
      </button>
    </DropdownMenu.Trigger>
    <DropdownMenu.Portal>
      <DropdownMenu.Content data-home-design-menu className={cn(theme.theme, styles.menu)} aria-label={t.homeCustomize}
        side="top" align="end" sideOffset={12} collisionPadding={16}>
        <DropdownMenu.Label className={styles.heading}>{t.homeCustomize}</DropdownMenu.Label>
        <DropdownMenu.Label className={styles.label}>{t.homeContentDesign}</DropdownMenu.Label>
        <DropdownMenu.RadioGroup aria-label={t.homeContentDesign} value={variant} onValueChange={value => {
          const next = parseHomeVariant(value)
          if (next) update({ variant: next })
        }}>
          {HOME_VARIANT_OPTIONS.map(option => <DropdownMenu.RadioItem key={option.id} value={option.id} className={styles.option}>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenu.ItemIndicator className={styles.indicator}><Check size={16} aria-hidden="true" /></DropdownMenu.ItemIndicator>
          </DropdownMenu.RadioItem>)}
        </DropdownMenu.RadioGroup>
        <DropdownMenu.Separator className={styles.separator} />
        <DropdownMenu.Label className={styles.label}>{t.featuredCategories}</DropdownMenu.Label>
        <DropdownMenu.RadioGroup aria-label={t.featuredCategories} value={categories} onValueChange={value => {
          const next = parseFeaturedCategoriesVariant(value)
          if (next) update({ categories: next })
        }}>
          {FEATURED_CATEGORIES_VARIANT_OPTIONS.map(option => <DropdownMenu.RadioItem key={option.id} value={option.id} className={styles.option}>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenu.ItemIndicator className={styles.indicator}><Check size={16} aria-hidden="true" /></DropdownMenu.ItemIndicator>
          </DropdownMenu.RadioItem>)}
        </DropdownMenu.RadioGroup>
      </DropdownMenu.Content>
    </DropdownMenu.Portal>
  </DropdownMenu.Root>
}
