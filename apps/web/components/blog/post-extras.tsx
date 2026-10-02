import Link from "next/link"

import { PostCard } from "@/components/blog/post-card"
import type { PostResponse } from "@/lib/api/types"

export function RelatedPosts({ posts }: { posts: PostResponse[] }) {
  if (posts.length === 0) return null

  return (
    <section className="mt-10">
      <h2 className="mb-4 text-xl font-semibold">相关文章</h2>
      <div className="grid gap-4 md:grid-cols-2">
        {posts.map((post) => (
          <PostCard
            key={post.id}
            post={{
              id: post.id,
              title: post.title,
              slug: post.slug,
              summary: post.summary,
              categoryName: post.categoryName,
              lastUpdateTime: post.lastUpdateTime,
              content: post.content,
            }}
          />
        ))}
      </div>
    </section>
  )
}

export function PostCopyright({
  title,
  permalink,
}: {
  title: string
  permalink: string
}) {
  return (
    <section className="mt-16 border-t border-border/70 pt-8">
      <p className="mb-4 font-mono text-sm text-muted-foreground">_EOF_</p>
      <h2 className="text-lg">{title}</h2>
      <dl className="mt-4 space-y-2 text-sm">
        <div>
          <dt className="inline font-medium">本文作者：</dt>
          <dd className="inline">程序设计实验室</dd>
        </div>
        <div>
          <dt className="inline font-medium">本文链接：</dt>
          <dd className="inline">
            <Link href={permalink} className="break-all text-primary hover:underline">
              {permalink}
            </Link>
          </dd>
        </div>
        <div>
          <dt className="inline font-medium">版权声明：</dt>
          <dd className="inline">
            本博客所有文章除特别声明外，均采用{" "}
            <a href="https://creativecommons.org/licenses/by-nc-nd/4.0/" target="_blank" rel="noreferrer">
              BY-NC-SA
            </a>{" "}
            许可协议。转载请注明出处！
          </dd>
        </div>
      </dl>
    </section>
  )
}
