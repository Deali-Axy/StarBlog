import type { Metadata } from "next"

import { AboutProfile } from "@/components/about/about-profile"
import { getI18n } from "@/lib/i18n-server"
import { pageMetadata } from "@/lib/seo"
import { getSite } from "@/lib/site"

export async function generateMetadata(): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const description = `${site.authorName} · ${site.authorBio}`
  return pageMetadata({ locale, title: `${t.aboutTitle} · ${site.authorName}`, description, path: "/about" })
}

export default async function AboutPage() {
  const { locale, t } = await getI18n()
  return <AboutProfile site={getSite(locale)} t={t} />
}
