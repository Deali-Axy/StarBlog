"use client"

import { useState } from "react"
import { useHomeDesign } from "@/components/home/home-design-context"
import styles from "./home-design.module.css"
import Link from "next/link"
import { ArrowDownRight, ArrowUpRight, Camera, Rss, Shuffle } from "lucide-react"

import { StarMark } from "@/components/brand/star-mark"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { HeroPhoto } from "@/components/home/hero-photo"
import type { PhotoResponse, PublishedPostSummary, SiteOverviewResponse } from "@/lib/api/types"
import { messages, type Locale } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import type { getSite } from "@/lib/site"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export type HomeHeroProps = {
    locale: Locale
    site: ReturnType<typeof getSite>
    overview: SiteOverviewResponse | null
    photo?: PhotoResponse | null
    topPost?: PublishedPostSummary | null
}

function CoverHero({ locale, site, overview, photo, topPost }: HomeHeroProps) {
    const t = messages[locale]
    const identity = site.authorName.match(/^(.+?)\s*[(（]([^()（）]+)[)）]\s*$/u)
    const name = identity?.[1]?.trim() || site.authorName
    const alias = identity?.[2]?.trim()
    const hasVisual = Boolean(photo || topPost)

    return (
        <section data-home-hero="cover"
            className={cn(
                styles.container,
                "pt-8 pb-10",
                "[&_:is(a,button)]:[-webkit-tap-highlight-color:transparent] [&_:is(a,button):focus-visible]:outline-2 [&_:is(a,button):focus-visible]:outline-solid [&_:is(a,button):focus-visible]:outline-offset-4 [&_:is(a,button):focus-visible]:outline-(--hero-ink)",
                "motion-reduce:[&_*]:transition-none! motion-reduce:[&_*]:animate-none! motion-reduce:[&_*::before]:transition-none! motion-reduce:[&_*::before]:animate-none! motion-reduce:[&_*::after]:transition-none! motion-reduce:[&_*::after]:animate-none!",
            )}
            aria-labelledby="home-hero-name"
        >
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
                        <h1 id="home-hero-name" className={cn(headingClassName, "-ml-[0.055em] text-[clamp(6rem,22vw,10rem)] leading-tight tracking-tighter wrap-anywhere md:text-[clamp(7rem,15vw,11.5rem)] data-[long-name=true]:text-[clamp(3.5rem,10vw,6rem)]")} data-long-name={Array.from(name).length > 3}>
                            {name}
                        </h1>
                        <StarMark className="mt-3 ml-3 size-7 shrink-0 rotate-12 text-(--hero-ink)" />
                    </div>
                    {alias ? <p className="mt-1 font-heading text-[clamp(2rem,5vw,3rem)] leading-tight tracking-tighter italic wrap-anywhere">{alias}<span aria-hidden="true" className="text-(--hero-ink)"> /</span></p> : null}

                    <p className="mt-7 font-heading text-xl leading-relaxed">{t.tagline}</p>

                    <div className="mt-6 border-l border-(--hero-ink) pl-4">
                        <p className="text-sm font-medium">{site.authorTitle}</p>
                        <p className="mt-2 max-w-sm text-sm leading-7 text-muted-foreground">{site.authorBio}</p>
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

                {photo ? (
                    <HeroPhoto photo={photo} />
                ) : topPost ? (
                    <div className="min-w-0 self-stretch md:mt-6">
                        <p className="mb-6 text-xs tracking-widest">{t.pinned}</p>
                        <EnhancedPostCard
                            variant="featured"
                            post={{ ...topPost, featured: true }}
                        />
                    </div>
                ) : null}
            </div>

            <div className="flex flex-wrap items-center justify-between gap-7 border-y border-t-border border-b-foreground py-5">
                {overview ? (
                    <dl className="flex gap-8">
                        {[
                            { label: t.posts, count: overview.postsCount, href: routes.blog },
                            { label: t.categories, count: overview.categoriesCount, href: routes.blog },
                            { label: t.photography, count: overview.photosCount, href: routes.photography },
                        ].map(({ label, count, href }) => (
                            <div key={label} className="flex flex-col-reverse gap-1">
                                <dt className="text-xs text-muted-foreground">{label}</dt>
                                <dd className="font-heading text-3xl leading-none lining-nums tabular-nums"><Link href={href} className="hover:text-(--hero-ink)">{count.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")}</Link></dd>
                            </div>
                        ))}
                    </dl>
                ) : null}

                <div className="grid gap-3 md:ml-auto [&_a]:hover:text-(--hero-ink)">
                    <div className="flex flex-wrap items-center gap-x-5 gap-y-2.5 font-mono text-xs text-muted-foreground" aria-label={t.skills}>
                        {site.skills.map((skill) => (
                            <Link key={skill.label} href={routes.searchResults(skill.keyword)}>{skill.label}</Link>
                        ))}
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
        </section>
    )
}

function SignatureHero({ locale, site, overview, topPost, children }: HomeHeroProps & { children?: React.ReactNode }) {
    const t = messages[locale]
    const name = site.authorName.replace(/\s*[(（].*?[)）]\s*$/u, "")
    return <section data-home-hero={children ? "photograph" : "signature"} aria-labelledby="home-hero-name" className={cn(styles.container, "pt-8 pb-10")}>
        {children}
        <div className={cn("border-b border-foreground py-10 sm:py-14", children && "bg-background")}>
            <p className="mb-5 text-xs tracking-[.2em] text-primary">{t.personalJournal} / {t.hero}</p>
            <h1 id="home-hero-name" className="font-heading text-[clamp(3rem,9vw,6rem)] leading-tight tracking-[-.055em] wrap-anywhere">{name}<span className="ml-4 text-primary" aria-hidden="true">＊</span></h1>
            <p className="mt-5 font-heading text-xl">{t.tagline}</p>
            <p className="mt-4 max-w-xl text-sm leading-7 text-muted-foreground">{site.authorTitle} / {site.authorBio}</p>
            <div className="mt-6 flex flex-wrap items-center gap-x-7 gap-y-2 text-sm [&_a]:inline-flex [&_a]:min-h-11 [&_a]:items-center [&_a]:gap-2 [&_a]:hover:text-primary">
                <Link href={routes.blogRandom}><Shuffle size={16} aria-hidden="true" />{t.randomPost}</Link>
                <Link href={routes.photographyRandom}><Camera size={16} aria-hidden="true" />{t.randomPhoto}</Link>
                <Link href={routes.about}>{t.aboutMe}<ArrowUpRight size={16} aria-hidden="true" /></Link>
            </div>
            {overview ? <p className="mt-5 text-xs text-muted-foreground">{t.posts} {overview.postsCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")} / {t.categories} {overview.categoriesCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")} / {t.photography} {overview.photosCount.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")}</p> : null}
            {topPost ? <p className="mt-5 border-t border-border pt-5 text-sm"><span className="mr-3 text-xs text-primary">{t.pinned}</span><Link href={postHref(topPost)} className="hover:text-primary">{topPost.title}</Link></p> : null}
        </div>
    </section>
}
export function HomeHero(props: HomeHeroProps) {
    const { config } = useHomeDesign()
    const [failedPhoto, setFailedPhoto] = useState<string | null>(null)
    if (config.hero === "cover") return <CoverHero {...props} />
    if (config.hero === "signature") return <SignatureHero {...props} />
    if (!props.photo || failedPhoto === props.photo.id) return <>
        <SignatureHero {...props} />
        <p role="status" className={cn(styles.container, "pb-4 text-xs text-muted-foreground")}>{messages[props.locale].homePhotoFallback}</p>
    </>
    return <SignatureHero {...props}><HeroPhoto photo={props.photo} fullWidth onFailure={() => setFailedPhoto(props.photo!.id)} /></SignatureHero>
}
