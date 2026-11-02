import Link from "next/link"

import type { RssViewProps } from "@/components/rss/rss-view-props"
import { routes } from "@/lib/routes"

export function FlatRss({ t, feedAddress }: RssViewProps) {
  const copy = t.rssPage

  return (
    <div className="mx-auto w-full max-w-3xl px-4 pt-8 pb-16 sm:pt-10 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-ring">
      <header className="border-b border-border pb-7">
        <h1 className="text-3xl font-semibold tracking-tight">RSS</h1>
        <p className="mt-3 text-sm leading-7 text-muted-foreground">{copy.flatIntro}</p>
      </header>

      <section aria-labelledby="flat-feed-address-title" className="border-b border-border py-7">
        <h2 id="flat-feed-address-title" className="mb-4 text-base font-semibold">{copy.addressLabel}</h2>
        {feedAddress}
        <div className="mt-4 flex flex-wrap items-center justify-between gap-x-6 gap-y-2">
          <p className="text-xs leading-6 text-muted-foreground">{copy.formatNote}</p>
          <a href={routes.feed} className="inline-flex min-h-11 items-center text-sm underline decoration-border underline-offset-4 hover:decoration-current">{t.openFeed}</a>
        </div>
      </section>

      <section aria-labelledby="flat-rss-guide-title" className="py-7">
        <h2 id="flat-rss-guide-title" className="text-base font-semibold">{copy.flatGuideTitle}</h2>
        <ol className="mt-4 list-decimal space-y-4 pl-5 marker:text-sm marker:text-muted-foreground">
          {copy.steps.map((step) => (
            <li key={step.title} className="pl-1">
              <h3 className="text-sm font-medium">{step.title}</h3>
              <p className="mt-1 text-sm leading-7 text-muted-foreground">{step.detail}</p>
            </li>
          ))}
        </ol>
        <p className="mt-7 text-sm leading-7 text-muted-foreground">{copy.note}</p>
        <Link href={routes.blog} className="mt-4 inline-flex min-h-11 items-center text-sm underline decoration-border underline-offset-4 hover:decoration-current">{t.allPosts}</Link>
      </section>
    </div>
  )
}
