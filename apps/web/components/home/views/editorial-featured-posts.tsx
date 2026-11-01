"use client"
import { useI18n } from "@/components/app-providers"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { PublishedPostSummary } from "@/lib/api/types"
import { formatDate } from "@/lib/format"
import { postHref, routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"
import { ArrowUpRight } from "lucide-react"
import Link from "next/link"
export function EditorialFeaturedPosts({ posts }: {
  posts: PublishedPostSummary[]
}) {
  const { t } = useI18n()
  if(posts.length === 0)
    return null
  const single = posts.length === 1
  return <section data-home-section="featuredPosts" data-home-variant={"editorial"} className={homeSectionClassName} aria-labelledby="home-featured-title">
    <HomeSectionHeading id="home-featured-title" eyebrow={t.homeTheEdit} title={t.homeFeaturedTitle} href={routes.blog} action={t.allPosts} />
    <div className={cn("grid", "gap-6 sm:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)] sm:gap-y-7 min-[56.25rem]:gap-x-10", single && "sm:grid-cols-1")}>
      {posts.map((post, index) => {
        const lead = index === 0
        return <article key={post.id} className={(cn("min-w-0", lead ? "border-b border-border pb-7 sm:row-span-2 sm:border-r sm:border-b-0 sm:pr-6 sm:pb-0 min-[56.25rem]:pr-10" : index > 1 && "border-t border-border pt-6 sm:pt-7", single && "max-w-[46rem] sm:row-auto sm:border-r-0 sm:pr-0"))}>
          <Link href={postHref(post)} className="group flex h-full flex-col items-start">
            <span className={cn("block font-heading leading-none tracking-[-.06em] text-primary italic", lead ? "mb-6 text-[clamp(4.5rem,7vw,6.5rem)]" : "mb-3.5 text-2xl")} aria-hidden="true">{String(index + 1).padStart(2, "0")}.</span>
            <p style={{ zoom: "var(--home-scale)" }} className="mb-2.5 text-[11px] tracking-[.08em] wrap-anywhere text-primary">{post.categoryName || t.posts}</p>
            <h3 style={{ zoom: "var(--home-scale)" }} className={cn("font-heading leading-[1.55] font-semibold tracking-[-.025em] wrap-anywhere group-hover:text-primary", lead ? "text-3xl sm:text-[clamp(1.75rem,3vw,2.5rem)]" : "text-[22px]")}>{post.title}</h3>
            {post.summary ? <p style={{ zoom: "var(--home-scale)" }} className={cn("mt-4 text-[13px] leading-[1.95] text-muted-foreground", lead ? "line-clamp-5" : "line-clamp-2 sm:line-clamp-3")}>{post.summary}</p> : null}
            <div className="mt-auto flex w-full items-center justify-between gap-4 pt-6 font-mono text-[11px] text-muted-foreground">
              <time style={{ zoom: "var(--home-scale)" }} dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime, "YYYY.MM.DD")}</time>
              <ArrowUpRight size={16} aria-hidden="true" />
            </div>
          </Link>
        </article>
      })}
    </div>
  </section>
}
