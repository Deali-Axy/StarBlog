"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"
import { usePathname } from "next/navigation"

import { StarMark } from "@/components/brand/star-mark"
import type { LinkResponse } from "@/lib/api/types"
import { isPhotographyPath } from "@/lib/photography/path"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"

export function SiteFooter({ links = [] }: { links?: LinkResponse[] }) {
  const { t } = useI18n()
  const darkroom = isPhotographyPath(usePathname())
  const muted = darkroom ? "text-white/55 hover:text-white" : "text-muted-foreground hover:text-foreground"
  return (
    <footer className={cn("border-t", darkroom ? "border-white/10 bg-black text-white" : "border-border/70")}>
      <div className="page-wrap flex flex-col gap-8 py-12 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="flex items-center gap-2 font-heading">
            <StarMark className="size-3.5 text-primary" />
            {site.name}
          </p>
          <p className={cn("mt-2 max-w-sm text-sm", darkroom ? "text-white/55" : "text-muted-foreground")}>{t.tagline}</p>
        </div>
        <div className={cn("flex flex-wrap gap-x-5 gap-y-2 text-sm", darkroom ? "text-white/55" : "text-muted-foreground")}>
          <Link href={routes.blog} className={muted}>
            {t.posts}
          </Link>
          <Link href={routes.photography} className={muted}>
            {t.photography}
          </Link>
          <Link href={routes.links} className={muted}>
            {t.links}
          </Link>
          <Link href={routes.about} className={muted}>
            {t.about}
          </Link>
          <Link href={routes.rss} className={muted}>
            RSS
          </Link>
        </div>
        {links.length > 0 ? (
          <div className="max-w-sm">
            <p className="kicker">{t.links}</p>
            <ul className={cn("mt-3 flex flex-wrap gap-x-4 gap-y-1 text-sm", darkroom ? "text-white/55" : "text-muted-foreground")}>
              {links.map((link) => (
                <li key={link.id}>
                  <a href={link.url} target="_blank" rel="noreferrer" className={muted}>
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </div>
    </footer>
  )
}
