import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: "RSS Feed", description: t.feedDescription(site.name) }
}

export default async function RssPage() {
  const { t } = await getI18n()
  const feedUrl = absoluteUrl(routes.feed)

  return (
    <div className="mx-auto w-full max-w-3xl px-4 py-16">
      <p className="kicker">{t.subscribe}</p>
      <h1 className="mt-2 text-3xl">RSS</h1>
      <p className="mt-8 leading-relaxed text-muted-foreground">
        {t.rssHint}
      </p>
      <p className="mt-6 break-all font-mono text-sm">{feedUrl}</p>
      <Button className="mt-8" asChild>
        <a href={routes.feed}>{t.openFeed}</a>
      </Button>
    </div>
  )
}
