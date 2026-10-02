import type { Metadata } from "next"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export const metadata: Metadata = {
  title: "RSS Feed",
  description: `订阅 ${site.name} 的 Atom 源。`,
}

export default function RssPage() {
  const feedUrl = absoluteUrl(routes.feed)

  return (
    <div className="mx-auto w-full max-w-3xl px-4 py-16">
      <p className="kicker">订阅</p>
      <h1 className="mt-2 text-3xl">RSS</h1>
      <p className="mt-8 leading-relaxed text-muted-foreground">
        把地址放进阅读器即可，不必经过任何平台。
      </p>
      <p className="mt-6 break-all font-mono text-sm">{feedUrl}</p>
      <Button className="mt-8" asChild>
        <a href={routes.feed}>打开 Atom 源</a>
      </Button>
    </div>
  )
}
