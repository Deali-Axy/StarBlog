"use client"

import { useI18n } from "@/components/app-providers"

import { useSyncExternalStore } from "react"
import { useTheme } from "next-themes"
import { Moon, Sun } from "lucide-react"

import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const subscribe = () => () => {}

export function ThemeToggle({ className, iconOnly = false }: { className?: string; iconOnly?: boolean }) {
  const { t } = useI18n()
  const { resolvedTheme, setTheme } = useTheme()
  const mounted = useSyncExternalStore(subscribe, () => true, () => false)

  return (
    <Button
      variant="ghost"
      size="sm"
      className={cn("h-8 min-w-8 px-2 text-xs tracking-[0.2em]", className)}
      aria-label={t.switchTheme}
      title={t.switchTheme}
      onClick={() => {
        const dark = document.documentElement.classList.contains("dark")
        setTheme(dark ? "light" : "dark")
      }}
    >
      <Sun className={iconOnly ? "hidden size-5 dark:block" : "size-4 dark:hidden"} aria-hidden="true" />
      <Moon className={iconOnly ? "size-5 dark:hidden" : "hidden size-4 dark:block"} aria-hidden="true" />
      {!iconOnly && mounted ? (resolvedTheme === "dark" ? t.night : t.day) : null}
    </Button>
  )
}
