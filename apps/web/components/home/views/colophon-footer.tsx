"use client"
import { useI18n } from "@/components/app-providers"
import { getSite } from "@/lib/site"
import { cn } from "@/lib/utils"
import { ArrowUp } from "lucide-react"
export function ColophonFooter() {
  const { t, locale } = useI18n()
  const author = getSite(locale)
  return <footer data-home-footer={"colophon"} className={cn("w-full max-w-[var(--home-width)] mx-auto px-[1rem]", "pt-12 pb-24 sm:pb-12")}>
    <div className={"flex flex-wrap items-end justify-between gap-8 border-t border-foreground pt-8 pb-2"}>
      <div className="min-w-0">
        <p className="font-heading text-[clamp(1.5rem,3vw,2rem)] tracking-[-.04em]">{t.homeFooterHeadline}<span className="ml-3 text-primary" aria-hidden="true">＊</span></p>
        <p className="mt-3 text-xs text-muted-foreground">{`${author.name} / ${t.hero}`}</p>

      </div>
      <a href="#home-top" className="inline-flex min-h-11 items-center gap-4 text-xs text-muted-foreground hover:text-primary">{t.backToTop}<ArrowUp aria-hidden="true" className="size-4" /></a>
    </div>
  </footer>
}
