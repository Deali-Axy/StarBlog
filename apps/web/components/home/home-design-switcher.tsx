"use client"

import { Check, Palette } from "lucide-react"
import { DropdownMenu } from "radix-ui"

import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { FEATURED_CATEGORIES_VARIANT_OPTIONS, parseFeaturedCategoriesVariant } from "@/lib/featured-categories-variant"
import { HOME_VARIANT_OPTIONS, parseHomeVariant } from "@/lib/home-variant"
import { homeThemeClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const optionClassName = "flex min-h-11 cursor-pointer items-center gap-3 rounded p-2.5 text-[13px] outline-none data-[state=checked]:text-primary data-highlighted:bg-accent data-highlighted:text-foreground"
const labelClassName = "px-2.5 py-2 text-[11px] tracking-[.06em] text-muted-foreground"

export function HomeDesignSwitcher() {
  const { t, locale } = useI18n()
  const { variant, categories, update } = useHomeDesign()
  return <DropdownMenu.Root modal={false}>
    <DropdownMenu.Trigger asChild>
      <button type="button" data-home-design-trigger aria-label={t.homeCustomize} title={t.homeCustomize} className="fixed right-[max(20px,env(safe-area-inset-right))] bottom-[calc(24px+env(safe-area-inset-bottom))] z-45 grid size-12 cursor-pointer place-items-center rounded-full border border-primary bg-card text-primary shadow-[0_4px_18px_#0002] hover:bg-accent">
        <Palette size={20} aria-hidden="true" />
      </button>
    </DropdownMenu.Trigger>
    <DropdownMenu.Portal>
      <DropdownMenu.Content data-home-design-menu className={cn(homeThemeClassName, "z-50 max-h-(--radix-dropdown-menu-content-available-height) w-64 max-w-[calc(100vw-32px)] overflow-y-auto rounded-[.75rem] border border-border bg-card p-3 text-foreground shadow-[0_8px_32px_#0003]")} aria-label={t.homeCustomize}
        side="top" align="end" sideOffset={12} collisionPadding={16}>
        <DropdownMenu.Label className="px-2.5 pt-2 pb-3.5 font-heading text-lg">{t.homeCustomize}</DropdownMenu.Label>
        <DropdownMenu.Label className={labelClassName}>{t.homeContentDesign}</DropdownMenu.Label>
        <DropdownMenu.RadioGroup aria-label={t.homeContentDesign} value={variant} onValueChange={value => {
          const next = parseHomeVariant(value)
          if (next) update({ variant: next })
        }}>
          {HOME_VARIANT_OPTIONS.map(option => <DropdownMenu.RadioItem key={option.id} value={option.id} className={optionClassName}>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenu.ItemIndicator className="ml-auto inline-flex"><Check size={16} aria-hidden="true" /></DropdownMenu.ItemIndicator>
          </DropdownMenu.RadioItem>)}
        </DropdownMenu.RadioGroup>
        <DropdownMenu.Separator className="my-2.5 h-px bg-border" />
        <DropdownMenu.Label className={labelClassName}>{t.featuredCategories}</DropdownMenu.Label>
        <DropdownMenu.RadioGroup aria-label={t.featuredCategories} value={categories} onValueChange={value => {
          const next = parseFeaturedCategoriesVariant(value)
          if (next) update({ categories: next })
        }}>
          {FEATURED_CATEGORIES_VARIANT_OPTIONS.map(option => <DropdownMenu.RadioItem key={option.id} value={option.id} className={optionClassName}>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenu.ItemIndicator className="ml-auto inline-flex"><Check size={16} aria-hidden="true" /></DropdownMenu.ItemIndicator>
          </DropdownMenu.RadioItem>)}
        </DropdownMenu.RadioGroup>
      </DropdownMenu.Content>
    </DropdownMenu.Portal>
  </DropdownMenu.Root>
}
