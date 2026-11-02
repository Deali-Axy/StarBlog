"use client"

import { useRef, useState } from "react"
import { useSearchParams } from "next/navigation"
import { Check, Palette } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItemIndicator, DropdownMenuLabel, DropdownMenuPortal, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { resolveUtilityVariant, utilityStyleHref, type UtilityVariant } from "@/lib/utility-variant"

export function useUtilityDesign() {
  const variant = resolveUtilityVariant(useSearchParams().get("uv"))

  function switchVariant(next: UtilityVariant) {
    window.history.pushState(null, "", utilityStyleHref(window.location.href, next))
  }

  return { variant, switchVariant }
}

export function UtilityDesignSwitcher({ value, onChange }: {
  value: UtilityVariant
  onChange: (value: UtilityVariant) => void
}) {
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  const triggerRef = useRef<HTMLButtonElement>(null)

  return (
    <DropdownMenu modal={false} open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <button ref={triggerRef} type="button" aria-label={t.designPreview} title={t.designPreview} className="fixed z-45 grid size-12 cursor-pointer place-items-center border border-border bg-card text-foreground [right:max(20px,env(safe-area-inset-right))] [bottom:calc(80px_+_env(safe-area-inset-bottom))] hover:bg-muted focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-ring">
          <Palette size={20} aria-hidden="true" />
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuPortal>
        <DropdownMenuContent aria-label={t.designPreview} side="top" align="end" sideOffset={12} collisionPadding={16} onCloseAutoFocus={(event) => { event.preventDefault(); triggerRef.current?.focus() }} className="z-50 w-58 max-w-[calc(100vw-32px)] border border-border bg-card p-2 text-foreground shadow-lg">
          <DropdownMenuLabel className="px-2.5 pt-2 pb-3 text-xs text-muted-foreground">{t.utilityDesign.label}</DropdownMenuLabel>
          <DropdownMenuRadioGroup value={value} onValueChange={(next) => { onChange(resolveUtilityVariant(next)); setOpen(false) }}>
            {(["flat", "editorial"] as const).map((style) => (
              <DropdownMenuRadioItem key={style} value={style} className="flex min-h-11 cursor-pointer items-center gap-3 px-2.5 py-2 text-sm outline-none data-[state=checked]:text-primary data-highlighted:bg-accent data-highlighted:text-foreground">
                <span>{t.utilityDesign[style]}</span>
                <DropdownMenuItemIndicator className="ml-auto inline-flex"><Check size={16} aria-hidden="true" /></DropdownMenuItemIndicator>
              </DropdownMenuRadioItem>
            ))}
          </DropdownMenuRadioGroup>
        </DropdownMenuContent>
      </DropdownMenuPortal>
    </DropdownMenu>
  )
}
