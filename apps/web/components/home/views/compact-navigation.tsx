"use client"
import { useI18n } from "@/components/app-providers"
import Link from "next/link"
import { Brand, useEntries } from "../home-brand"
import { TopbarTools } from "./topbar-tools"
export function CompactNavigation() {
  const { t } = useI18n()
  const entries = useEntries()
  return <header data-home-navigation="compact" className={"flex flex-wrap items-center justify-between gap-[1rem] [padding:1.5rem_1rem] [border-bottom:1px_solid_var(--border)] min-[40rem]:px-[1.5rem]"}>
    <Brand />
    <nav aria-label={t.siteNavigation} className={"[order:3] w-full [&_ul]:flex [&_ul]:flex-wrap [&_ul]:[gap:.25rem_1.5rem] [&_a]:inline-flex [&_a]:min-h-11 [&_a]:items-center [&_a]:text-[.875rem] min-[70rem]:[order:initial] min-[70rem]:w-auto min-[70rem]:ml-auto"}><ul>{entries.map((entry, index) => <li key={entry.href}><Link href={entry.href} aria-current={index === 0 ? "page" : undefined} className="hover:text-primary aria-[current=page]:text-primary">{entry.label}</Link></li>)}</ul></nav>
    <TopbarTools />
  </header>
}
