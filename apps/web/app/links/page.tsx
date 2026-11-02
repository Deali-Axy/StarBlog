import type { Metadata } from "next"
import { ArrowDown, ArrowUpRight, Asterisk, Globe2, Send } from "lucide-react"

import { LinkApplyForm } from "@/components/links/link-apply-form"
import { linkDomain } from "@/components/home/link-domain"
import { listVisibleLinks } from "@/lib/api/queries"
import { getI18n } from "@/lib/i18n-server"
import { routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.friendLinks, description: t.linksDescription, alternates: { canonical: routes.links } }
}

export default async function LinksPage() {
  const { locale, t } = await getI18n()
  const links = await listVisibleLinks()
  const site = getSite(locale)
  const copy = t.linksPage

  return (
    <div className={cn(pageContainerClassName, "pt-8 pb-14 sm:pt-12 sm:pb-20 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-primary")}>
      <div className="flex items-center justify-between gap-4 border-b border-foreground pb-4">
        <p className="font-mono text-xs tracking-wider text-muted-foreground">{copy.eyebrow}</p>
        <Asterisk aria-hidden="true" className="size-5 shrink-0 text-primary" />
      </div>

      <header className="grid gap-8 py-12 sm:py-16 md:grid-cols-[minmax(0,1fr)_15rem] md:items-end md:gap-16">
        <div>
          <p className="mb-4 text-sm text-primary">{t.friendLinks}</p>
          <h1 className="max-w-3xl font-heading text-4xl leading-tight tracking-tight sm:text-6xl sm:leading-tight">{copy.headline}</h1>
          <p className="mt-6 max-w-xl text-sm leading-7 text-muted-foreground">{copy.intro}</p>
        </div>
        <a href="#link-apply" className="group flex min-h-14 items-center justify-between gap-6 border-y border-primary/40 py-4 text-sm text-primary">
          <span className="flex items-center gap-3"><Send aria-hidden="true" className="size-4 stroke-1" />{t.applyLink}</span>
          <ArrowDown aria-hidden="true" className="size-4 transition-transform group-hover:translate-y-1 motion-reduce:transform-none" />
        </a>
      </header>

      <section aria-labelledby="links-directory-title" className="border-t border-foreground pt-6 pb-14 sm:pb-20">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="font-mono text-xs tracking-wider text-primary">{copy.directory}</p>
            <h2 id="links-directory-title" className="mt-3 font-heading text-2xl tracking-tight sm:text-3xl">{copy.directoryTitle}</h2>
          </div>
          <p className="font-mono text-xs text-muted-foreground">{copy.count(links.length)}</p>
        </div>
        {links.length > 0 ? (
          <ul className="grid gap-x-8 sm:grid-cols-2 lg:grid-cols-3">
            {links.map((link, index) => (
              <li key={link.id} className="min-w-0 border-t border-border">
                <a href={link.url} target="_blank" rel="noreferrer" className="group flex h-full flex-col py-6 transition-colors hover:bg-primary/5">
                  <div className="mb-6 flex items-center justify-between gap-4">
                    <span aria-hidden="true" className="grid size-11 place-items-center border border-primary/25 font-heading text-2xl text-primary italic transition-colors group-hover:border-primary">{Array.from(link.name.trim())[0]?.toUpperCase() || "＊"}</span>
                    <span aria-hidden="true" className="font-mono text-[10px] text-muted-foreground">/ {String(index + 1).padStart(2, "0")}</span>
                  </div>
                  <div className="flex items-start justify-between gap-4">
                    <h3 className="font-heading text-xl leading-snug wrap-anywhere transition-colors group-hover:text-primary">{link.name}</h3>
                    <ArrowUpRight aria-hidden="true" className="mt-1 size-4 shrink-0 text-primary transition-transform group-hover:-translate-y-0.5 group-hover:translate-x-0.5 motion-reduce:transform-none" />
                  </div>
                  {link.description ? <p className="mt-3 line-clamp-3 text-sm leading-7 wrap-anywhere text-muted-foreground">{link.description}</p> : null}
                  <p className="mt-auto pt-5 font-mono text-xs wrap-anywhere text-muted-foreground">{linkDomain(link.url)}</p>
                </a>
              </li>
            ))}
          </ul>
        ) : (
          <div className="border border-dashed border-primary/30 bg-primary/5 px-6 py-12 text-center">
            <Globe2 aria-hidden="true" className="mx-auto mb-5 size-9 stroke-1 text-primary" />
            <h3 className="font-heading text-2xl">{copy.emptyTitle}</h3>
            <p className="mt-3 text-sm leading-7 text-muted-foreground">{t.noLinks}</p>
            <a href="#link-apply" className="mt-5 inline-flex min-h-11 items-center gap-2 text-sm text-primary underline underline-offset-4">{t.applyLink}<ArrowDown aria-hidden="true" className="size-4" /></a>
          </div>
        )}
      </section>

      <section id="link-apply" aria-labelledby="link-apply-title" className="scroll-mt-28 border-t border-foreground pt-7">
        <p className="font-mono text-xs tracking-wider text-primary">{copy.applyLabel}</p>
        <h2 id="link-apply-title" className="mt-3 font-heading text-3xl leading-snug tracking-tight sm:text-4xl">{copy.applyTitle}</h2>
        <p className="mt-4 max-w-2xl text-sm leading-7 text-muted-foreground">{copy.applyIntro}</p>

        <div className="mt-10 grid items-start gap-10 lg:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)] lg:gap-16">
          <div className="border border-border bg-card p-5 sm:p-8">
            <h3 className="mb-7 flex items-center gap-3 font-heading text-xl"><Send aria-hidden="true" className="size-5 stroke-1 text-primary" />{t.applyLink}</h3>
            <LinkApplyForm />
          </div>
          <aside className="min-w-0 space-y-9">
            <section aria-labelledby="link-site-info" className="border-t-2 border-primary bg-primary/5 p-5 sm:p-6">
              <h3 id="link-site-info" className="font-heading text-xl">{copy.siteInfo}</h3>
              <p className="mt-2 text-xs leading-6 text-muted-foreground">{copy.siteInfoHint}</p>
              <dl className="mt-5 space-y-4 text-sm">
                {[[t.websiteName, site.name], [t.url, site.url], [t.description, site.description]].map(([label, value]) => (
                  <div key={label} className="grid grid-cols-[5rem_minmax(0,1fr)] gap-3">
                    <dt className="text-muted-foreground">{label}</dt>
                    <dd className="leading-6 wrap-anywhere select-all">{value}</dd>
                  </div>
                ))}
              </dl>
            </section>
            <section aria-labelledby="link-guidelines-title">
              <h3 id="link-guidelines-title" className="font-heading text-xl">{copy.guidelines}</h3>
              <ol className="mt-5 divide-y divide-border border-y border-border">
                {t.linkRequirements.map((item, index) => (
                  <li key={item.title} className="flex gap-4 py-4">
                    <span aria-hidden="true" className="pt-0.5 font-mono text-[10px] text-primary">{String(index + 1).padStart(2, "0")}</span>
                    <div><p className="text-sm font-medium">{item.title}</p><p className="mt-1 text-xs leading-6 text-muted-foreground">{item.detail}</p></div>
                  </li>
                ))}
              </ol>
            </section>
          </aside>
        </div>
      </section>
      <p className="mt-14 flex items-center gap-3 border-t border-foreground pt-5 font-heading text-sm text-muted-foreground italic"><Asterisk aria-hidden="true" className="size-4 text-primary" />{copy.closing}</p>
    </div>
  )
}
