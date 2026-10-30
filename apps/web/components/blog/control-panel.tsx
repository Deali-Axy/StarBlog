"use client"

import { ArrowDown, ArrowRight, ArrowUp, AlignLeft, List, Search } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { BlogQuery } from "@/lib/blog-query"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import styles from "./blog-designs.module.css"

export function ControlPanel() {
  const { t } = useI18n()
  const { query, variant, href, navigate } = useBlogNavigation()
  return <div className={styles.tools}>
    <form action={routes.blog} method="get" className={styles.searchForm} onSubmit={(event) => {
      event.preventDefault()
      navigate({ search: String(new FormData(event.currentTarget).get("search") ?? "").trim() || undefined })
    }}>
      {Object.entries(query).filter(([key, value]) => key !== "search" && key !== "page" && value !== undefined).map(([key, value]) => <input key={key} type="hidden" name={key} value={value} />)}
      <div className={styles.searchInput}>
        <Search className="size-4 shrink-0 text-muted-foreground" aria-hidden="true" />
        <input key={query.search ?? ""} type="search" name="search" defaultValue={query.search} aria-label={t.searchPosts} placeholder={t.searchTitle} />
      </div>
      <button type="submit" aria-label={t.search} title={t.search} className={styles.searchButton}><ArrowRight className="size-5" /></button>
    </form>
    <div className={styles.toolSettings}>
      <Select value={query.sortBy} onValueChange={(sortBy) => navigate({ sortBy: sortBy as BlogQuery["sortBy"] })}>
        <SelectTrigger aria-label={t.sort} className={styles.sortTrigger}><SelectValue /></SelectTrigger>
        <SelectContent position="popper" className={cn(blogThemeClassName(variant), "bg-card")}>
          <SelectItem value="lastUpdateTime">{t.updated}</SelectItem>
          <SelectItem value="creationTime">{t.created}</SelectItem>
          <SelectItem value="title">{t.title}</SelectItem>
        </SelectContent>
      </Select>
      <BlogLink href={href({ sortType: query.sortType === "desc" ? "asc" : "desc" })} aria-label={query.sortType === "desc" ? t.ascending : t.descending}
        title={query.sortType === "desc" ? t.ascending : t.descending} className={styles.sortDirection}>
        {query.sortType === "desc" ? <ArrowDown className="size-4" /> : <ArrowUp className="size-4" />}
      </BlogLink>
      <div role="group" aria-label={t.articleView} className={styles.viewSwitch}>
        {[{ value: "grid", label: t.expandedView, Icon: AlignLeft }, { value: "list", label: t.compactView, Icon: List }].map(({ value, label, Icon }) =>
          <BlogLink key={value} href={href({ view: value as BlogQuery["view"] })} aria-label={label} aria-current={query.view === value ? "true" : undefined}
            title={value === "grid" ? t.expandedView : t.compactView}><Icon className="size-4" /></BlogLink>)}
      </div>
    </div>
  </div>
}
