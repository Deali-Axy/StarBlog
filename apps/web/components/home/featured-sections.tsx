import Link from "next/link"
import { Clock, Star } from "lucide-react"

import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { Button } from "@/components/ui/button"
import { Marquee } from "@/components/ui/marquee"
import type { PostResponse, PublishedPostSummary } from "@/lib/api/types"
import { postHref, routes } from "@/lib/routes"

export function LatestMarquee({ posts }: { posts: PostResponse[] }) {
  if (posts.length === 0) return null

  return (
    <section className="relative overflow-hidden py-10">
      <div className="mb-6 flex items-center gap-3">
        <div className="flex size-8 items-center justify-center rounded-lg bg-gradient-to-r from-blue-500 to-purple-600">
          <Clock className="size-4 text-white" />
        </div>
        <h2 className="text-2xl font-bold">最新文章</h2>
        <div className="h-px flex-1 bg-gradient-to-r from-gray-300 to-transparent dark:from-gray-600" />
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>更多文章 →</Link>
        </Button>
      </div>
      <div className="relative">
        <div className="pointer-events-none absolute top-0 bottom-0 left-0 z-10 w-16 bg-gradient-to-r from-background to-transparent" />
        <div className="pointer-events-none absolute top-0 right-0 bottom-0 z-10 w-16 bg-gradient-to-l from-background to-transparent" />
        <Marquee pauseOnHover className="[--duration:50s]">
          {posts.map((post) => (
            <Link
              key={post.id}
              href={postHref(post)}
              className="w-72 rounded-xl border border-border/70 bg-card p-4 shadow-sm"
            >
              <h3 className="line-clamp-2 font-semibold">{post.title}</h3>
              <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">{post.summary || "暂无摘要"}</p>
            </Link>
          ))}
        </Marquee>
      </div>
    </section>
  )
}

export function FeaturedPosts({ posts }: { posts: PublishedPostSummary[] }) {
  if (posts.length === 0) return null

  return (
    <section className="mb-16">
      <div className="mb-8 flex items-center gap-3">
        <div className="flex size-8 items-center justify-center rounded-lg bg-gradient-to-r from-yellow-400 to-orange-500">
          <Star className="size-4 text-white" />
        </div>
        <h2 className="text-2xl font-bold">精选推荐</h2>
        <span className="text-sm text-muted-foreground">({posts.length})</span>
        <div className="h-px flex-1 bg-gradient-to-r from-gray-300 to-transparent dark:from-gray-600" />
        <Button variant="outline" size="sm" asChild>
          <Link href={routes.blog}>查看全部</Link>
        </Button>
      </div>
      <div className="grid grid-cols-1 gap-8 md:grid-cols-2 xl:grid-cols-4">
        {posts.map((post) => (
          <EnhancedPostCard
            key={post.id}
            variant="featured"
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
