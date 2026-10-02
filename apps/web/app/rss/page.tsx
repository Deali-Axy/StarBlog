import type { Metadata } from "next"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export const metadata: Metadata = {
  title: "RSS Feed",
  description: `订阅 ${site.name} 的 Atom 源。`,
}

export default function RssPage() {
  const feedUrl = absoluteUrl(routes.feed)

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-800">
      <div className="mx-auto w-full max-w-3xl px-4 py-12">
        <div className="mb-8 flex items-end justify-between">
          <h1 className="text-3xl font-bold">订阅内容</h1>
          <p className="text-sm text-muted-foreground">RSS Feed</p>
        </div>
        <Card className="border-0 bg-white/80 py-16 shadow-sm dark:bg-slate-900/80">
          <CardContent className="space-y-4 text-center">
            <h2 className="text-xl font-semibold">订阅地址</h2>
            <p className="break-all font-mono text-sm">{feedUrl}</p>
            <p className="text-muted-foreground">将订阅地址添加到 RSS 阅读器即可</p>
            <Button asChild>
              <a href={routes.feed}>打开 Atom 源</a>
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
