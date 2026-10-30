"use client"

import { BlogStyleLink as Link } from "@/components/blog/blog-layout-context"
import { ArrowUpRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { useBlogNavigation } from "@/components/blog/blog-navigation"
import { BlogBreadcrumb, BlogDirectory, BlogFilters, BlogResults, type BlogDesignProps } from "@/components/blog/blog-design-shared"
import { ControlPanel } from "@/components/blog/control-panel"
import { formatDate } from "@/lib/format"
import { postHref } from "@/lib/routes"
import { cn } from "@/lib/utils"
import styles from "./blog-designs.module.css"

export function IndexDesign(props: BlogDesignProps) {
  const { t } = useI18n()
  const { query } = useBlogNavigation()
  const { posts, title, path } = props
  return <div data-blog-layout="index" className={cn(query.view === "list" && styles.indexCompact)}>
    <header className={styles.indexMasthead}>
      <div>
        <p className={styles.indexEyebrow}>{t.indexEyebrow}</p>
        <h1 className={styles.indexTitle}>{query.categoryId ? title : t.typeIndex}</h1>
        <p className={styles.indexNote}>{t.indexDescription}</p>
      </div>
      <div className={styles.indexCount}><strong>{posts.totalCount}</strong><span>{t.indexCountLabel}</span></div>
    </header>
    <div className={styles.indexNavigation}>
      <BlogDirectory {...props} />
      <BlogBreadcrumb path={path} />
    </div>
    <ControlPanel />
    <BlogFilters title={title} />
    <BlogResults posts={posts}>
      <div className={styles.indexTableHeader} aria-hidden="true"><span data-mobile-label={t.indexRecordShort}>{t.indexRecord}</span><span>{t.title}</span><span>{t.articleAbstract}</span><span>↗</span></div>
      {posts.items.map((post, index) => <article key={post.id} data-blog-article={post.id} className={styles.indexArticle}>
        <Link href={postHref(post)} aria-label={`${t.readArticle}: ${post.title}`}>
          <div className={styles.indexMeta}>
            <b>{String((query.page - 1) * query.pageSize + index + 1).padStart(3, "0")}</b>
            <span>{post.categoryName || t.posts}</span>
            <time dateTime={post.lastUpdateTime}>{formatDate(post.lastUpdateTime)}</time>
          </div>
          <h2 className={styles.articleTitle}>{post.title}</h2>
          {post.summary ? <p className={cn(styles.articleSummary, "line-clamp-4")}>{post.summary}</p> : null}
          <ArrowUpRight className={styles.indexArrow} aria-hidden="true" />
        </Link>
      </article>)}
    </BlogResults>
  </div>
}
