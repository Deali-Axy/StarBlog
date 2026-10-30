"use client"

import Link from "next/link"
import { ArrowDownRight, ArrowUpRight, Clock3, FileText } from "lucide-react"
import { motion, useReducedMotion } from "motion/react"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import type { PostResponse } from "@/lib/api/types"
import { formatDate, readingMinutes } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"

export function BlogArticleCard({ post, index }: { post: PostResponse; index: number }) {
  const { t } = useI18n()
  const { variant, query } = useBlogNavigation()
  const reducedMotion = useReducedMotion()
  const list = query.view === "list"
  const editorial = variant === "editorial"
  const studio = variant === "studio"
  const date = formatDate(post.lastUpdateTime)
  const ordinal = String((query.page - 1) * query.pageSize + index + 1).padStart(2, "0")

  return (
    <motion.article data-blog-article={post.id}
      whileHover={studio && !reducedMotion ? { y: -3 } : undefined}
      transition={{ duration: 0.18 }}
      className={cn("group relative min-w-0",
        variant === "library" && "rounded-2xl border border-border bg-card shadow-sm shadow-foreground/2 transition-shadow hover:shadow-lg hover:shadow-primary/5",
        editorial && "border-t border-foreground/25",
        studio && "overflow-hidden rounded-lg border border-border bg-linear-to-br from-card to-card/60 transition-colors hover:border-primary/55",
      )}>
      {studio ? <div aria-hidden="true" className="absolute inset-x-0 top-0 h-px bg-linear-to-r from-primary/60 via-primary/10 to-transparent" /> : null}
      <Link href={postHref(post)} className={cn("flex h-full min-w-0 flex-col p-6", list && "sm:flex-row sm:gap-6",
        editorial && "px-0 py-7", studio && "p-5", !list && "min-h-72")}
        aria-label={`${t.readArticle}: ${post.title}`}>
        {editorial && list ? (
          <div className="mb-4 flex shrink-0 items-center gap-2 text-xs text-muted-foreground sm:mb-0 sm:w-24 sm:flex-col sm:items-start sm:gap-1 sm:pt-1">
            <time dateTime={post.lastUpdateTime} className="tabular-nums">{date}</time>
            <span className="hidden text-3xl font-heading text-border sm:block" aria-hidden="true">{ordinal}</span>
          </div>
        ) : null}
        <div className="flex min-w-0 flex-1 flex-col">
          <div className={cn("mb-4 flex min-w-0 items-center justify-between gap-3 text-xs", studio && "font-mono")}>
            <span className={cn("min-w-0 truncate font-medium text-primary", variant === "library" && "rounded-md bg-accent/70 px-2 py-1", editorial && "tracking-wider")}>
              {post.categoryName || t.posts}
            </span>
            {studio ? <span className="shrink-0 text-muted-foreground/60" aria-hidden="true">/{ordinal}</span> : <ArrowUpRight aria-hidden="true" className="size-4 shrink-0 text-muted-foreground/50 transition-colors group-hover:text-primary" />}
          </div>
          <h2 className={cn("text-lg leading-relaxed font-semibold break-words transition-colors group-hover:text-primary [overflow-wrap:anywhere]",
            editorial && "font-heading text-2xl leading-snug", studio && "text-lg", list && "sm:text-xl")}>
            {post.title}
          </h2>
          {post.summary ? <p className={cn("mt-3 line-clamp-3 text-sm leading-7 text-muted-foreground [overflow-wrap:anywhere]", list && "line-clamp-2", editorial && "mt-4 leading-7")}>{post.summary}</p> : null}
          <div className={cn("mt-auto flex flex-wrap items-center gap-x-3 gap-y-2 pt-6 text-xs text-muted-foreground", studio && "font-mono", editorial && "pt-5")}>
            {!(editorial && list) ? <time dateTime={post.lastUpdateTime}>{date}</time> : null}
            <span className="inline-flex items-center gap-1.5"><Clock3 className="size-3.5" aria-hidden="true" />{t.readingTime(readingMinutes(post.content ?? post.summary))}</span>
            {studio ? <FileText className="ml-auto size-3.5 text-primary/60" aria-hidden="true" /> : null}
            {editorial ? <span className="ml-auto inline-flex items-center gap-2 text-primary">{t.readArticle}<ArrowDownRight className="size-3.5" aria-hidden="true" /></span> : null}
          </div>
        </div>
      </Link>
    </motion.article>
  )
}
