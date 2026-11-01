"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { ArrowUpRight } from "lucide-react"
import type { BlogCollectionProps } from "../blog-collection-props"
export function IndexExpandedPosts({ posts }: BlogCollectionProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  return <div>      <div className={"grid [grid-template-columns:130px_minmax(0,_1fr)_280px_24px] gap-6 py-3.5 [border-block:1px_solid_var(--foreground)] text-[.625rem] [letter-spacing:.08em] text-muted-foreground max-[74.9375rem]:[grid-template-columns:100px_minmax(0,_1fr)_220px_20px] max-[74.9375rem]:gap-5 max-[63.9375rem]:[grid-template-columns:90px_minmax(0,_1fr)_24px] max-[63.9375rem]:[&_span:nth-child(3)]:hidden max-[39.9375rem]:[grid-template-columns:45px_minmax(0,_1fr)_20px] max-[39.9375rem]:gap-3 max-[39.9375rem]:py-3 max-[39.9375rem]:[&_span:nth-child(3)]:hidden max-[39.9375rem]:[&_>_span:first-child]:text-[0] max-[39.9375rem]:[&_>_span:first-child::after]:[content:attr(data-mobile-label)] max-[39.9375rem]:[&_>_span:first-child::after]:text-[.625rem]"} aria-hidden="true"><span data-mobile-label={t.indexRecordShort}>{t.indexRecord}</span><span>{t.title}</span><span>{t.articleAbstract}</span><span>↗</span></div>
    {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className={"[border-bottom:1px_solid_var(--border)] [&_>_a]:grid [&_>_a]:[grid-template-columns:130px_minmax(0,_1fr)_280px_24px] [&_>_a]:gap-6 [&_>_a]:items-start [&_>_a]:py-7.5 [&_>_a]:[transition:background_.18s] [&_>_a:hover]:bg-muted [&_h2]:text-[26px] [&_h2]:[letter-spacing:-.025em] [&_h2]:[line-height:1.6] [&_p]:text-xs [&_p]:[line-height:2] [&_p]:text-muted-foreground max-[74.9375rem]:[&_>_a]:[grid-template-columns:100px_minmax(0,_1fr)_220px_20px] max-[74.9375rem]:[&_>_a]:gap-5 max-[74.9375rem]:[&_h2]:text-[23px] max-[63.9375rem]:[&_>_a]:[grid-template-columns:90px_minmax(0,_1fr)_24px] max-[63.9375rem]:[&_h2]:text-[25px] max-[63.9375rem]:[&_p]:[grid-column:2] max-[63.9375rem]:[&_p]:[grid-row:2] max-[39.9375rem]:[&_>_a]:[grid-template-columns:45px_minmax(0,_1fr)_20px] max-[39.9375rem]:[&_>_a]:gap-3 max-[39.9375rem]:[&_>_a]:py-6 max-[39.9375rem]:[&_h2]:text-[21px] max-[39.9375rem]:[&_h2]:[line-height:1.65] max-[39.9375rem]:[&_p]:text-[.6875rem] max-[39.9375rem]:[&_p]:[line-height:2] group/article"}>
      <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`}>
        <div className={"flex flex-col items-start gap-2.5 text-[.625rem] text-muted-foreground wrap-anywhere [&_b]:[font:11px_ui-monospace,_monospace] [&_b]:text-foreground [&_b]:mb-3 max-[63.9375rem]:[grid-row:span_2] max-[39.9375rem]:gap-2 max-[39.9375rem]:text-[.5625rem] max-[39.9375rem]:[&_time]:[writing-mode:vertical-rl] max-[39.9375rem]:[&_time]:[letter-spacing:.04em] max-[39.9375rem]:[&_b]:mb-0 max-[39.9375rem]:[&_b]:text-[.625rem]"}>
          <b>{String((query.page - 1) * query.pageSize + index + 1).padStart(3, "0")}</b>
          <span>{post.categoryName || t.posts}</span>
          <time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time>
        </div>
        <h2 className={"wrap-anywhere font-bold"}>{post.title}</h2>
        {post.summary ? <p className={"wrap-anywhere line-clamp-4"}>{post.summary}</p> : null}
        <ArrowUpRight className={"[grid-column:4] w-5.5 h-5.5 mt-[5px] [transition:transform_.18s] max-[63.9375rem]:[grid-column:3] max-[63.9375rem]:[grid-row:1] max-[39.9375rem]:w-4.5"} aria-hidden="true" />
      </Link>
    </article>)}</div>
}
