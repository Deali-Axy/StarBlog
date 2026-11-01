"use client"
import { useI18n } from "@/components/app-providers"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { LinkResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { ArrowUpRight } from "lucide-react"
import { linkDomain } from "../link-domain"
export function DirectoryLinks({ links }: {
  links: LinkResponse[]
}) {
  const { t } = useI18n()
  return <section data-home-section="links" data-home-variant={"directory"} className={homeSectionClassName} aria-labelledby="home-links-title">
    <HomeSectionHeading id="home-links-title" eyebrow={t.homeGoodCompany} title={t.homeLinksTitle} href={routes.links} action={t.applyLink} />
    {links.length ? <ul className={"grid sm:grid-cols-2 min-[56.25rem]:grid-cols-3 gap-0 sm:gap-x-6 sm:gap-y-3"}>
      {links.map(link => <li key={link.id} className="min-w-0">
        <a href={link.url} target="_blank" rel="noreferrer" className={"group flex h-full items-start gap-3 border-border border-t py-5"}>
          <span className={"h-10 w-8 shrink-0 place-items-center border border-border font-heading text-[22px] text-primary italic group-hover:border-primary grid"} aria-hidden="true">{Array.from(link.name.trim())[0]?.toUpperCase() || "＊"}</span>
          <div className={"min-w-0"}><h3 style={{ zoom: "var(--home-scale)" }} className="flex items-baseline gap-2 font-heading text-lg leading-normal font-medium wrap-anywhere group-hover:text-primary">{link.name}<ArrowUpRight className="shrink-0" size={14} aria-hidden="true" /></h3>
            {link.description ? <p style={{ zoom: "var(--home-scale)" }} className="mt-2 line-clamp-2 text-xs leading-[1.8] text-muted-foreground">{link.description}</p> : null}
            <span className={"block font-mono text-[11px] wrap-anywhere text-muted-foreground mt-2.5"}>{linkDomain(link.url)}</span>
          </div>
        </a>
      </li>)}
    </ul> : <p style={{ zoom: "var(--home-scale)" }} className="text-[13px] text-muted-foreground">{t.noLinks}</p>}
  </section>
}
