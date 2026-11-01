"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { ArrowUpRight } from "lucide-react"
import type { BlogCollectionProps } from "../blog-collection-props"
export function ArchiveCompactPosts({ posts, selected, rowRefs, select, keyboardSelect }: BlogCollectionProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  return <div className={""}>
    {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} data-selected={selected?.id === post.id} className={"relative grid [grid-template-columns:30px_minmax(0,_1fr)] gap-x-2.5 [padding:18px_46px_18px_18px] min-w-0 [border-bottom:1px_solid_var(--border)] [&[data-selected=true]]:bg-accent [&[data-selected=true]]:[box-shadow:inset_3px_0_0_var(--primary)] [&:hover]:bg-muted [&_button]:block [&_button]:w-full [&_button]:text-left [&_button]:cursor-pointer [&_h2]:text-sm [&_h2]:[line-height:1.8] [&_h2]:font-medium [&_p]:mt-2 [&_p]:text-[.6875rem] [&_p]:[line-height:1.9] [&_p]:text-muted-foreground [&_p]:[display:-webkit-box] [&_p]:[-webkit-box-orient:vertical] [&_p]:[-webkit-line-clamp:2] [&_p]:overflow-hidden [&&]:py-3 [&&_p]:hidden max-[63.9375rem]:py-5 max-[63.9375rem]:[&_h2]:text-base max-[39.9375rem]:[grid-template-columns:24px_minmax(0,_1fr)] max-[39.9375rem]:gap-1.5 max-[39.9375rem]:pl-3 max-[39.9375rem]:[&_h2]:text-sm"}>
      <span className={"pt-[3px] [font:10px_ui-monospace,_monospace] text-muted-foreground"}>{String((query.page - 1) * query.pageSize + index + 1).padStart(3, "0")}</span>
      <div><h2 className={"wrap-anywhere font-bold"}><button type="button" ref={(element) => {
        if(element)
          rowRefs?.current.set(post.id, element)
        else
          rowRefs?.current.delete(post.id)
      }} aria-label={`${t.previewArticle}: ${post.title}`} aria-pressed={selected?.id === post.id} aria-controls="blog-archive-preview" onClick={(event) => select?.(post, event.currentTarget)} onKeyDown={(event) => keyboardSelect?.(event, index)}>
        {post.title}
      </button></h2>
        <div className={"flex flex-wrap items-center gap-2.5 mt-2 [font:9px_ui-monospace,_monospace] text-muted-foreground max-[39.9375rem]:text-[.5625rem]"}><span>{post.categoryName || t.posts}</span><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time></div>

      </div>
      <Link href={postHref(post)} className={"absolute top-4.5 right-3 flex items-center justify-center w-7 h-7 text-muted-foreground"} aria-label={`${t.readArticle}: ${post.title}`} title={t.readArticle}><ArrowUpRight className="size-4" /></Link>
    </article>)}
  </div>
}
