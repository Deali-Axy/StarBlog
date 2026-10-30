"use client"

import { useI18n } from "@/components/app-providers"

import { Newspaper } from "lucide-react"

import { EnhancedPostCard, type PostCardModel } from "@/components/blog/enhanced-post-card"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function BlogPostGrid({
  posts,
  title,
}: {
  posts: PostCardModel[]
  title?: string
}) {
  const { t } = useI18n()
  if (posts.length === 0) {
    return (
      <div className="py-16 text-center">
        <div className="mx-auto mb-6 flex size-20 items-center justify-center rounded-full bg-muted">
          <Newspaper className="size-10 text-muted-foreground" />
        </div>
        <h3 className={cn(headingClassName, "text-xl font-semibold")}>{t.noPosts}</h3>
        <p className="mt-2 text-muted-foreground">{t.noPublishedPosts}</p>
      </div>
    )
  }

  return (
    <div>
      {title ? <h2 className={cn(headingClassName, "mb-6 text-2xl")}>{title}</h2> : null}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {posts.map((post) => (
          <EnhancedPostCard key={post.id} post={post} />
        ))}
      </div>
    </div>
  )
}
