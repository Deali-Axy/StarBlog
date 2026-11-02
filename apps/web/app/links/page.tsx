import type { Metadata } from "next"
import { Suspense } from "react"

import { LinksDesign } from "@/components/links/links-design"
import { listVisibleLinks } from "@/lib/api/queries"
import { getI18n } from "@/lib/i18n-server"
import { routes } from "@/lib/routes"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.friendLinks, description: t.linksDescription, alternates: { canonical: routes.links } }
}

export default async function LinksPage() {
  const links = await listVisibleLinks()
  return <Suspense><LinksDesign links={links} /></Suspense>
}
