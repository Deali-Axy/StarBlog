"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import { StarMark } from "@/components/brand/star-mark"
import type { LinkResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"

export function SiteFooter({ links = [] }: { links?: LinkResponse[] }) {
  const { t } = useI18n()
  return (
    <footer className="border-t border-border/70">
      <div className="page-wrap flex flex-col gap-8 py-12 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="flex items-center gap-2 font-heading">
            <StarMark className="size-3.5 text-primary" />
            {site.name}
          </p>
          <p className="mt-2 max-w-sm text-sm text-muted-foreground">{t.tagline}</p>
        </div>
        <div className="flex flex-wrap gap-x-5 gap-y-2 text-sm text-muted-foreground">
          <Link href={routes.blog} className="hover:text-foreground">
            {t.posts}
          </Link>
          <Link href={routes.photography} className="hover:text-foreground">
            {t.photography}
          </Link>
          <Link href={routes.links} className="hover:text-foreground">
            {t.links}
          </Link>
          <Link href={routes.about} className="hover:text-foreground">
            {t.about}
          </Link>
          <Link href={routes.rss} className="hover:text-foreground">
            RSS
          </Link>
        </div>
        {links.length > 0 ? (
          <div className="max-w-sm">
            <p className="kicker">{t.links}</p>
            <ul className="mt-3 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
              {links.map((link) => (
                <li key={link.id}>
                  <a href={link.url} target="_blank" rel="noreferrer" className="hover:text-foreground">
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
