import Link from "next/link"

import { Badge } from "@/components/ui/badge"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn, coverGradient } from "@/lib/utils"

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
    <article
      className={cn(
        "group relative overflow-hidden rounded-2xl border border-border/70 bg-card shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-lg",
        featured && "hover:-translate-y-2 hover:shadow-2xl",
      )}
    >
      <Link href={href} className="absolute inset-0 z-10">
        <span className="sr-only">{post.title}</span>
      </Link>
      <div
        className={cn("relative overflow-hidden", featured ? "h-52" : "h-36")}
        style={{ background: coverGradient(post.title) }}
      >
        <div className="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent" />
        <div className="absolute top-3 left-3 z-20 flex gap-2">
          {featured ? (
            <Badge className="border-0 bg-gradient-to-r from-yellow-400 to-orange-500 text-white">精选</Badge>
          ) : null}
          {post.categoryName ? (
            <Badge variant="secondary" className="bg-white/90 text-gray-800 dark:bg-black/50 dark:text-white">
              {post.categoryName}
            </Badge>
          ) : null}
        </div>
      </div>
      <div className={cn("p-5", featured && "p-6")}>
        <h3
          className={cn(
            "font-semibold text-foreground group-hover:text-blue-600 dark:group-hover:text-blue-400",
            featured ? "text-xl leading-tight" : "text-lg",
          )}
        >
          {post.title}
        </h3>
        {post.summary ? (
          <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-muted-foreground">{post.summary}</p>
        ) : null}
        <p className="mt-4 text-xs text-muted-foreground">{formatDate(post.lastUpdateTime)}</p>
      </div>
    </article>
  )
}
