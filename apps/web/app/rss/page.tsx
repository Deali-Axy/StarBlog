import type { Metadata } from "next"
import { Suspense } from "react"

import { RssDesign } from "@/components/rss/rss-design"
import { getI18n } from "@/lib/i18n-server"
import { pageMetadata } from "@/lib/seo"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export async function generateMetadata(): Promise<Metadata> {
  const { locale, t } = await getI18n()
  return pageMetadata({ locale, title: locale === "en" ? "Subscribe via RSS" : "RSS 订阅", description: t.feedDescription(site.name), path: routes.rss })
}

export default function RssPage() {
  return <Suspense><RssDesign feedUrl={absoluteUrl(routes.feed)} /></Suspense>
}
