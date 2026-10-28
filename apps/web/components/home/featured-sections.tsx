"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { LatestPostRow } from "@/components/home/latest-post-row"
import { Button } from "@/components/ui/button"
import type { PostResponse, PublishedPostSummary } from "@/lib/api/types"
import { routes } from "@/lib/routes"

export function LatestPosts({ posts }: { posts: PostResponse[] }) {
  const { t } = useI18n()
  if (posts.length === 0) return null

  return (
    <section className="mt-20">
      <div className="mb-6 flex items-end justify-between gap-4">
        <div>
          <p className="kicker">{t.latest}</p>
          <h2 className="mt-2 text-2xl">{t.latestPosts}</h2>
        </div>
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>{t.allPosts}</Link>
        </Button>
      </div>
      <ul className="divide-y divide-border/70 border-y border-border/70">
        {posts.slice(0, 8).map((post) => (
          <LatestPostRow key={post.id} post={post} />
        ))}
      </ul>
    </section>
  )
}

export function FeaturedPosts({ posts }: { posts: PublishedPostSummary[] }) {
  const { t } = useI18n()
  if (posts.length === 0) return null

  return (
    <section className="mt-8">
      <div className="mb-8 flex items-end justify-between gap-4">
        <div>
          <p className="kicker">{t.featured}</p>
          <h2 className="mt-2 text-2xl">{t.recommended}</h2>
        </div>
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>{t.viewAll}</Link>
        </Button>
      </div>
      <div className="grid grid-cols-1 gap-10 md:grid-cols-2">
        {posts.map((post) => (
          <EnhancedPostCard
            key={post.id}
            post={{
              id: post.id,
              title: post.title,
              slug: post.slug,
              summary: post.summary,
              categoryName: post.categoryName,
              lastUpdateTime: post.lastUpdateTime,
              featured: true,
            }}
          />
        ))}
      </div>
    </section>
  )
}
