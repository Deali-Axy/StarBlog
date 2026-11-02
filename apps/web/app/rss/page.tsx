import type { Metadata } from "next"
import Link from "next/link"
import { ArrowUpRight, Asterisk, Rss } from "lucide-react"

import { FeedAddress } from "@/components/rss/feed-address"
import { getI18n } from "@/lib/i18n-server"
import { routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"
import { pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: "RSS Feed", description: t.feedDescription(site.name), alternates: { canonical: routes.rss } }
}

export default async function RssPage() {
  const { t } = await getI18n()
  const copy = t.rssPage

  return (
    <div className={cn(pageContainerClassName, "pt-8 pb-14 sm:pt-12 sm:pb-20 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-primary")}>
      <div className="flex items-center justify-between gap-4 border-b border-foreground pb-4">
        <p className="font-mono text-xs tracking-wider text-muted-foreground">{copy.eyebrow}</p>
        <Asterisk aria-hidden="true" className="size-5 shrink-0 text-primary" />
      </div>

      <header className="grid gap-10 py-12 sm:py-16 md:grid-cols-[minmax(0,1fr)_16rem] md:items-center md:gap-12">
        <div>
          <p className="mb-4 text-sm text-primary">{t.subscribe}</p>
          <h1 className="font-heading text-4xl leading-tight tracking-tight whitespace-pre-line sm:text-6xl sm:leading-tight">{copy.headline}</h1>
          <p className="mt-6 max-w-xl text-sm leading-7 text-muted-foreground">{copy.intro}</p>
        </div>
        <div aria-hidden="true" className="relative mx-auto hidden size-60 items-center justify-center md:flex">
          <div className="absolute inset-0 rounded-full border border-primary/20" />
          <div className="absolute inset-5 rounded-full border border-dashed border-primary/25" />
          <div className="absolute inset-10 rounded-full bg-primary/5" />
          <Rss className="relative size-20 stroke-1 text-primary" />
          <Asterisk className="absolute top-3 right-8 size-7 bg-background text-primary" />
          <span className="absolute bottom-1 left-14 bg-background px-2 font-mono text-[10px] tracking-[0.2em] text-primary">STAY CURIOUS</span>
        </div>
      </header>

      <section aria-labelledby="feed-address-title" className="border-y border-foreground bg-card px-5 py-7 sm:px-8 sm:py-8">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <h2 id="feed-address-title" className="flex items-center gap-3 font-heading text-xl"><Rss aria-hidden="true" className="size-5 text-primary" />{copy.addressLabel}</h2>
          <span className="border border-primary/30 px-2 py-1 font-mono text-[10px] tracking-widest text-primary">ATOM / XML</span>
        </div>
        <p className="mb-5 text-sm leading-7 text-muted-foreground">{t.rssHint}</p>
        <FeedAddress url={absoluteUrl(routes.feed)} />
        <div className="mt-5 flex flex-wrap items-center justify-between gap-x-6 gap-y-3">
          <p className="text-xs leading-6 text-muted-foreground">{copy.formatNote}</p>
          <a href={routes.feed} className="inline-flex min-h-11 items-center gap-2 text-sm text-primary underline decoration-primary/40 underline-offset-4 hover:decoration-primary">{t.openFeed}<ArrowUpRight aria-hidden="true" className="size-4" /></a>
        </div>
      </section>

      <section aria-labelledby="rss-guide-title" className="py-14 sm:py-20">
        <p className="font-mono text-xs tracking-wider text-primary">{copy.guideLabel}</p>
        <h2 id="rss-guide-title" className="mt-3 font-heading text-3xl leading-snug tracking-tight sm:text-4xl">{copy.guideTitle}</h2>
        <ol className="mt-9 grid gap-7 md:grid-cols-3 md:gap-9">
          {copy.steps.map((step, index) => (
            <li key={step.title} className="border-t border-border pt-5">
              <span aria-hidden="true" className="font-heading text-4xl text-primary/60 italic">{String(index + 1).padStart(2, "0")}</span>
              <h3 className="mt-5 font-heading text-xl">{step.title}</h3>
              <p className="mt-3 max-w-sm text-sm leading-7 text-muted-foreground">{step.detail}</p>
            </li>
          ))}
        </ol>
      </section>

      <aside aria-labelledby="rss-note-title" className="relative overflow-hidden bg-foreground px-6 py-9 text-background sm:px-10 sm:py-12">
        <Rss aria-hidden="true" className="pointer-events-none absolute -right-8 -bottom-8 size-56 stroke-1 opacity-10" />
        <div className="relative grid gap-6 md:grid-cols-[12rem_minmax(0,1fr)] md:gap-10">
          <p className="max-w-40 font-mono text-xs leading-6 tracking-wider opacity-70">{copy.noteLabel}</p>
          <div>
            <h2 id="rss-note-title" className="font-heading text-2xl leading-snug tracking-tight sm:text-3xl">{copy.noteTitle}</h2>
            <p className="mt-4 max-w-xl text-sm leading-7 opacity-75">{copy.note}</p>
            <Link href={routes.blog} className="mt-6 inline-flex min-h-11 items-center gap-3 text-sm underline decoration-current/40 underline-offset-5 hover:decoration-current">{copy.browse}<ArrowUpRight aria-hidden="true" className="size-4" /></Link>
          </div>
        </div>
      </aside>
      <p className="mt-12 flex items-center gap-3 border-t border-foreground pt-5 font-heading text-sm text-muted-foreground italic"><Asterisk aria-hidden="true" className="size-4 text-primary" />{copy.closing}</p>
    </div>
  )
}
