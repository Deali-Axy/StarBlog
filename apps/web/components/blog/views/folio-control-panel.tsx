"use client"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { BlogQuery } from "@/lib/blog-query"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { AlignLeft, ArrowDown, ArrowRight, ArrowUp, List, Search } from "lucide-react"
import { useBlogInteractions } from "../blog-interactions"
export function FolioControlPanel() {
  const { searchDraft, setSearchDraft } = useBlogInteractions()
  const { t } = useI18n()
  const { query, href, navigate } = useBlogNavigation()
  return <div className={"flex items-center gap-5 py-4 [&&]:[border-bottom:1px_solid_var(--foreground)] max-[39.9375rem]:flex-wrap max-[39.9375rem]:gap-1 max-[39.9375rem]:py-2.5"}>
    <form action={routes.blog} method="get" className={"flex items-center gap-2 min-w-0 [flex:1] max-[39.9375rem]:[flex-basis:100%] max-[39.9375rem]:[border-bottom:1px_solid_var(--border)]"} onSubmit={(event) => {
      event.preventDefault()
      navigate({ search: String(new FormData(event.currentTarget).get("search") ?? "").trim() || undefined })
    }}>
      {Object.entries(query).filter(([key, value]) => key !== "search" && key !== "page" && value !== undefined).map(([key, value]) => <input key={key} type="hidden" name={key} value={value} />)}
      <div className={"flex items-center gap-2.5 [flex:1] min-w-0 [&_input]:min-w-0 [&_input]:w-full [&_input]:h-10 [&_input]:border-0 [&_input]:rounded-none [&_input]:[background:transparent] [&_input]:text-[.8125rem] [&_input]:[outline-offset:2px] [&_input::placeholder]:text-muted-foreground"}>
        <Search className="size-4 shrink-0 text-muted-foreground" aria-hidden="true" />
        <input type="search" name="search" value={searchDraft} onChange={event => setSearchDraft(event.target.value)} aria-label={t.searchPosts} placeholder={t.searchTitle} />
      </div>
      <button type="submit" aria-label={t.search} title={t.search} className={"flex items-center justify-center w-10 h-10 shrink-0 cursor-pointer"}><ArrowRight className="size-5" /></button>
    </form>
    <div className={"flex items-center gap-2 max-[39.9375rem]:w-full max-[39.9375rem]:justify-end max-[39.9375rem]:[&_>_:first-child]:mr-auto"}>
      <Select value={query.sortBy} onValueChange={(sortBy) => navigate({ sortBy: sortBy as BlogQuery["sortBy"] })}>
        <SelectTrigger aria-label={t.sort} className={"min-w-29 h-9 border-0 rounded-none [background:transparent] [box-shadow:none] text-[.6875rem]"}><SelectValue /></SelectTrigger>
        <SelectContent position="popper" className={cn(blogThemeClassName("editorial"), "bg-card")}>
          <SelectItem value="lastUpdateTime">{t.updated}</SelectItem>
          <SelectItem value="creationTime">{t.created}</SelectItem>
          <SelectItem value="title">{t.title}</SelectItem>
        </SelectContent>
      </Select>
      <BlogLink href={href({ sortType: query.sortType === "desc" ? "asc" : "desc" })} aria-label={query.sortType === "desc" ? t.ascending : t.descending} title={query.sortType === "desc" ? t.ascending : t.descending} className={"flex w-9 h-9 items-center justify-center"}>
        {query.sortType === "desc" ? <ArrowDown className="size-4" /> : <ArrowUp className="size-4" />}
      </BlogLink>
      <div role="group" aria-label={t.articleView} className={"flex gap-0.5 [border-left:1px_solid_var(--border)] pl-2 [&_a]:flex [&_a]:w-8.5 [&_a]:h-8.5 [&_a]:items-center [&_a]:justify-center [&_a]:text-muted-foreground [&_a[aria-current=true]]:bg-accent [&_a[aria-current=true]]:text-foreground"}>
        {[{ value: "grid", label: t.expandedView, Icon: AlignLeft }, { value: "list", label: t.compactView, Icon: List }].map(({ value, label, Icon }) => <BlogLink key={value} href={href({ view: value as BlogQuery["view"] })} aria-label={label} aria-current={query.view === value ? "true" : undefined} title={value === "grid" ? t.expandedView : t.compactView}><Icon className="size-4" /></BlogLink>)}
      </div>
    </div>
  </div>
}
