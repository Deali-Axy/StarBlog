"use client"

import Link from "next/link"
import { Share2 } from "lucide-react"
import { toast } from "sonner"

import { flattenToc, SidebarTOC } from "@/components/blog/sidebar-toc"
import { MarkdownContent } from "@/components/blog/markdown"
import { CommentSection } from "@/components/blog/comment-section"
import { PostCopyright, RelatedPosts } from "@/components/blog/post-extras"
import { Button } from "@/components/ui/button"
import type { PostResponse, PostTranslationResponse } from "@/lib/api/types"
import { postHref } from "@/lib/routes"
import { site } from "@/lib/site"
import { formatDate, readingMinutes } from "@/lib/format"

export function BlogPostRenderer({
  post,
  related = [],
  permalink,
  translations = [],
  lang,
}: {
  post: PostResponse
  related?: PostResponse[]
  permalink: string
  translations?: PostTranslationResponse[]
  lang?: string
}) {
  const toc = flattenToc(post.toc)

  function share() {
    const url = window.location.href
    if (navigator.share) {
      void navigator.share({ title: post.title, url }).catch(() => {})
      return
    }
    void navigator.clipboard.writeText(url)
    toast.success("链接已复制")
  }

  return (
    <div className="page-wrap grid gap-12 py-12 lg:grid-cols-[minmax(0,1fr)_16rem] lg:py-16">
      <article>
        <p className="kicker">{post.categoryName || "文章"}</p>
        <h1 className="mt-3 text-3xl leading-tight md:text-4xl">{post.title}</h1>
        {post.summary ? (
          <p className="mt-4 max-w-2xl text-lg leading-relaxed text-muted-foreground">{post.summary}</p>
        ) : null}
        <div className="mt-5 flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-muted-foreground">
          <span>{site.authorName}</span>
          <span>{formatDate(post.creationTime, "YYYY年MM月DD日")}</span>
          <span>约 {readingMinutes(post.content)} 分钟</span>
          <Button variant="ghost" size="sm" className="h-7 px-2" onClick={share}>
            <Share2 />
            分享
          </Button>
        </div>
        {translations.length > 0 ? (
          <div className="mt-4 flex flex-wrap gap-2">
            <Button variant={!lang ? "default" : "outline"} size="sm" asChild>
              <Link href={postHref(post)}>原文</Link>
            </Button>
            {translations.map((item) => (
              <Button
                key={item.language}
                variant={lang === item.language ? "default" : "outline"}
                size="sm"
                asChild
              >
                <Link href={`${postHref(post)}?lang=${encodeURIComponent(item.language)}`}>
                  {item.language}
                </Link>
              </Button>
            ))}
          </div>
        ) : null}

        <div className="mt-10 max-w-2xl">
          <MarkdownContent content={post.content ?? ""} />
        </div>
        <PostCopyright title={post.title} permalink={permalink} />
        <RelatedPosts posts={related} />
        <CommentSection postId={post.id} />
      </article>
      <aside className="hidden lg:block">
        <div className="sticky top-24">
          {toc.length > 0 ? <SidebarTOC items={toc} className="max-h-[70vh] overflow-y-auto" /> : null}
        </div>
      </aside>
    </div>
  )
}
