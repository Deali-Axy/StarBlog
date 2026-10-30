"use client"

import { ArrowUpRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { LinkResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import styles from "./home-sections.module.css"

function domain(href: string) {
  try { return new URL(href).hostname.replace(/^www\./, "") } catch { return href }
}

export function FriendLinks({ links }: { links: LinkResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  return <section className={styles.section} aria-labelledby="home-links-title">
    <HomeSectionHeading id="home-links-title" eyebrow={variant === "desk" ? t.homePenPals : t.homeGoodCompany}
      title={variant === "desk" ? t.homeDeskLinksTitle : t.homeLinksTitle} href={routes.links} action={t.applyLink} />
    {links.length ? <ul className={styles.friendsGrid}>
      {links.map(link => <li key={link.id}>
        <a href={link.url} target="_blank" rel="noreferrer" className={styles.friendLink}>
          <span className={styles.friendInitial} aria-hidden="true">{Array.from(link.name.trim())[0]?.toUpperCase() || "＊"}</span>
          <div className={styles.friendInfo}><h3>{link.name}<ArrowUpRight size={14} aria-hidden="true" /></h3>
            {link.description ? <p>{link.description}</p> : null}
            <span className={styles.friendDomain}>{domain(link.url)}</span>
          </div>
        </a>
      </li>)}
    </ul> : <p className={styles.empty}>{t.noLinks}</p>}
  </section>
}
