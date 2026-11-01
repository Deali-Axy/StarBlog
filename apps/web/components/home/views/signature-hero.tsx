"use client"
import { messages } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { ArrowUpRight, Camera, Shuffle } from "lucide-react"
import Link from "next/link"
import type { HomeHeroProps } from "../home-hero-props"
export function SignatureHero({ locale, site, overview, topPost }: HomeHeroProps) {
  const t = messages[locale]
  const name = site.authorName.replace(/\s*[(（].*?[)）]\s*$/u, "")
  return <section data-home-hero="signature" aria-labelledby="home-hero-name" className={cn("w-full max-w-[var(--home-width)] mx-auto px-[1rem]", "pt-8 pb-10")}>

    <div className={"border-b border-foreground py-10 sm:py-14"}>
      <p style={{ zoom: "var(--home-scale)" }} className="mb-5 text-xs tracking-[.2em] text-primary">{t.personalJournal} / {t.hero}</p>
      <h1 style={{ zoom: "var(--home-scale)" }} id="home-hero-name" className="font-heading text-[clamp(3rem,9vw,6rem)] leading-tight tracking-[-.055em] wrap-anywhere">{name}<span className="ml-4 text-primary" aria-hidden="true">＊</span></h1>
      <p style={{ zoom: "var(--home-scale)" }} className="mt-5 font-heading text-xl">{t.tagline}</p>
      <p style={{ zoom: "var(--home-scale)" }} className="mt-4 max-w-xl text-sm leading-7 text-muted-foreground">{site.authorTitle} / {site.authorBio}</p>
      <div className="mt-6 flex flex-wrap items-center gap-x-7 gap-y-2 text-sm [&_a]:inline-flex [&_a]:min-h-11 [&_a]:items-center [&_a]:gap-2 [&_a]:hover:text-primary">
        <Link href={routes.blogRandom}><Shuffle size={16} aria-hidden="true" />{t.randomPost}</Link>
        <Link href={routes.photographyRandom}><Camera size={16} aria-hidden="true" />{t.randomPhoto}</Link>
        <Link href={routes.about}>{t.aboutMe}<ArrowUpRight size={16} aria-hidden="true" /></Link>
      </div>
      {overview ? <p style={{ zoom: "var(--home-scale)" }} className="mt-5 text-xs text-muted-foreground">{t.posts} {overview.postsCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")} / {t.categories} {overview.categoriesCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")} / {t.photography} {overview.photosCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")}</p> : null}
      {topPost ? <p style={{ zoom: "var(--home-scale)" }} className="mt-5 border-t border-border pt-5 text-sm"><span className="mr-3 text-xs text-primary">{t.pinned}</span><Link href={postHref(topPost)} className="hover:text-primary">{topPost.title}</Link></p> : null}
    </div>
  </section>
}
