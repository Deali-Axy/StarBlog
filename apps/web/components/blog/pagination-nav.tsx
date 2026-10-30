"use client"

import { ChevronLeft, ChevronRight } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { BlogLink, useBlogNavigation } from "@/components/blog/blog-navigation"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

function pageNumbers(current: number, total: number) {
  if (total <= 5) return Array.from({ length: total }, (_, index) => index + 1)
  const numbers = [...new Set([1, Math.max(2, current - 1), current, Math.min(total - 1, current + 1), total])]
    .filter((page) => page >= 1 && page <= total).sort((a, b) => a - b)
  const pages: Array<number | string> = []
  numbers.forEach((page, index) => {
    if (index && page - numbers[index - 1] > 1) pages.push(`gap-${page}`)
    pages.push(page)
  })
  return pages
}

export function PaginationNav({ totalPages, totalCount, itemCount }: { totalPages: number; totalCount: number; itemCount: number }) {
  const { t } = useI18n()
  const { query, variant, href } = useBlogNavigation()
  const { page, pageSize } = query
  const start = itemCount ? (page - 1) * pageSize + 1 : 0
  if (totalPages < 1 || page > totalPages) return null
  return (
    <nav aria-label={t.articlePagination} className={cn("mt-8 flex flex-wrap items-center justify-between gap-5 border-t border-border pt-6", variant === "studio" && "font-mono")}>
      <div className="space-y-2">
        <p className="text-xs text-muted-foreground">{t.resultRange(start, start + Math.max(0, itemCount - 1), totalCount)}</p>
        <div className="flex items-center gap-1 text-xs text-muted-foreground">
          <span className="mr-1">{t.perPage}</span>
          {[6, 12, 24].map((size) => <BlogLink key={size} href={href({ pageSize: size })} aria-label={`${t.perPage} ${size}`}
            aria-current={pageSize === size ? "true" : undefined}
            className={cn("flex h-8 min-w-8 items-center justify-center rounded-md px-1.5", pageSize === size ? "bg-accent font-medium text-primary" : "hover:bg-muted")}>{size}</BlogLink>)}
        </div>
      </div>
      <div className="flex max-w-full flex-wrap items-center gap-1">
        {page > 1 ? <Button variant="outline" size="icon" asChild><BlogLink href={href({ page: page - 1 })} aria-label={t.previousPage}><ChevronLeft /></BlogLink></Button>
          : <Button variant="outline" size="icon" disabled aria-label={t.previousPage}><ChevronLeft /></Button>}
        {pageNumbers(page, totalPages).map((number) => typeof number === "string" ? <span key={number} className="px-1 text-muted-foreground" aria-hidden="true">…</span>
          : <BlogLink key={number} href={href({ page: number })} aria-label={t.pageOf(number, totalPages)} aria-current={page === number ? "page" : undefined}
            className={cn("flex size-9 items-center justify-center rounded-lg text-sm tabular-nums", page === number ? "bg-primary font-medium text-primary-foreground" : "text-muted-foreground hover:bg-muted")}>{number}</BlogLink>)}
        {page < totalPages ? <Button variant="outline" size="icon" asChild><BlogLink href={href({ page: page + 1 })} aria-label={t.nextPage}><ChevronRight /></BlogLink></Button>
          : <Button variant="outline" size="icon" disabled aria-label={t.nextPage}><ChevronRight /></Button>}
      </div>
    </nav>
  )
}
