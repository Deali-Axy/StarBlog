"use client"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { StarMark } from "@/components/brand/star-mark"
import { messages } from "@/lib/i18n"
import { routes } from "@/lib/routes"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"
import { ArrowDownRight, ArrowUpRight, Camera, Rss, Shuffle } from "lucide-react"
import Link from "next/link"
import type { HomeHeroProps } from "../home-hero-props"
import { CoverHeroPhoto } from "./cover-hero-photo"
export function CoverHero({ locale, site, overview, photo, topPost }: HomeHeroProps) {
  const t = messages[locale]
  const identity = site.authorName.match(/^(.+?)\s*[(（]([^()（）]+)[)）]\s*$/u)
  const name = identity?.[1]?.trim() || site.authorName
  const alias = identity?.[2]?.trim()
  const hasVisual = Boolean(photo || topPost)
  return (<section data-home-hero="cover" className={cn("w-full max-w-[var(--home-width)] mx-auto px-[1rem]", "pt-8 pb-10", "[&_:is(a,button)]:[-webkit-tap-highlight-color:transparent] [&_:is(a,button):focus-visible]:outline-2 [&_:is(a,button):focus-visible]:outline-solid [&_:is(a,button):focus-visible]:outline-offset-4 [&_:is(a,button):focus-visible]:outline-(--hero-ink)", "motion-reduce:[&_*]:transition-none! motion-reduce:[&_*]:animate-none! motion-reduce:[&_*::before]:transition-none! motion-reduce:[&_*::before]:animate-none! motion-reduce:[&_*::after]:transition-none! motion-reduce:[&_*::after]:animate-none!")} aria-labelledby="home-hero-name">
    <div className="flex flex-wrap items-center justify-between gap-3 pt-6 text-xs tracking-widest">
      <span className="flex items-center gap-2.5">
        <span className="size-1.5 rounded-full bg-(--hero-ink)" />
        {t.personalJournal}
      </span>
      <span className="text-(--hero-ink)">{t.hero}</span>
    </div>

    <div className={cn("grid gap-10 py-9 md:pt-8 md:pb-10", hasVisual && "md:grid-cols-[minmax(0,0.85fr)_minmax(0,1.15fr)]")}>
      <div className="min-w-0">
        <div className="relative flex w-fit max-w-full items-start">
          <h1 style={{ zoom: "var(--home-scale)" }} id="home-hero-name" className={cn(headingClassName, "-ml-[0.055em] text-[clamp(6rem,22vw,10rem)] leading-tight tracking-tighter wrap-anywhere md:text-[clamp(7rem,15vw,11.5rem)] data-[long-name=true]:text-[clamp(3.5rem,10vw,6rem)] min-[70rem]:[&:not([data-long-name=true])]:text-[clamp(5.5rem,13cqw,11.5rem)]")} data-long-name={Array.from(name).length > 3}>
            {name}
          </h1>
          <StarMark className="mt-3 ml-3 size-7 shrink-0 rotate-12 text-(--hero-ink)" />
        </div>
        {alias ? <p style={{ zoom: "var(--home-scale)" }} className="mt-1 font-heading text-[clamp(2rem,5vw,3rem)] leading-tight tracking-tighter italic wrap-anywhere">{alias}<span aria-hidden="true" className="text-(--hero-ink)"> /</span></p> : null}

        <p style={{ zoom: "var(--home-scale)" }} className="mt-7 font-heading text-xl leading-relaxed">{t.tagline}</p>

        <div className="mt-6 border-l border-(--hero-ink) pl-4">
          <p style={{ zoom: "var(--home-scale)" }} className="text-sm font-medium">{site.authorTitle}</p>
          <p style={{ zoom: "var(--home-scale)" }} className="mt-2 max-w-sm text-sm leading-7 text-muted-foreground">{site.authorBio}</p>
          <Link href={routes.about} className="mt-2.5 inline-flex items-center gap-2 text-xs text-muted-foreground hover:text-(--hero-ink)">
            {t.aboutMe}<ArrowUpRight aria-hidden="true" className="size-3.5" />
          </Link>
        </div>

        <div className="mt-7 flex flex-wrap items-center gap-6">
          <Link href={routes.blogRandom} className="group/random inline-flex items-center gap-2.5 text-sm">
            <span className="grid size-11 place-items-center rounded-full bg-(--hero-ink) text-primary-foreground transition-transform duration-200 ease-in-out group-hover/random:-rotate-12"><Shuffle aria-hidden="true" className="size-5" /></span>
            <span>{t.randomPost}</span>
            <ArrowUpRight aria-hidden="true" className="size-4" />
          </Link>
          <Link href={routes.photographyRandom} className="inline-flex items-center gap-2.5 text-sm text-muted-foreground hover:text-(--hero-ink)">
            <Camera aria-hidden="true" className="size-4" />{t.randomPhoto}
          </Link>
        </div>
      </div>

      {photo ? (<CoverHeroPhoto photo={photo} />) : topPost ? (<div className="min-w-0 self-stretch md:mt-6">
        <p style={{ zoom: "var(--home-scale)" }} className="mb-6 text-xs tracking-widest">{t.pinned}</p>
        <EnhancedPostCard variant="featured" post={{ ...topPost, featured: true }} />
      </div>) : null}
    </div>

    <div className="flex flex-wrap items-center justify-between gap-7 border-y border-t-border border-b-foreground py-5">
      {overview ? (<dl className="flex gap-8">
        {[
          { label: t.posts, count: overview.postsCount, href: routes.blog },
          { label: t.categories, count: overview.categoriesCount, href: routes.blog },
          { label: t.photography, count: overview.photosCount, href: routes.photography },
        ].map(({ label, count, href }) => (<div key={label} className="flex flex-col-reverse gap-1">
          <dt style={{ zoom: "var(--home-scale)" }} className="text-xs text-muted-foreground">{label}</dt>
          <dd style={{ zoom: "var(--home-scale)" }} className="font-heading text-3xl leading-none lining-nums tabular-nums"><Link href={href} className="hover:text-(--hero-ink)">{count.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")}</Link></dd>
        </div>))}
      </dl>) : null}

      <div className="grid gap-3 md:ml-auto [&_a]:hover:text-(--hero-ink)">
        <div className="flex flex-wrap items-center gap-x-5 gap-y-2.5 font-mono text-xs text-muted-foreground" aria-label={t.skills}>
          {site.skills.map((skill) => (<Link key={skill.label} href={routes.searchResults(skill.keyword)}>{skill.label}</Link>))}
        </div>
        <div className="flex flex-wrap items-center gap-x-5 gap-y-2.5 text-xs text-muted-foreground [&_a]:inline-flex [&_a]:items-center [&_a]:gap-1 [&_svg]:size-3">
          <a href={site.githubUrl} target="_blank" rel="noreferrer">GitHub<ArrowUpRight aria-hidden="true" /></a>
          <a href={site.zhihuUrl} target="_blank" rel="noreferrer">{t.zhihu}<ArrowUpRight aria-hidden="true" /></a>
          <a href={site.cnblogsUrl} target="_blank" rel="noreferrer">{t.cnblogs}<ArrowUpRight aria-hidden="true" /></a>
          <Link href={routes.rss}><Rss aria-hidden="true" />RSS</Link>
        </div>
      </div>
      <ArrowDownRight aria-hidden="true" className="hidden size-9 stroke-1 text-(--hero-ink) md:ml-4 md:block" />
    </div>
  </section>)
}
