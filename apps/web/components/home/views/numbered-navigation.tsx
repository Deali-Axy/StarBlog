"use client"
import { useI18n } from "@/components/app-providers"
import { ArrowUpRight } from "lucide-react"
import Link from "next/link"
import { Brand, useEntries } from "../home-brand"
import { SidebarTools } from "./sidebar-tools"
export function NumberedNavigation() {
  const { t } = useI18n()
  const entries = useEntries()
  return <header data-home-navigation="numbered" className="flex flex-wrap items-center justify-between gap-x-2 gap-y-6 px-4 pt-7 sm:gap-x-6 sm:px-0 min-[70rem]:sticky min-[70rem]:top-0 min-[70rem]:h-dvh min-[70rem]:flex-col min-[70rem]:flex-nowrap min-[70rem]:items-stretch min-[70rem]:justify-start min-[70rem]:gap-0 min-[70rem]:self-start min-[70rem]:overflow-y-auto min-[70rem]:border-r min-[70rem]:border-border min-[70rem]:pt-13 min-[70rem]:pr-7 min-[70rem]:pb-6 [@media(min-width:1120px)_and_(max-height:740px)]:pt-6">
    <Brand />
    <nav aria-label={t.siteNavigation} className="order-3 w-full min-[70rem]:order-none min-[70rem]:mt-16 min-[70rem]:shrink-0 [@media(min-width:1120px)_and_(max-height:740px)]:mt-6">
      <p className="mb-4 hidden justify-between text-[10px] tracking-[.2em] text-muted-foreground min-[70rem]:flex">{t.index}<span aria-hidden="true">↙</span></p>
      <ul className="grid grid-cols-2 gap-x-6 sm:grid-cols-4 min-[70rem]:block">
        {entries.map((entry, index) => <li key={entry.href} className={index === 0 ? "hidden min-[70rem]:block" : undefined}>
          <Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className="group flex min-h-14 items-center gap-3 border-t border-border py-3 hover:text-primary min-[70rem]:min-h-15.5 aria-[current=page]:text-primary">
            <span className="mt-1 self-start font-mono text-[10px] text-muted-foreground" aria-hidden="true">{String(index).padStart(2, "0")}</span>
            <span className="min-w-0"><span className="font-heading text-lg wrap-anywhere min-[70rem]:text-base">{entry.label}</span><span className="mt-1.5 hidden font-mono text-[8px] tracking-[.12em] text-muted-foreground min-[70rem]:block" aria-hidden="true">{entry.caption}</span></span>
            <ArrowUpRight aria-hidden="true" className="ml-auto size-4 shrink-0 text-muted-foreground max-[359px]:hidden" />
          </Link>
        </li>)}
      </ul>
    </nav>
    <SidebarTools />
  </header>
}
