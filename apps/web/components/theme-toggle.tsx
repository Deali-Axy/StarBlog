"use client"

import { useI18n } from "@/components/app-providers"

import { useSyncExternalStore } from "react"
import { useTheme } from "next-themes"

import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const subscribe = () => () => {}

export function ThemeToggle({ className }: { className?: string }) {
  const { t } = useI18n()
  const { resolvedTheme, setTheme } = useTheme()
  const mounted = useSyncExternalStore(subscribe, () => true, () => false)

  return (
    <Button
      variant="ghost"
      size="sm"
      className={cn("h-8 min-w-8 px-2 text-xs tracking-[0.2em]", className)}
      aria-label={t.switchTheme}
      onClick={() => {
        const dark = document.documentElement.classList.contains("dark")
        setTheme(dark ? "light" : "dark")
      }}
    >
      {mounted ? (resolvedTheme === "dark" ? t.night : t.day) : null}
    </Button>
  )
}
