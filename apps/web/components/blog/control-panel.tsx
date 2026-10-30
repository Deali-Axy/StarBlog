"use client"

import { ArrowDown, ArrowUp, LayoutGrid, List, Search } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { BlogQuery } from "@/lib/blog-query"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

export function ControlPanel() {
  const { t } = useI18n()
  const { query, variant, href, navigate } = useBlogNavigation()
  return (
    <div className={cn("flex flex-col gap-3 p-3 sm:flex-row sm:flex-wrap sm:items-center",
      variant === "library" && "rounded-xl border border-border bg-card shadow-sm shadow-foreground/2",
      variant === "editorial" && "border-y border-foreground/20 px-0 py-4",
      variant === "studio" && "rounded-lg border border-border bg-card/80 ring-1 ring-card")}>
      <form action={routes.blog} method="get" className="flex min-w-0 flex-1 items-center gap-2 sm:basis-52" onSubmit={(event) => {
        event.preventDefault()
        navigate({ search: String(new FormData(event.currentTarget).get("search") ?? "").trim() || undefined })
      }}>
        {Object.entries(query).filter(([key, value]) => key !== "search" && key !== "page" && value !== undefined).map(([key, value]) => <input key={key} type="hidden" name={key} value={value} />)}
        <div className="relative min-w-0 flex-1">
          <Search className="pointer-events-none absolute top-3 left-3 size-4 text-muted-foreground" />
          <input key={query.search ?? ""} type="search" name="search" defaultValue={query.search} aria-label={t.searchPosts} placeholder={t.searchTitle}
            className="h-10 w-full min-w-0 rounded-lg border border-transparent bg-muted/60 pr-3 pl-9 text-sm placeholder:text-muted-foreground focus:border-ring focus:bg-background" />
        </div>
        <Button type="submit" size="sm" className="h-10 px-3">{t.search}</Button>
      </form>
      <div className="flex items-center gap-2">
        <Select value={query.sortBy} onValueChange={(sortBy) => navigate({ sortBy: sortBy as BlogQuery["sortBy"] })}>
          <SelectTrigger aria-label={t.sort} className="h-10 min-w-0 flex-1 border-border bg-background sm:w-32 sm:flex-none"><SelectValue /></SelectTrigger>
          <SelectContent position="popper" className={cn(blogThemeClassName(variant), "bg-card")}>
            <SelectItem value="lastUpdateTime">{t.updated}</SelectItem>
            <SelectItem value="creationTime">{t.created}</SelectItem>
            <SelectItem value="title">{t.title}</SelectItem>
          </SelectContent>
        </Select>
        <Button variant="outline" size="icon" className="size-10" asChild>
          <BlogLink href={href({ sortType: query.sortType === "desc" ? "asc" : "desc" })} aria-label={query.sortType === "desc" ? t.ascending : t.descending} title={query.sortType === "desc" ? t.ascending : t.descending}>
            {query.sortType === "desc" ? <ArrowDown className="size-4" /> : <ArrowUp className="size-4" />}
          </BlogLink>
        </Button>
        <div role="group" aria-label={t.articleView} className="ml-1 flex rounded-lg border border-border bg-background p-1">
          {[{ value: "grid", label: t.gridView, Icon: LayoutGrid }, { value: "list", label: t.listView, Icon: List }].map(({ value, label, Icon }) => (
            <BlogLink key={value} href={href({ view: value as BlogQuery["view"] })} aria-label={label} aria-current={query.view === value ? "true" : undefined} title={label}
              className={cn("flex size-8 items-center justify-center rounded-md transition-colors", query.view === value ? "bg-accent text-primary" : "text-muted-foreground hover:text-foreground")}><Icon className="size-4" /></BlogLink>
          ))}
        </div>
      </div>
    </div>
  )
}
