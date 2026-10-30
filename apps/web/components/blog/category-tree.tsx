"use client"

import { ChevronRight, Library, Search, X } from "lucide-react"
import { Collapsible } from "radix-ui"
import type { CategoryNodeResponse } from "@/lib/api/types"
import { filterCategories } from "@/lib/blog-categories"
import { cn } from "@/lib/utils"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"

export interface CategoryTreeState {
  expanded: Set<number>
  search: string
  setSearch: (search: string) => void
  toggle: (id: number) => void
}

interface CategoryTreeProps {
  categoryNodes: CategoryNodeResponse[]
  state: CategoryTreeState
  onSelect?: () => void
  className?: string
}

export function CategoryTree({ categoryNodes, state, onSelect, className }: CategoryTreeProps) {
  const { t } = useI18n()
  const { query, variant, href } = useBlogNavigation()
  const filtered = filterCategories(categoryNodes, state.search)
  const searching = Boolean(state.search.trim())

  function nodeView(node: CategoryNodeResponse, depth = 0) {
    const hasChildren = Boolean(node.nodes?.length)
    const open = searching || state.expanded.has(node.id)
    const selected = query.categoryId === node.id
    return (
      <Collapsible.Root key={node.id} open={open} onOpenChange={() => state.toggle(node.id)} data-category-id={node.id}>
        <div className={cn(
          "group my-0.5 flex min-h-10 items-center gap-0.5 rounded-lg pr-2 text-sm transition-colors",
          variant === "editorial" && "rounded-none",
          selected ? "bg-accent font-medium text-primary" : "text-muted-foreground hover:bg-muted hover:text-foreground",
        )} style={{ paddingLeft: `${8 + depth * 12}px` }}>
          {hasChildren ? (
            <Collapsible.Trigger
              className="flex size-8 shrink-0 items-center justify-center rounded-md hover:bg-foreground/5"
              aria-label={`${open ? t.collapseCategory : t.expandCategory}: ${node.text}`}
              disabled={searching}
            >
              <ChevronRight className={cn("size-3.5 transition-transform motion-reduce:transition-none", open && "rotate-90")} />
            </Collapsible.Trigger>
          ) : <span className="flex size-8 shrink-0 items-center justify-center"><span className="size-1 rounded-full bg-current opacity-40" /></span>}
          <BlogLink href={href({ categoryId: node.id })} onClick={onSelect} aria-current={selected ? "page" : undefined}
            className="min-w-0 flex-1 py-2" title={node.text}>
            <span className="block truncate">{node.text}</span>
          </BlogLink>
          {node.tags?.[0] != null ? <span className="shrink-0 text-[11px] tabular-nums opacity-70">{node.tags[0]}</span> : null}
        </div>
        {hasChildren ? <Collapsible.Content>{node.nodes!.map((child) => nodeView(child, depth + 1))}</Collapsible.Content> : null}
      </Collapsible.Root>
    )
  }

  return (
    <nav className={cn("flex min-h-0 flex-col", className)} aria-label={t.categoryTree}>
      <div className="shrink-0 px-3 pt-4 pb-3">
        <p className={cn("mb-3 px-1 text-xs font-medium tracking-wider text-muted-foreground", variant === "studio" && "font-mono uppercase")}>
          {t.browseCategories}
        </p>
        <div className="relative">
          <Search className="pointer-events-none absolute top-3 left-3 size-3.5 text-muted-foreground" />
          <input type="search" value={state.search} onChange={(event) => state.setSearch(event.target.value)}
            placeholder={t.searchCategories} aria-label={t.searchCategories}
            className="h-10 w-full rounded-lg border border-border bg-background pr-9 pl-8 text-xs placeholder:text-muted-foreground [&::-webkit-search-cancel-button]:appearance-none" />
          {state.search ? <button type="button" onClick={() => state.setSearch("")} aria-label={t.clearCategorySearch}
            className="absolute top-1 right-1 flex size-8 items-center justify-center rounded-md text-muted-foreground hover:text-foreground"><X className="size-3.5" /></button> : null}
        </div>
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain px-2 pb-3 [scrollbar-width:thin]">
        <BlogLink data-category-all href={href({ categoryId: undefined })} onClick={onSelect} aria-current={!query.categoryId ? "page" : undefined}
          className={cn("mb-3 flex min-h-11 items-center gap-2.5 rounded-lg px-3 text-sm font-medium", variant === "editorial" && "rounded-none border-b border-border",
            !query.categoryId ? "bg-accent text-primary" : "text-muted-foreground hover:bg-muted hover:text-foreground")}>
          <Library className="size-4" />{t.allPosts}
        </BlogLink>
        <div data-category-roots>{filtered.length ? filtered.map((node) => nodeView(node)) : <p className="px-3 py-8 text-sm text-muted-foreground">{searching ? t.noMatchingCategories : t.noCategories}</p>}</div>
      </div>
    </nav>
  )
}
