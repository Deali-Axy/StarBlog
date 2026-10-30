"use client"

import { ArrowUpRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { LinkResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

function domain(href: string) {
  try { return new URL(href).hostname.replace(/^www\./, "") } catch { return href }
}

export function FriendLinks({ links }: { links: LinkResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  const desk = variant === "desk"
  return <section className={homeSectionClassName} aria-labelledby="home-links-title">
    <HomeSectionHeading id="home-links-title" eyebrow={variant === "desk" ? t.homePenPals : t.homeGoodCompany}
      title={variant === "desk" ? t.homeDeskLinksTitle : t.homeLinksTitle} href={routes.links} action={t.applyLink} />
    {links.length ? <ul className={cn("grid sm:grid-cols-2 min-[900px]:grid-cols-3", desk ? "gap-3 sm:gap-4" : "gap-0 sm:gap-x-6 sm:gap-y-3")}>
      {links.map(link => <li key={link.id} className="min-w-0">
        <a href={link.url} target="_blank" rel="noreferrer" className={cn("group flex h-full items-start gap-3 border-border py-5", desk ? "border border-dashed bg-card px-5" : "border-t")}>
          <span className={cn("h-10 w-8 shrink-0 place-items-center border border-border font-heading text-[22px] text-primary italic group-hover:border-primary", desk ? "hidden" : "grid")} aria-hidden="true">{Array.from(link.name.trim())[0]?.toUpperCase() || "＊"}</span>
          <div className={cn("min-w-0", desk && "flex h-full flex-col")}><h3 className="flex items-baseline gap-2 font-heading text-lg leading-normal font-medium wrap-anywhere group-hover:text-primary">{link.name}<ArrowUpRight className="shrink-0" size={14} aria-hidden="true" /></h3>
            {link.description ? <p className="mt-2 line-clamp-2 text-xs leading-[1.8] text-muted-foreground">{link.description}</p> : null}
            <span className={cn("block font-mono text-[11px] wrap-anywhere text-muted-foreground", desk ? "mt-auto pt-6" : "mt-2.5")}>{domain(link.url)}</span>
          </div>
        </a>
      </li>)}
    </ul> : <p className="text-[13px] text-muted-foreground">{t.noLinks}</p>}
  </section>
}
