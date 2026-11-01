"use client"
import { useI18n } from "@/components/app-providers"
import { DropdownMenuContent, DropdownMenuItemIndicator, DropdownMenuLabel, DropdownMenuPortal, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { blogThemeClassName } from "@/lib/blog-theme"
import { BLOG_VARIANT_OPTIONS, type BlogVariant } from "@/lib/blog-variant"
import { cn } from "@/lib/utils"
import { Check, Palette } from "lucide-react"
import type { BlogPreviewProps } from "../blog-preview-props"
export function FolioPreview({ value, onValueChange, onCloseAutoFocus }: BlogPreviewProps) {
  const { t, locale } = useI18n()
  return <>    <DropdownMenuTrigger asChild>
    <button type="button" aria-label={t.designPreview} title={t.designPreview} data-blog-preview-trigger data-variant="editorial" className={"fixed [z-index:45] [right:max(20px,_env(safe-area-inset-right))] [bottom:calc(80px_+_env(safe-area-inset-bottom))] grid [place-items:center] w-12 h-12 [border:1px_solid_var(--foreground)] rounded-[50%] bg-card text-primary [box-shadow:0_3px_16px_#0002] cursor-pointer [border-color:var(--primary)] [&:hover]:[filter:brightness(.95)]"}>
      <Palette size={20} aria-hidden="true" />
    </button>
  </DropdownMenuTrigger>
    <DropdownMenuPortal>
      <DropdownMenuContent aria-label={t.designPreview} side="top" align="end" sideOffset={12} collisionPadding={16} onCloseAutoFocus={onCloseAutoFocus} data-blog-preview-menu data-variant="editorial" className={cn(blogThemeClassName("editorial"), "[z-index:50] w-[232px] [max-width:calc(100vw_-_32px)] p-3 border border-border bg-card text-foreground [box-shadow:0_8px_32px_#0003] outline-none rounded-[.75rem]")}>
        <DropdownMenuLabel className={"[padding:8px_10px_12px] text-[.625rem] text-muted-foreground [letter-spacing:.08em]"}>{t.designPreview}</DropdownMenuLabel>
        <DropdownMenuRadioGroup value={value} onValueChange={(value) => onValueChange(value as BlogVariant)}>
          {BLOG_VARIANT_OPTIONS.map((option, index) => <DropdownMenuRadioItem key={option.id} value={option.id} className={"flex items-center gap-3 min-h-11 p-2.5 text-[.8125rem] cursor-pointer outline-none [&_small]:[font:10px_ui-monospace,_monospace] [&_small]:text-muted-foreground [&[data-state=checked]]:text-primary [&[data-highlighted]]:bg-accent [&[data-highlighted]]:text-foreground"}>
            <small aria-hidden="true">0{index + 1}</small>
            <span>{locale === "zh" ? option.labelZh : option.labelEn}</span>
            <DropdownMenuItemIndicator className={"inline-flex ml-auto"}><Check size={16} aria-hidden="true" /></DropdownMenuItemIndicator>
          </DropdownMenuRadioItem>)}
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenuPortal></>
}
