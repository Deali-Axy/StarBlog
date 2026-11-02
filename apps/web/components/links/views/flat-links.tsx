import { ArrowUpRight } from "lucide-react"

import { linkDomain } from "@/components/home/link-domain"
import type { LinksViewProps } from "@/components/links/links-view-props"

export function FlatLinks({ links, t, site, form }: LinksViewProps) {
  return (
    <div className="mx-auto w-full max-w-6xl px-4 pt-8 pb-16 sm:pt-10 [&_a:focus-visible]:outline-2 [&_a:focus-visible]:outline-offset-4 [&_a:focus-visible]:outline-ring">
      <header className="flex flex-wrap items-start justify-between gap-5 border-b border-border pb-7">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">{t.friendLinks}</h1>
          <p className="mt-3 max-w-2xl text-sm leading-7 text-muted-foreground">{t.linksPage.flatIntro}</p>
        </div>
        <a href="#link-apply" className="inline-flex min-h-11 items-center border border-border px-4 text-sm transition-colors hover:bg-muted">{t.applyLink}</a>
      </header>

      <section aria-label={t.friendLinks} className="py-7">
        <p className="mb-3 text-xs text-muted-foreground">{t.linksPage.count(links.length)}</p>
        {links.length > 0 ? (
          <ul className="grid gap-x-10 sm:grid-cols-2 lg:grid-cols-3">
            {links.map((link) => (
              <li key={link.id} className="min-w-0 border-b border-border">
                <a href={link.url} target="_blank" rel="noreferrer" className="group flex h-full flex-col py-5">
                  <div className="flex items-start justify-between gap-4">
                    <h2 className="text-base font-medium wrap-anywhere group-hover:underline group-hover:underline-offset-4">{link.name}</h2>
                    <ArrowUpRight aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-muted-foreground" />
                  </div>
                  {link.description ? <p className="mt-2 line-clamp-2 text-sm leading-6 wrap-anywhere text-muted-foreground">{link.description}</p> : null}
                  <p className="mt-auto pt-3 text-xs wrap-anywhere text-muted-foreground">{linkDomain(link.url)}</p>
                </a>
              </li>
            ))}
          </ul>
        ) : <p className="py-7 text-sm leading-7 text-muted-foreground">{t.noLinks}</p>}
      </section>

      <section id="link-apply" aria-labelledby="flat-link-apply-title" className="scroll-mt-28 pt-6">
        <h2 id="flat-link-apply-title" className="text-xl font-semibold">{t.applyLink}</h2>
        <p className="mt-3 max-w-2xl text-sm leading-7 text-muted-foreground">{t.linksPage.flatApplyIntro}</p>
        <div className="mt-7 grid items-start gap-10 lg:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)] lg:gap-16">
          <div className="min-w-0">{form}</div>
          <aside className="min-w-0 space-y-7">
            <section aria-labelledby="flat-site-info-title">
              <h3 id="flat-site-info-title" className="text-sm font-semibold">{t.linksPage.siteInfo}</h3>
              <dl className="mt-4 space-y-3 text-sm">
                {[[t.websiteName, site.name], [t.url, site.url], [t.description, site.description]].map(([label, value]) => (
                  <div key={label} className="grid grid-cols-[6rem_minmax(0,1fr)] gap-3">
                    <dt className="leading-6 text-muted-foreground">{label}</dt>
                    <dd className="leading-6 wrap-anywhere select-all">{value}</dd>
                  </div>
                ))}
              </dl>
            </section>
            <details className="border-y border-border py-4">
              <summary className="cursor-pointer text-sm font-medium focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-ring">{t.requirements}</summary>
              <ul className="mt-4 space-y-4">
                {t.linkRequirements.map((item) => (
                  <li key={item.title}>
                    <p className="text-sm font-medium">{item.title}</p>
                    <p className="mt-1 text-xs leading-6 text-muted-foreground">{item.detail}</p>
                  </li>
                ))}
              </ul>
            </details>
          </aside>
        </div>
      </section>
    </div>
  )
}
