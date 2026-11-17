"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { ArrowUpRight } from "lucide-react"
import type { BlogCollectionProps } from "../blog-collection-props"
export function FolioExpandedPosts({ posts }: BlogCollectionProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  return <div className="grid grid-cols-12 gap-x-12 gap-y-8 pt-4 max-lg:gap-x-8 max-sm:block max-sm:space-y-5">
    {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className="group relative col-span-5 min-w-0 [&:nth-child(4n+1)]:col-span-7 [&:nth-child(4n+3)]:col-span-5 [&:nth-child(4n+4)]:col-span-7 [&:nth-child(4n+2)]:rounded-tr-[40px] [&:nth-child(4n+2)]:bg-muted/65">
      <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`} className="flex h-full min-h-80 flex-col items-start rounded-sm py-7 group-[&:nth-child(4n+2)]:px-7 max-sm:min-h-0 max-sm:py-6 max-sm:group-[&:nth-child(4n+2)]:px-6">
        <div className="mb-5 flex w-full items-center justify-between gap-4 text-[11px] tracking-[.06em] text-muted-foreground [&_b]:font-mono [&_b]:text-xs [&_b]:font-normal [&_b]:text-primary/70"><span>{post.categoryName || t.posts}</span><b aria-hidden="true">{String((query.page - 1) * query.pageSize + index + 1).padStart(2, "0")}</b></div>
        <h2 className="wrap-anywhere font-heading text-[28px] font-semibold leading-[1.65] tracking-[-.025em] transition-colors group-hover:text-primary lg:group-[&:nth-child(4n+1)]:text-[34px] max-sm:text-[24px]">{post.title}</h2>
        {post.summary ? <p className="mt-5 line-clamp-3 wrap-anywhere text-[13px] leading-[2.1] text-muted-foreground">{post.summary}</p> : null}
        <div className={"flex items-center justify-between gap-4 mt-auto pt-7 w-full text-[.625rem] text-muted-foreground [&_span]:inline-flex [&_span]:gap-1.5 [&_span]:items-center [&_span]:text-primary max-[39.9375rem]:text-[.5625rem]"}><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time><span>{t.readArticle}<ArrowUpRight className="size-3.5" aria-hidden="true" /></span></div>
      </Link>
    </article>)}
  </div>
}
