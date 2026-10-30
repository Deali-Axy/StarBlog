"use client"

import Link from "next/link"
import { ArrowUpRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { BlogBreadcrumb, BlogDirectory, BlogFilters, BlogResults, type BlogDesignProps } from "@/components/blog/blog-design-shared"
import { ControlPanel } from "@/components/blog/control-panel"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"
import styles from "./blog-designs.module.css"

export function FolioDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  const { posts, title, path } = props
  return <div data-blog-layout="folio">
    <div className={styles.folioRunningHead}><span>{t.folioRunningTitle}</span><span>{t.folioPage} {String(query.page).padStart(2, "0")}</span></div>
    <header className={styles.folioMasthead}>
      <h1 className={styles.folioTitle}>{query.categoryId ? title : t.fieldNotes}</h1>
      <p className={styles.folioDeck}>{t.folioDescription}<span>{t.postCount(posts.totalCount)} · {t.folioColophon}</span></p>
    </header>
    <div className={styles.folioNavigation}><BlogBreadcrumb path={path} /><BlogDirectory {...props} /></div>
    <ControlPanel />
    <BlogFilters title={title} />
    <BlogResults posts={posts}>
      <div className={cn(styles.folioSpread, query.view === "list" && styles.folioList)}>
        {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className={styles.folioArticle}>
          <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`}>
            <div className={styles.folioArticleHeading}><span>{post.categoryName || t.posts}</span><b aria-hidden="true">{String((query.page - 1) * query.pageSize + index + 1).padStart(2, "0")}</b></div>
            <h2 className={styles.articleTitle}>{post.title}</h2>
            {post.summary ? <p className={cn(styles.articleSummary, "line-clamp-5")}>{post.summary}</p> : null}
            <div className={styles.folioArticleFooter}><time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time><span>{t.readArticle}<ArrowUpRight className="size-3.5" aria-hidden="true" /></span></div>
          </Link>
        </article>)}
      </div>
    </BlogResults>
  </div>
}
