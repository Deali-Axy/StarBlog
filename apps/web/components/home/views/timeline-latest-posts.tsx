"use client"
import { useI18n } from "@/components/app-providers"
import { HomeSectionHeading } from "@/components/home/section-heading"
import { TimelinePostRow } from "@/components/home/views/timeline-post-row"
import type { PostResponse } from "@/lib/api/types"
import { groupHomePostsByYear } from "@/lib/home-variant"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
export function TimelineLatestPosts({ posts }: {
  posts: PostResponse[]
}) {
  const { t } = useI18n()
  if(posts.length === 0)
    return null
  const groups = groupHomePostsByYear(posts.slice(0, 8))
  return <section data-home-section="latestPosts" data-home-variant={"timeline"} className={homeSectionClassName} aria-labelledby="home-latest-title">
    <HomeSectionHeading id="home-latest-title" eyebrow={t.homeInProgress} title={t.homeLatestTitle} href={routes.blog} action={t.allPosts} />
    <div className="border-t border-foreground">
      {groups.map(group => <div key={group.year} className={"grid pt-6 not-first:mt-6 grid-cols-1 gap-4 sm:grid-cols-[6rem_minmax(0,1fr)] sm:gap-7"}>
        <div className={"flex text-[11px] text-muted-foreground flex-row items-baseline gap-2.5 sm:flex-col sm:items-start"}>
          <span>{t.updated}</span><strong className={"font-heading font-medium tracking-[-.04em] text-foreground text-[28px] sm:text-[40px]"}>{group.year || "—"}</strong>
        </div>
        <ul className="min-w-0">
          {group.items.map(post => <TimelinePostRow key={post.id} post={post} />)}
        </ul>
      </div>)}
    </div>
  </section>
}
