import type { Metadata } from "next"

import { AboutProfile } from "@/components/about/about-profile"
import { getI18n } from "@/lib/i18n-server"
import { getSite } from "@/lib/site"

export async function generateMetadata(): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const description = `${site.authorName} · ${site.authorBio}`
  return {
    title: t.aboutTitle,
    description,
    alternates: { canonical: "/about" },
    openGraph: { title: `${t.aboutTitle} · ${site.authorName}`, description, url: "/about" },
  }
}

export default async function AboutPage() {
  const { locale, t } = await getI18n()
  return <AboutProfile site={getSite(locale)} t={t} />
}
