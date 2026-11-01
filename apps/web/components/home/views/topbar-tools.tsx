"use client"
import { useI18n } from "@/components/app-providers"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { routes } from "@/lib/routes"
import { Moon, Rss, Search, Sun } from "lucide-react"
import Link from "next/link"
import { useSyncExternalStore } from "react"
import { useHomeMode } from "../home-mode"
const subscribeMounted = () => () => { }
export function TopbarTools() {
  const { t } = useI18n()
  const { effectiveMode, changeMode } = useHomeMode()
  const mounted = useSyncExternalStore(subscribeMounted, () => true, () => false)
  return <div className={"flex items-center gap-0 sm:gap-2"}>
    <Link href={routes.search} className={"inline-flex min-h-11 min-w-11 items-center justify-center text-muted-foreground hover:text-primary"} aria-label={t.searchPosts}>
      <Search aria-hidden="true" className="size-4" /><span className={"hidden"}>{t.searchPosts}</span>

    </Link>
    <div className="flex items-center min-[70rem]:justify-between">
      <button type="button" className="grid size-11 cursor-pointer place-items-center hover:text-primary" aria-label={t.switchTheme} title={t.switchTheme} onClick={() => changeMode((effectiveMode ?? (document.documentElement.classList.contains("dark") ? "dark" : "light")) === "dark" ? "light" : "dark")}>
        {mounted && effectiveMode === "light" ? <Moon aria-hidden="true" className="size-5" /> : <Sun aria-hidden="true" className="size-5" />}
      </button>
      <LanguageToggle className="min-h-11 min-w-11" />
      <Link href={routes.rss} aria-label={t.subscribe} title={t.subscribe} className="hidden size-11 items-center justify-center sm:inline-flex"><Rss aria-hidden="true" className="size-4" /></Link>
    </div>
  </div>
}
