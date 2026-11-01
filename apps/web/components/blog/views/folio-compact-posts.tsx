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
  return <div className={"[grid-template-columns:repeat(12,_minmax(0,_1fr))] gap-x-16 gap-y-0 max-[63.9375rem]:gap-x-8 max-[39.9375rem]:block block"}>
    {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className={"[grid-column:span_5] relative min-w-0 [border-bottom:1px_solid_var(--border)] [&:nth-child(4n+1)]:[grid-column:span_7] [&:nth-child(4n+3)]:[grid-column:span_4] [&:nth-child(4n+4)]:[grid-column:span_8] [&_>_a]:flex [&_>_a]:flex-col [&_>_a]:items-start [&_>_a]:min-h-[350px] [&_>_a]:h-full [&_>_a]:[padding-block:34px_30px] [&:nth-child(4n+2)_>_a]:[margin-block:24px] [&:nth-child(4n+2)_>_a]:[padding:28px_32px] [&:nth-child(4n+2)_>_a]:[min-height:calc(100%_-_48px)] [&:nth-child(4n+2)_>_a]:h-auto [&:nth-child(4n+2)_>_a]:bg-muted [&:nth-child(4n+2)_>_a]:[border-radius:0_56px_0_0] [&_h2]:[font:600_30px/1.65_var(--font-heading)] [&_h2]:[letter-spacing:-.025em] [&:nth-child(4n+1)_h2]:text-[37px] [&_p]:mt-5.5 [&_p]:text-[.8125rem] [&_p]:[line-height:2.15] [&_p]:text-muted-foreground [&_a:hover_h2]:[text-decoration:underline] [&_a:hover_h2]:[text-decoration-thickness:1px] [&_a:hover_h2]:[text-underline-offset:7px] [&&_>_a]:grid [&&_>_a]:[grid-template-columns:120px_minmax(0,_1fr)] [&&_>_a]:[gap:0_40px] [&&_>_a]:min-h-0 [&&_>_a]:[margin:0] [&&_>_a]:[padding:26px_0] [&&_>_a]:[background:transparent] [&&:nth-child(4n+2)_>_a]:grid [&&:nth-child(4n+2)_>_a]:[grid-template-columns:120px_minmax(0,_1fr)] [&&:nth-child(4n+2)_>_a]:[gap:0_40px] [&&:nth-child(4n+2)_>_a]:min-h-0 [&&:nth-child(4n+2)_>_a]:[margin:0] [&&:nth-child(4n+2)_>_a]:[padding:26px_0] [&&:nth-child(4n+2)_>_a]:[background:transparent] [&&_h2]:text-[1.75rem] [&&_p]:mt-3 max-[63.9375rem]:[&_h2]:text-[1.75rem] max-[63.9375rem]:[&:nth-child(4n+1)_h2]:text-[1.75rem] max-[63.9375rem]:[&:nth-child(4n+2)_>_a]:px-6 max-[39.9375rem]:[&_>_a]:min-h-0 max-[39.9375rem]:[&_>_a]:py-7 max-[39.9375rem]:[&:nth-child(4n+2)_>_a]:min-h-0 max-[39.9375rem]:[&:nth-child(4n+2)_>_a]:p-6 max-[39.9375rem]:[&:nth-child(4n+2)_>_a]:[margin-block:20px] max-[39.9375rem]:[&_h2]:text-[27px] max-[39.9375rem]:[&:nth-child(4n+1)_h2]:text-[27px] max-[39.9375rem]:[&_p]:text-xs max-[39.9375rem]:[&_p]:mt-4 max-[39.9375rem]:[&&_>_a]:[grid-template-columns:42px_minmax(0,_1fr)] max-[39.9375rem]:[&&_>_a]:[gap:0_16px] max-[39.9375rem]:[&&:nth-child(4n+2)_>_a]:[grid-template-columns:42px_minmax(0,_1fr)] max-[39.9375rem]:[&&:nth-child(4n+2)_>_a]:[gap:0_16px] max-[39.9375rem]:[&&_h2]:text-[23px]"}>
      <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`}>
        <div className={"flex items-center justify-between gap-5 w-full mb-[25px] text-muted-foreground [&_span]:text-[.6875rem] [&_span]:[letter-spacing:.06em] [&_b]:[font:500_34px/1_var(--font-heading)] [&_b]:text-primary [&&]:[grid-row:span_3] [&&]:flex-col [&&]:items-start [&&]:justify-start [&&]:gap-5 max-[39.9375rem]:mb-5 max-[39.9375rem]:[&&]:gap-4 max-[39.9375rem]:[&&_span]:text-[.5625rem] max-[39.9375rem]:[&&_span]:[writing-mode:vertical-rl]"}><span>{post.categoryName || t.posts}</span><b aria-hidden="true">{String((query.page - 1) * query.pageSize + index + 1).padStart(2, "0")}</b></div>
        <h2 className={"wrap-anywhere font-bold"}>{post.title}</h2>
        {post.summary ? <p className={"wrap-anywhere line-clamp-5"}>{post.summary}</p> : null}
        <div className={"flex items-center justify-between gap-4 mt-auto pt-7 w-full text-[.625rem] text-muted-foreground [&_span]:inline-flex [&_span]:gap-1.5 [&_span]:items-center [&_span]:text-primary [&&]:[grid-column:2] max-[39.9375rem]:text-[.5625rem]"}><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time><span>{t.readArticle}<ArrowUpRight className="size-3.5" aria-hidden="true" /></span></div>
      </Link>
    </article>)}
  </div>
}
