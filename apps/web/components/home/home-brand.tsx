"use client"
import { useI18n } from "@/components/app-providers"
import { SiteLogo } from "@/components/brand/site-logo"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import Link from "next/link"
export function useEntries() {
  const { t } = useI18n()
  return [
    { href: routes.home, label: t.home, caption: "COVER" },
    { href: routes.blog, label: t.posts, caption: "JOURNAL" },
    { href: routes.photography, label: t.photography, caption: "GALLERY" },
    { href: routes.links, label: t.links, caption: "CONNECTIONS" },
    { href: routes.about, label: t.about, caption: "PROFILE" },
  ]
}
export function Brand() {
  const { t } = useI18n()
  return <Link href={routes.home} className="inline-flex min-w-0 flex-col items-start gap-3" aria-label={`${site.name} · ${t.home}`}>
    <SiteLogo />
    <span className="font-sans text-[9px] font-normal tracking-[.2em] text-muted-foreground">{t.homeCoverLabel}</span>
  </Link>
}
