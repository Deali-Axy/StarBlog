import type { Metadata } from "next"
import { Suspense } from "react"

import { RssDesign } from "@/components/rss/rss-design"
import { getI18n } from "@/lib/i18n-server"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: "RSS Feed", description: t.feedDescription(site.name), alternates: { canonical: routes.rss } }
}

export default function RssPage() {
  return <Suspense><RssDesign feedUrl={absoluteUrl(routes.feed)} /></Suspense>
}
