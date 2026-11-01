"use client"
import { useI18n } from "@/components/app-providers"
import { routes } from "@/lib/routes"
import { getSite } from "@/lib/site"
import { cn } from "@/lib/utils"
import { ArrowUp, ArrowUpRight } from "lucide-react"
import Link from "next/link"
export function ContactFooter() {
  const { t, locale } = useI18n()
  const author = getSite(locale)
  return <footer data-home-footer={"contact"} className={cn("w-full max-w-[var(--home-width)] mx-auto px-[1rem]", "pt-12 pb-24 sm:pb-12")}>
    <div data-home-surface={""} data-home-padding={""} className={"flex flex-wrap items-end justify-between gap-8 border border-border bg-card [box-shadow:var(--home-shadow)] p-[calc(1.5rem*var(--home-density))]"}>
      <div className="min-w-0">
        <p className="font-heading text-[clamp(1.5rem,3vw,2rem)] tracking-[-.04em]">{author.authorName}<span className="ml-3 text-primary" aria-hidden="true">＊</span></p>
        <p className="mt-3 text-xs text-muted-foreground">{author.authorBio}</p>
        {<div className="mt-4 flex flex-wrap gap-x-6 text-xs [&_a]:inline-flex [&_a]:min-h-11 [&_a]:items-center [&_a]:gap-1 [&_a]:hover:text-primary">
          <a href={author.githubUrl} target="_blank" rel="noreferrer">GitHub<ArrowUpRight size={14} aria-hidden="true" /></a>
          <a href={author.zhihuUrl} target="_blank" rel="noreferrer">{t.zhihu}<ArrowUpRight size={14} aria-hidden="true" /></a>
          <a href={author.cnblogsUrl} target="_blank" rel="noreferrer">{t.cnblogs}<ArrowUpRight size={14} aria-hidden="true" /></a>
          <Link href={routes.about}>{t.aboutMe}</Link>
        </div>}
      </div>
      <a href="#home-top" className="inline-flex min-h-11 items-center gap-4 text-xs text-muted-foreground hover:text-primary">{t.backToTop}<ArrowUp aria-hidden="true" className="size-4" /></a>
    </div>
  </footer>
}
