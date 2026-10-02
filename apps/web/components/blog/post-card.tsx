import Link from "next/link"
import { CalendarDays, Clock } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
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

  if (viewMode === "list") {
    return (
      <Card className="border-0 bg-white/80 py-0 shadow-sm backdrop-blur-sm transition-shadow hover:shadow-lg dark:bg-slate-900/80">
        <CardContent className="p-4">
          <div className="flex items-start justify-between gap-3">
            <Link href={href} className="min-w-0">
              <h3 className="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400">{post.title}</h3>
              {post.summary ? (
                <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">{post.summary}</p>
              ) : null}
            </Link>
            {post.featured ? (
              <Badge className="border-0 bg-gradient-to-r from-yellow-400 to-orange-500 text-white">精选</Badge>
            ) : null}
          </div>
          <div className="mt-3 flex items-center justify-between text-xs text-muted-foreground">
            <span className="flex items-center gap-1">
              <CalendarDays className="size-3" />
              {formatDate(post.lastUpdateTime, "MM-DD")}
            </span>
            {post.categoryName ? <Badge variant="outline">{post.categoryName}</Badge> : null}
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="border-0 bg-white/80 shadow-sm backdrop-blur-sm transition-all hover:scale-[1.02] hover:shadow-lg dark:bg-slate-900/80">
      <CardHeader>
        {post.featured ? (
          <Badge className="mb-2 w-fit border-0 bg-gradient-to-r from-yellow-400 to-orange-500 text-white">
            精选
          </Badge>
        ) : null}
        <Link href={href}>
          <h3 className="line-clamp-2 text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400">
            {post.title}
          </h3>
        </Link>
        {post.summary ? (
          <p className="mt-2 line-clamp-3 text-sm text-muted-foreground">{post.summary}</p>
        ) : null}
      </CardHeader>
      <CardContent>
        <Separator className="mb-3" />
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <div className="flex items-center gap-4">
            <span className="flex items-center gap-1">
              <CalendarDays className="size-3" />
              {formatDate(post.lastUpdateTime, "MM-DD")}
            </span>
            <span className="flex items-center gap-1">
              <Clock className="size-3" />
              {readingMinutes(post.content ?? post.summary)} 分钟
            </span>
          </div>
          {post.categoryName ? <Badge variant="outline">{post.categoryName}</Badge> : null}
        </div>
      </CardContent>
    </Card>
  )
}
