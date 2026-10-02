import { Newspaper } from "lucide-react"

import { EnhancedPostCard, type PostCardModel } from "@/components/blog/enhanced-post-card"

export function BlogPostGrid({
  posts,
  title,
}: {
  posts: PostCardModel[]
  title?: string
}) {
  if (posts.length === 0) {
    return (
      <div className="py-16 text-center">
        <div className="mx-auto mb-6 flex size-20 items-center justify-center rounded-full bg-muted">
          <Newspaper className="size-10 text-muted-foreground" />
        </div>
        <h3 className="text-xl font-semibold">暂无文章</h3>
        <p className="mt-2 text-muted-foreground">还没有发布任何文章</p>
      </div>
    )
  }

  return (
    <div>
      {title ? <h2 className="mb-6 text-2xl">{title}</h2> : null}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {posts.map((post) => (
          <EnhancedPostCard key={post.id} post={post} />
        ))}
      </div>
    </div>
  )
}
