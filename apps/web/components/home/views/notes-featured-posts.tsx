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
export function NotesFeaturedPosts({ posts }: {
  posts: PublishedPostSummary[]
}) {
  const { t } = useI18n()
  if(posts.length === 0)
    return null
  const single = posts.length === 1
  return <section data-home-section="featuredPosts" data-home-variant={"notes"} className={homeSectionClassName} aria-labelledby="home-featured-title">
    <HomeSectionHeading id="home-featured-title" eyebrow={t.homeReadingNotes} title={t.homeDeskFeaturedTitle} href={routes.blog} action={t.allPosts} />
    <div className={cn("grid", "gap-5 px-1.5 pt-2 pb-6 sm:grid-cols-3 sm:gap-6", single && "sm:grid-cols-1")}>
      {posts.map((post, index) => {
        return <article data-home-surface={""} data-home-padding={""} key={post.id} className={(cn("min-w-0", "relative border border-border bg-card p-6 sm:p-5 min-[56.25rem]:p-6", index % 3 === 1 && "rotate-[.75deg] sm:translate-y-3 sm:rotate-[1.2deg]", index % 3 === 2 && "-rotate-1", single && "max-w-[46rem] sm:row-auto sm:border-r-0 sm:pr-0")) + " [box-shadow:var(--home-shadow)] p-[calc(1.5rem*var(--home-density))]"}>
          <Link href={postHref(post)} className="group flex h-full flex-col items-start">
            <span className={"block font-heading tracking-[-.06em] text-primary italic mb-5 text-5xl sm:mb-7 sm:text-6xl"} aria-hidden="true">{String(index + 1).padStart(2, "0")}.</span>
            <p style={{ zoom: "var(--home-scale)" }} className="mb-2.5 text-[11px] tracking-[.08em] wrap-anywhere text-primary">{post.categoryName || t.posts}</p>
            <h3 style={{ zoom: "var(--home-scale)" }} className={"font-heading font-semibold tracking-[-.025em] wrap-anywhere group-hover:text-primary text-2xl sm:text-[22px]"}>{post.title}</h3>
            {post.summary ? <p style={{ zoom: "var(--home-scale)" }} className={"mt-4 text-[13px] leading-[1.95] text-muted-foreground line-clamp-4"}>{post.summary}</p> : null}
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
