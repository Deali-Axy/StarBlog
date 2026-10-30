"use client"

import Link from "next/link"
import { ArrowUpRight } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { LatestPostRow } from "@/components/home/latest-post-row"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { PostResponse, PublishedPostSummary } from "@/lib/api/types"
import { formatDate } from "@/lib/format"
import { groupHomePostsByYear } from "@/lib/home-variant"
import { postHref, routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function LatestPosts({ posts }: { posts: PostResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (posts.length === 0) return null
  const groups = groupHomePostsByYear(posts.slice(0, 8))
  const desk = variant === "desk"

  return <section className={homeSectionClassName} aria-labelledby="home-latest-title">
    <HomeSectionHeading id="home-latest-title" eyebrow={desk ? t.homeLooseLeaves : t.homeInProgress}
      title={desk ? t.homeDeskLatestTitle : t.homeLatestTitle} href={routes.blog} action={t.allPosts} />
    <div className="border-t border-foreground">
      {groups.map(group => <div key={group.year} className={cn("grid pt-6 not-first:mt-6", desk ? "grid-cols-1 gap-5" : "grid-cols-1 gap-4 sm:grid-cols-[6rem_minmax(0,1fr)] sm:gap-7")}>
        <div className={cn("flex text-[11px] text-muted-foreground", desk ? "flex-row items-baseline gap-3" : "flex-row items-baseline gap-2.5 sm:flex-col sm:items-start")}>
          <span>{t.updated}</span><strong className={cn("font-heading leading-none font-medium tracking-[-.04em] text-foreground", desk ? "text-2xl" : "text-[28px] sm:text-[40px]")}>{group.year || "—"}</strong>
        </div>
        <ul className="min-w-0">
          {group.items.map(post => <LatestPostRow key={post.id} post={post} />)}
        </ul>
      </div>)}
    </div>
  </section>
}

export function FeaturedPosts({ posts }: { posts: PublishedPostSummary[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (posts.length === 0) return null
  const desk = variant === "desk"
  const single = posts.length === 1

  return <section className={homeSectionClassName} aria-labelledby="home-featured-title">
    <HomeSectionHeading id="home-featured-title" eyebrow={desk ? t.homeReadingNotes : t.homeTheEdit}
      title={desk ? t.homeDeskFeaturedTitle : t.homeFeaturedTitle} href={routes.blog} action={t.allPosts} />
    <div className={cn("grid", desk ? "gap-5 px-1.5 pt-2 pb-6 sm:grid-cols-3 sm:gap-6" : "gap-6 sm:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)] sm:gap-y-7 min-[900px]:gap-x-10", single && "sm:grid-cols-1")}>
      {posts.slice(0, 3).map((post, index) => {
        const lead = index === 0
        return <article key={post.id} className={cn("min-w-0", desk
          ? "relative border border-border bg-card p-6 shadow-[4px_5px_0_var(--muted)] sm:p-5 min-[900px]:p-6"
          : lead ? "border-b border-border pb-7 sm:row-span-2 sm:border-r sm:border-b-0 sm:pr-6 sm:pb-0 min-[900px]:pr-10" : index > 1 && "border-t border-border pt-6 sm:pt-7",
          desk && index % 3 === 1 && "rotate-[.75deg] sm:translate-y-3 sm:rotate-[1.2deg]",
          desk && index % 3 === 2 && "-rotate-1",
          single && "max-w-[46rem] sm:row-auto sm:border-r-0 sm:pr-0"
        )}>
          <Link href={postHref(post)} className="group flex h-full flex-col items-start">
            <span className={cn("block font-heading leading-none tracking-[-.06em] text-primary italic", desk ? "mb-5 text-5xl sm:mb-7 sm:text-6xl" : lead ? "mb-6 text-[clamp(4.5rem,7vw,6.5rem)]" : "mb-3.5 text-2xl")} aria-hidden="true">{String(index + 1).padStart(2, "0")}.</span>
            <p className="mb-2.5 text-[11px] tracking-[.08em] wrap-anywhere text-primary">{post.categoryName || t.posts}</p>
            <h3 className={cn("font-heading leading-[1.55] font-semibold tracking-[-.025em] wrap-anywhere group-hover:text-primary", desk ? "text-2xl sm:text-[22px]" : lead ? "text-3xl sm:text-[clamp(1.75rem,3vw,2.5rem)]" : "text-[22px]")}>{post.title}</h3>
            {post.summary ? <p className={cn("mt-4 text-[13px] leading-[1.95] text-muted-foreground", desk ? "line-clamp-4" : lead ? "line-clamp-5" : "line-clamp-2 sm:line-clamp-3")}>{post.summary}</p> : null}
            <div className="mt-auto flex w-full items-center justify-between gap-4 pt-6 font-mono text-[11px] text-muted-foreground">
              <time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime, "YYYY.MM.DD")}</time>
              <ArrowUpRight size={16} aria-hidden="true" />
            </div>
          </Link>
        </article>
      })}
    </div>
  </section>
}
