"use client"

import Link from "next/link"
import { useId, useState } from "react"
import { ArrowUpRight, Plus } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { formatDate } from "@/lib/format"
import type { PostResponse } from "@/lib/api/types"
import { postHref } from "@/lib/routes"
import styles from "./home-sections.module.css"

type LatestPostRowModel = Pick<PostResponse, "id" | "title" | "slug" | "summary" | "categoryName" | "lastUpdateTime">

export function LatestPostRow({ post }: { post: LatestPostRowModel }) {
  const { t } = useI18n()
  const [expanded, setExpanded] = useState(false)
  const summaryId = useId()
  return <li className={styles.latestRow} data-expanded={expanded || undefined}>
    <div className={styles.latestRowHeading}>
      <time dateTime={post.lastUpdateTime} className={styles.date}>{formatDate(post.lastUpdateTime, "MM.DD")}</time>
      <div className={styles.latestTitle}>
        <h3><Link href={postHref(post)}>{post.title}</Link></h3>
        {post.categoryName ? <p className={styles.latestCategory}>{post.categoryName}</p> : null}
      </div>
      {post.summary ? <button type="button" aria-label={`${t.articlePreview}: ${post.title}`} aria-expanded={expanded}
        aria-controls={summaryId} className={styles.expandButton} onClick={() => setExpanded(!expanded)}>
        <Plus size={18} aria-hidden="true" />
      </button> : <Link href={postHref(post)} className={styles.expandButton} aria-label={`${t.readArticle}: ${post.title}`}><ArrowUpRight size={17} aria-hidden="true" /></Link>}
    </div>
    {post.summary ? <div id={summaryId} hidden={!expanded} className={styles.articleSummary}>
      <p>{post.summary}</p>
      <Link href={postHref(post)}>{t.readArticle}<ArrowUpRight size={14} aria-hidden="true" /></Link>
    </div> : null}
  </li>
}
