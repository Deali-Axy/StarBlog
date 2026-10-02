import Link from "next/link"

import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { Button } from "@/components/ui/button"
import { formatDate } from "@/lib/format"
import type { PostResponse, PublishedPostSummary } from "@/lib/api/types"
import { postHref, routes } from "@/lib/routes"

export function LatestPosts({ posts }: { posts: PostResponse[] }) {
  if (posts.length === 0) return null

  return (
    <section className="mt-20">
      <div className="mb-6 flex items-end justify-between gap-4">
        <div>
          <p className="kicker">最近记录</p>
          <h2 className="mt-2 text-2xl">最新文章</h2>
        </div>
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>全部文章</Link>
        </Button>
      </div>
      <ul className="divide-y divide-border/70 border-y border-border/70">
        {posts.slice(0, 8).map((post) => (
          <li key={post.id}>
            <Link href={postHref(post)} className="group flex flex-col gap-1 py-4 sm:flex-row sm:items-baseline sm:gap-8">
              <time className="w-28 shrink-0 text-sm text-muted-foreground">{formatDate(post.lastUpdateTime)}</time>
              <h3 className="text-lg leading-snug group-hover:text-primary">{post.title}</h3>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  )
}

export function FeaturedPosts({ posts }: { posts: PublishedPostSummary[] }) {
  if (posts.length === 0) return null

  return (
    <section className="mt-8">
      <div className="mb-8 flex items-end justify-between gap-4">
        <div>
          <p className="kicker">精选</p>
          <h2 className="mt-2 text-2xl">推荐阅读</h2>
        </div>
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>查看全部</Link>
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
