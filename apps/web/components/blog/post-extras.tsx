"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"
import type { ReactNode } from "react"
import type { BlogVariant } from "@/lib/blog-variant"
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

const attributionAppearance = {
  editorial: "border border-border border-l-2 border-l-primary bg-card",
  studio: "rounded-md border border-border bg-muted/40 shadow-sm",
  library: "border-2 border-foreground bg-card shadow-[4px_4px_0_var(--border)]",
} satisfies Record<BlogVariant, string>

export function PostCopyright({
  title,
  permalink,
  appearance = "editorial",
  children,
}: {
  title: string
  permalink: string
  appearance?: BlogVariant
  children?: ReactNode
}) {
  const { locale, t } = useI18n()
  return (
    <section data-article-attribution className={cn("mt-8 min-w-0 overflow-hidden", attributionAppearance[appearance])}>
      <div className="space-y-5 p-5 sm:p-6">
        <header>
          <p className="mb-2 font-mono text-xs tracking-wide text-primary">{t.articleAttribution}</p>
          <h2 className={cn(headingClassName, "wrap-anywhere text-lg leading-relaxed")}>{title}</h2>
          <p className="mt-2 text-xs text-muted-foreground">{t.author}<span className="text-foreground">{getSite(locale).labName}</span></p>
        </header>
        <dl className="space-y-3 text-xs leading-relaxed sm:text-sm">
          <div className="grid min-w-0 gap-1 sm:grid-cols-[auto_minmax(0,1fr)] sm:gap-3">
            <dt className="font-medium text-muted-foreground">{t.permalink}</dt>
            <dd className="min-w-0">
              <Link href={permalink} className="break-all text-primary underline-offset-4 hover:underline">{permalink}</Link>
            </dd>
          </div>
          <div className="grid min-w-0 gap-1 sm:grid-cols-[auto_minmax(0,1fr)] sm:gap-3">
            <dt className="font-medium text-muted-foreground">{t.copyright}</dt>
            <dd className="text-muted-foreground">
              {t.licenseBefore}{" "}
              <a href="https://creativecommons.org/licenses/by-nc-nd/4.0/" target="_blank" rel="noreferrer" className="font-medium text-foreground underline decoration-border underline-offset-4 hover:text-primary">BY-NC-SA</a>{" "}
              {t.licenseAfter}
            </dd>
          </div>
        </dl>
      </div>
      {children ? <div className="border-t border-border bg-muted/30 px-2 py-1 sm:px-3">{children}</div> : null}
    </section>
  )
}
