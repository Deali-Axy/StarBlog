import Link from "next/link"

import type { PostCardModel } from "@/components/blog/enhanced-post-card"
import { formatDate, readingMinutes } from "@/lib/format"
import { postHref } from "@/lib/routes"

export function PostCard({
  post,
  viewMode = "grid",
}: {
  post: PostCardModel & { content?: string | null }
  viewMode?: "grid" | "list"
}) {
  const href = postHref(post)

  return (
    <article className={viewMode === "list" ? "border-b border-border/70 py-5" : "border-t border-border/70 pt-5"}>
      <Link href={href} className="group block">
        <div className="flex items-baseline justify-between gap-3">
          <h3 className="text-lg leading-snug group-hover:text-primary">{post.title}</h3>
          {post.featured ? <span className="kicker shrink-0">精选</span> : null}
        </div>
        {post.summary ? (
          <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-muted-foreground">{post.summary}</p>
        ) : null}
        <p className="mt-3 text-xs text-muted-foreground">
          {formatDate(post.lastUpdateTime)}
          {post.categoryName ? ` · ${post.categoryName}` : ""}
          {` · ${readingMinutes(post.content ?? post.summary)} 分钟`}
        </p>
      </Link>
    </article>
  )
}
