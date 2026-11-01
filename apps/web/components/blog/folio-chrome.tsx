"use client"
import { useI18n } from "@/components/app-providers"
import { site } from "@/lib/site"
import { FolioBackToTop, FolioCopyright, FolioFooterNav, FolioFooterWordmark, FolioHeaderTools, FolioHeaderWordmark, FolioSiteNav } from "./views/folio-chrome-parts"
export function FolioHeader() {
  const { t } = useI18n()
  return <header data-blog-header="editorial" className={"sticky top-0 [z-index:40] bg-background text-foreground"}>
    <div className={"[width:min(100%_-_96px,_1200px)] mx-auto max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)]"}>
      <div className={"grid [grid-template-columns:1fr_auto_1fr] gap-6 items-center min-h-19 max-[39.9375rem]:flex max-[39.9375rem]:min-h-16.5 max-[39.9375rem]:gap-2 max-[39.9375rem]:justify-between"}>
        <p className={"text-[.6875rem] [line-height:1.8] text-muted-foreground [&_span]:block [&_span]:[font:8px_ui-monospace,_monospace] [&_span]:[letter-spacing:.1em] [&_span]:mt-[3px] max-[39.9375rem]:hidden"}>{site.authorName}<span>{t.folioChromeLabel}</span></p>
        <FolioHeaderWordmark />
        <FolioHeaderTools />
      </div>
      <FolioSiteNav />
    </div>
  </header>
}
export function FolioFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="editorial" className={"[padding-block:32px_24px] max-[39.9375rem]:py-5"}>
    <div className={"[width:min(100%_-_96px,_1200px)] mx-auto [border-top:3px_double_var(--foreground)] text-center pt-10 max-[63.9375rem]:[width:calc(100%_-_48px)] max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:pt-7"}>
      <span className={"inline-block text-primary text-[1.75rem]"} aria-hidden="true">✳</span>
      <p className={"mt-4 [font:600_clamp(38px,_4.5vw,_66px)/1.4_var(--font-heading)] [letter-spacing:-.04em] max-[39.9375rem]:text-[40px]"}>{t.folioFooterHeadline}</p>
      <p className={"mt-3 text-xs text-muted-foreground max-[39.9375rem]:text-[.6875rem] max-[39.9375rem]:[line-height:1.9]"}>{t.folioFooterNote}</p>
      <FolioFooterNav />
      <div className={"flex flex-wrap items-center justify-between gap-5 [border-top:1px_solid_var(--border)] pt-5 text-[.625rem] text-muted-foreground max-[39.9375rem]:gap-4 max-[39.9375rem]:justify-center max-[39.9375rem]:text-[.5625rem]"}><FolioFooterWordmark /><FolioCopyright /><FolioBackToTop /></div>
    </div>
  </footer>
}
