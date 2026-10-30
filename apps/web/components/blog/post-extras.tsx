"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"
import { getSite } from "@/lib/site"

import { PostCard } from "@/components/blog/post-card"
import type { PostResponse } from "@/lib/api/types"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function RelatedPosts({ posts }: { posts: PostResponse[] }) {
  const { t } = useI18n()
  if (posts.length === 0) return null

  return (
    <section className="mt-10">
      <h2 className={cn(headingClassName, "mb-4 text-xl font-semibold")}>{t.relatedPosts}</h2>
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
  const { locale, t } = useI18n()
  return (
    <section className="mt-16 border-t border-border/70 pt-8">
      <p className="mb-4 font-mono text-sm text-muted-foreground">_EOF_</p>
      <h2 className={cn(headingClassName, "text-lg")}>{title}</h2>
      <dl className="mt-4 space-y-2 text-sm">
        <div>
          <dt className="inline font-medium">{t.author}</dt>
          <dd className="inline">{getSite(locale).labName}</dd>
        </div>
        <div>
          <dt className="inline font-medium">{t.permalink}</dt>
          <dd className="inline">
            <Link href={permalink} className="break-all text-primary hover:underline">
              {permalink}
            </Link>
          </dd>
        </div>
        <div>
          <dt className="inline font-medium">{t.copyright}</dt>
          <dd className="inline">
            {t.licenseBefore}{" "}
            <a href="https://creativecommons.org/licenses/by-nc-nd/4.0/" target="_blank" rel="noreferrer">
              BY-NC-SA
            </a>{" "}
            {t.licenseAfter}
          </dd>
        </div>
      </dl>
    </section>
  )
}
