import Link from "next/link"

import { formatDate } from "@/lib/format"
import type { PostResponse } from "@/lib/api/types"
import { postHref } from "@/lib/routes"

type LatestPostRowModel = Pick<
  PostResponse,
  "id" | "title" | "slug" | "summary" | "categoryName" | "lastUpdateTime"
>

export function LatestPostRow({ post }: { post: LatestPostRowModel }) {
  return (
    <li>
      <Link
        href={postHref(post)}
        className="group flex flex-col gap-1 py-5 sm:flex-row sm:items-baseline sm:gap-8"
      >
        <time className="w-28 shrink-0 text-sm text-muted-foreground">
          {formatDate(post.lastUpdateTime)}
        </time>
        <div className="min-w-0 flex-1">
          <h3 className="text-lg leading-snug group-hover:text-primary">{post.title}</h3>
          {post.categoryName ? (
            <p className="mt-1 text-xs text-muted-foreground">{post.categoryName}</p>
          ) : null}
          {post.summary ? (
            <p className="mt-1.5 line-clamp-2 text-sm leading-relaxed text-muted-foreground">
              {post.summary}
            </p>
          ) : null}
        </div>
      </Link>
    </li>
  )
}
