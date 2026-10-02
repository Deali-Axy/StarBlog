import Link from "next/link"

import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"

export interface PostCardModel {
  id: string
  title: string
  slug?: string | null
  summary?: string | null
  categoryName?: string | null
  creationTime?: string
  lastUpdateTime: string
  featured?: boolean
}

interface EnhancedPostCardProps {
  post: PostCardModel
  variant?: "default" | "featured"
}

export function EnhancedPostCard({ post, variant = "default" }: EnhancedPostCardProps) {
  const featured = variant === "featured" || post.featured
  const href = postHref(post)

  return (
    <article className={cn("group border-t border-border/80 pt-5", featured && "pt-0 border-t-0")}>
      <Link href={href} className="block">
        <p className="kicker">{post.categoryName || (featured ? "置顶" : "文章")}</p>
        <h3 className={cn("mt-2 group-hover:text-primary", featured ? "text-2xl leading-snug" : "text-lg")}>
          {post.title}
        </h3>
        {post.summary ? (
          <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-muted-foreground">{post.summary}</p>
        ) : null}
        <p className="mt-3 text-xs text-muted-foreground">{formatDate(post.lastUpdateTime)}</p>
      </Link>
    </article>
  )
}
