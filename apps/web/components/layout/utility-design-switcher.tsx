"use client"

import { useId } from "react"
import { useSearchParams } from "next/navigation"
import { useI18n } from "@/components/app-providers"
import { resolveUtilityVariant, utilityStyleHref, type UtilityVariant } from "@/lib/utility-variant"
import { cn } from "@/lib/utils"

export function useUtilityDesign() {
  const variant = resolveUtilityVariant(useSearchParams().get("uv"))

  function switchVariant(next: UtilityVariant) {
    window.history.pushState(null, "", utilityStyleHref(window.location.href, next))
  }

  return { variant, switchVariant }
}

export function UtilityDesignSwitcher({ value, onChange, className }: {
  value: UtilityVariant
  onChange: (value: UtilityVariant) => void
  className?: string
}) {
  const { t } = useI18n()
  const id = useId()

  return (
    <div className={cn("mx-auto flex w-full max-w-6xl justify-end px-4 pt-6", className)}>
      <div className="flex items-center gap-3 text-xs text-muted-foreground">
        <label htmlFor={id}>{t.utilityDesign.label}</label>
        <select id={id} value={value} onChange={(event) => onChange(resolveUtilityVariant(event.target.value))} className="min-h-10 border border-border bg-background px-3 text-sm text-foreground focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring">
          <option value="flat">{t.utilityDesign.flat}</option>
          <option value="editorial">{t.utilityDesign.editorial}</option>
        </select>
      </div>
    </div>
  )
}
