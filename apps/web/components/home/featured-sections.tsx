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
import styles from "./home-sections.module.css"

export function LatestPosts({ posts }: { posts: PostResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (posts.length === 0) return null
  const groups = groupHomePostsByYear(posts.slice(0, 8))

  return <section className={styles.section} aria-labelledby="home-latest-title">
    <HomeSectionHeading id="home-latest-title" eyebrow={variant === "desk" ? t.homeLooseLeaves : t.homeInProgress}
      title={variant === "desk" ? t.homeDeskLatestTitle : t.homeLatestTitle} href={routes.blog} action={t.allPosts} />
    <div className={styles.stream}>
      {groups.map(group => <div key={group.year} className={styles.yearGroup}>
        <div className={styles.yearLabel}><span>{t.updated}</span><strong>{group.year || "—"}</strong></div>
        <ul className={styles.latestList}>
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

  return <section className={styles.section} aria-labelledby="home-featured-title">
    <HomeSectionHeading id="home-featured-title" eyebrow={variant === "desk" ? t.homeReadingNotes : t.homeTheEdit}
      title={variant === "desk" ? t.homeDeskFeaturedTitle : t.homeFeaturedTitle} href={routes.blog} action={t.allPosts} />
    <div className={styles.featuredGrid} data-single={posts.length === 1 || undefined}>
      {posts.slice(0, 3).map((post, index) => <article key={post.id} className={styles.featuredArticle}>
        <Link href={postHref(post)} className={styles.featuredLink}>
          <span className={styles.folio} aria-hidden="true">{String(index + 1).padStart(2, "0")}.</span>
          <p className={styles.category}>{post.categoryName || t.posts}</p>
          <h3>{post.title}</h3>
          {post.summary ? <p className={styles.featuredSummary}>{post.summary}</p> : null}
          <div className={styles.featuredMeta}>
            <time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime, "YYYY.MM.DD")}</time>
            <ArrowUpRight size={16} aria-hidden="true" />
          </div>
        </Link>
      </article>)}
    </div>
  </section>
}
