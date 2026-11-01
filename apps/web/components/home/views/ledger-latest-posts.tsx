"use client"
import { useI18n } from "@/components/app-providers"
import { HomeSectionHeading } from "@/components/home/section-heading"
import { LedgerPostRow } from "@/components/home/views/ledger-post-row"
import type { PostResponse } from "@/lib/api/types"
import { groupHomePostsByYear } from "@/lib/home-variant"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
export function LedgerLatestPosts({ posts }: {
  posts: PostResponse[]
}) {
  const { t } = useI18n()
  if(posts.length === 0)
    return null
  const groups = groupHomePostsByYear(posts.slice(0, 8))
  return <section data-home-section="latestPosts" data-home-variant={"ledger"} className={homeSectionClassName} aria-labelledby="home-latest-title">
    <HomeSectionHeading id="home-latest-title" eyebrow={t.homeLooseLeaves} title={t.homeDeskLatestTitle} href={routes.blog} action={t.allPosts} />
    <div className="border-t border-foreground">
      {groups.map(group => <div key={group.year} className={"grid pt-6 not-first:mt-6 grid-cols-1 gap-5"}>
        <div className={"flex text-[11px] text-muted-foreground flex-row items-baseline gap-3"}>
          <span>{t.updated}</span><strong className={"font-heading font-medium tracking-[-.04em] text-foreground text-2xl"}>{group.year || "—"}</strong>
        </div>
        <ul className="min-w-0">
          {group.items.map(post => <LedgerPostRow key={post.id} post={post} />)}
        </ul>
      </div>)}
    </div>
  </section>
}
