"use client"
import { useI18n } from "@/components/app-providers"
import { LanguageToggle } from "@/components/layout/language-toggle"
import { routes } from "@/lib/routes"
import { ArrowRight, Moon, Rss, Search, Sun } from "lucide-react"
import Link from "next/link"
import { useSyncExternalStore } from "react"
import { useHomeMode } from "../home-mode"
const subscribeMounted = () => () => { }
export function SidebarTools() {
  const { t } = useI18n()
  const { effectiveMode, changeMode } = useHomeMode()
  const mounted = useSyncExternalStore(subscribeMounted, () => true, () => false)
  return <div className={"flex items-center gap-0 sm:gap-2 min-[70rem]:mt-auto min-[70rem]:shrink-0 min-[70rem]:flex-col min-[70rem]:items-stretch min-[70rem]:gap-3 min-[70rem]:pt-8"}>
    <Link href={routes.search} className={"inline-flex min-h-11 min-w-11 items-center justify-center text-muted-foreground hover:text-primary min-[70rem]:justify-start min-[70rem]:gap-2.5 min-[70rem]:border-b min-[70rem]:border-border min-[70rem]:text-xs"} aria-label={t.searchPosts}>
      <Search aria-hidden="true" className="size-4" /><span className={"hidden min-[70rem]:block"}>{t.searchPosts}</span>
      {<ArrowRight aria-hidden="true" className="ml-auto hidden size-3.5 min-[70rem]:block" />}
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
