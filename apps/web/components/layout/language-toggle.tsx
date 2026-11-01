"use client"

import { Languages } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import { useHydrated } from "@/components/ui/use-hydrated"
import { localeCookie } from "@/lib/i18n"

export function LanguageToggle({ className }: { className?: string }) {
  const { locale, t } = useI18n()
  const hydrated = useHydrated()

  function switchLanguage() {
    const next = locale === "zh" ? "en" : "zh"
    document.cookie = `${localeCookie}=${next}; Path=/; Max-Age=31536000; SameSite=Lax${location.protocol === "https:" ? "; Secure" : ""}`
    const url = new URL(window.location.href)
    url.searchParams.delete("lang")
    window.location.assign(url.toString())
  }

  return (
    <Button disabled={!hydrated} variant="ghost" size="sm" onClick={switchLanguage} aria-label={t.switchLanguage} title={t.switchLanguage} className={className}>
      <Languages className="size-4" aria-hidden="true" />
      {locale === "zh" ? "EN" : "中文"}
    </Button>
  )
}
