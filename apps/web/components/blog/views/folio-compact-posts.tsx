"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { ArrowUpRight } from "lucide-react"
import type { BlogCollectionProps } from "../blog-collection-props"
export function FolioCompactPosts({ posts }: BlogCollectionProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  return <div className="space-y-2 pt-4">
    {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className="group min-w-0 rounded-xl px-5 transition-colors hover:bg-card max-sm:px-2">
      <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`} className="grid grid-cols-[88px_minmax(0,1fr)] gap-x-8 py-7 max-sm:grid-cols-[32px_minmax(0,1fr)] max-sm:gap-x-4 max-sm:py-5">
        <div className="row-span-3 flex flex-col items-start gap-4 text-muted-foreground [&_span]:text-[10px] [&_b]:font-mono [&_b]:text-sm [&_b]:font-normal [&_b]:text-primary/70 max-sm:[&_span]:[writing-mode:vertical-rl]"><span>{post.categoryName || t.posts}</span><b aria-hidden="true">{String((query.page - 1) * query.pageSize + index + 1).padStart(2, "0")}</b></div>
        <h2 className="wrap-anywhere font-heading text-[26px] font-semibold leading-[1.65] transition-colors group-hover:text-primary max-sm:text-[22px]">{post.title}</h2>
        {post.summary ? <p className="mt-3 line-clamp-2 wrap-anywhere text-[13px] leading-[2] text-muted-foreground">{post.summary}</p> : null}
        <div className={"flex items-center justify-between gap-4 mt-auto pt-7 w-full text-[.625rem] text-muted-foreground [&_span]:inline-flex [&_span]:gap-1.5 [&_span]:items-center [&_span]:text-primary col-start-2 max-[39.9375rem]:text-[.5625rem]"}><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time><span>{t.readArticle}<ArrowUpRight className="size-3.5" aria-hidden="true" /></span></div>
      </Link>
    </article>)}
  </div>
}
