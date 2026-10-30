"use client"

import Link from "next/link"
import { useId, useState } from "react"
import { ArrowUpRight, Plus } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { formatDate } from "@/lib/format"
import type { PostResponse } from "@/lib/api/types"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"

const expandButtonClassName = "inline-flex size-11 cursor-pointer items-center justify-center text-primary hover:bg-accent"

type LatestPostRowModel = Pick<PostResponse, "id" | "title" | "slug" | "summary" | "categoryName" | "lastUpdateTime">

export function LatestPostRow({ post }: { post: LatestPostRowModel }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  const [expanded, setExpanded] = useState(false)
  const summaryId = useId()
  return <li className={cn("group border-b border-border py-4 first:pt-0", variant === "desk" && "border-dashed")} data-expanded={expanded || undefined}>
    <div className="grid grid-cols-[2.5rem_minmax(0,1fr)_2.75rem] items-start gap-2 sm:grid-cols-[3rem_minmax(0,1fr)_2.75rem] sm:gap-3">
      <time dateTime={post.lastUpdateTime} className="pt-2 font-mono text-[11px] text-muted-foreground group-hover:text-primary group-focus-within:text-primary">{formatDate(post.lastUpdateTime, "MM.DD")}</time>
      <div className="min-w-0 pt-1">
        <h3 className="font-heading text-lg leading-[1.65] font-medium wrap-anywhere"><Link href={postHref(post)} className="hover:text-primary">{post.title}</Link></h3>
        {post.categoryName ? <p className="mt-2 text-[11px] text-muted-foreground">{post.categoryName}</p> : null}
      </div>
      {post.summary ? <button type="button" aria-label={`${t.articlePreview}: ${post.title}`} aria-expanded={expanded}
        aria-controls={summaryId} className={expandButtonClassName} onClick={() => setExpanded(!expanded)}>
        <Plus className={cn("transition-transform duration-160 motion-reduce:transition-none", expanded && "rotate-45")} size={18} aria-hidden="true" />
      </button> : <Link href={postHref(post)} className={expandButtonClassName} aria-label={`${t.readArticle}: ${post.title}`}><ArrowUpRight size={17} aria-hidden="true" /></Link>}
    </div>
    {post.summary ? <div id={summaryId} hidden={!expanded} className="pt-3 pr-1 pb-1 pl-12 sm:pr-14 sm:pl-15">
      <p className="text-[13px] leading-[1.95] wrap-anywhere text-muted-foreground">{post.summary}</p>
      <Link href={postHref(post)} className="mt-1.5 inline-flex min-h-11 items-center gap-1.5 text-xs text-primary">{t.readArticle}<ArrowUpRight size={14} aria-hidden="true" /></Link>
    </div> : null}
  </li>
}
