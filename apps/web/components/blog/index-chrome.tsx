"use client"
import { useI18n } from "@/components/app-providers"
import { routes } from "@/lib/routes"
import { ArrowUpRight } from "lucide-react"
import { IndexBackToTop, IndexCopyright, IndexFooterNav, IndexFooterWordmark, IndexHeaderTools, IndexHeaderWordmark, IndexSiteNav } from "./views/index-chrome-parts"
export function IndexHeader() {
  const { t } = useI18n()
  return <header data-blog-header="library" className={"sticky top-0 z-40 bg-background text-foreground"}>
    <div className={"flex items-center gap-8 min-h-20 w-[min(100%_-_64px,_1280px)] mx-auto [border-bottom:2px_solid_var(--foreground)] max-[74.9375rem]:gap-5 max-[63.9375rem]:w-[calc(100%_-_48px)] max-[39.9375rem]:w-[calc(100%_-_32px)] max-[39.9375rem]:min-h-18 max-[39.9375rem]:gap-2 max-[23rem]:flex-wrap max-[23rem]:py-3"}>
      <div className={"flex flex-col items-start gap-[7px] shrink-0 [&_>_span]:ml-8 [&_>_span]:text-[.5rem] [&_>_span]:tracking-[.18em] max-[39.9375rem]:gap-1.5 max-[39.9375rem]:[&_>_span]:ml-[25px] max-[39.9375rem]:[&_>_span]:text-[7px] max-[39.9375rem]:[&_>_span]:tracking-[.1em]"}><IndexHeaderWordmark /><span>{t.indexChromeLabel}</span></div>
      <IndexSiteNav />
      <IndexHeaderTools />
    </div>
  </header>
}
export function IndexFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="library" className={"[--footer-foreground:#eff1e7] mt-6 [background:#1b2018] text-footer-foreground [&_:is(a,_button):focus-visible]:outline-current"}>
    <div className={"grid grid-cols-[minmax(0,_1fr)_220px] gap-12 w-[min(100%_-_64px,_1280px)] mx-auto pbs-[56px] pbe-[24px] max-[39.9375rem]:w-[calc(100%_-_32px)] max-[39.9375rem]:grid-cols-[minmax(0,_1fr)] max-[39.9375rem]:gap-8 max-[39.9375rem]:pt-9"}>
      <div className={"[&_p]:text-[clamp(48px,_6vw,_84px)] [&_p]:leading-[1.2] [&_p]:tracking-[-.055em] [&_p]:font-bold [&_p]:wrap-anywhere [&_>_a]:inline-flex [&_>_a]:items-center [&_>_a]:justify-between [&_>_a]:gap-18 [&_>_a]:mt-7 [&_>_a]:py-[14px] [&_>_a]:px-[18px] [&_>_a]:bg-highlight [&_>_a]:text-[#202719] [&_>_a]:text-xs max-[39.9375rem]:[&_p]:text-[49px] max-[39.9375rem]:[&_>_a]:mt-5 max-[39.9375rem]:[&_>_a]:py-3"}><p>{t.indexFooterHeadline}</p><a href={routes.feed}>{t.subscribe}<ArrowUpRight className="size-6" aria-hidden="true" /></a></div>
      <IndexFooterNav />
      <div className={"col-[1_/_-1] flex flex-wrap items-center gap-6 [border-top:1px_solid_color-mix(in_srgb,_var(--footer-foreground)_25%,_transparent)] pt-6 [font:10px_ui-monospace,_monospace] max-[39.9375rem]:gap-y-[12px] max-[39.9375rem]:gap-x-[20px] max-[39.9375rem]:text-[.5625rem]"}><IndexFooterWordmark /><IndexCopyright /><IndexBackToTop /></div>
    </div>
  </footer>
}
