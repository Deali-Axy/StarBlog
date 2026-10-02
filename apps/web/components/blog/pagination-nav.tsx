import Link from "next/link"
import { ChevronLeft, ChevronRight, Settings2 } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { routes } from "@/lib/routes"

interface PaginationNavProps {
  currentPage: number
  totalPages: number
  searchQuery?: string
  categoryId?: number
  sortBy?: string
  sortType?: "asc" | "desc"
  pageSize?: number
  viewMode?: "grid" | "list"
}

function pageNumbers(current: number, total: number) {
  const pages: Array<number | "ellipsis"> = []
  if (total <= 7) {
    for (let page = 1; page <= total; page += 1) pages.push(page)
    return pages
  }
  pages.push(1)
  if (current > 4) pages.push("ellipsis")
  const start = Math.max(2, current - 1)
  const end = Math.min(total - 1, current + 1)
  for (let page = start; page <= end; page += 1) pages.push(page)
  if (current < total - 3) pages.push("ellipsis")
  pages.push(total)
  return pages
}

export function PaginationNav({
  currentPage,
  totalPages,
  searchQuery,
  categoryId,
  sortBy,
  sortType,
  pageSize = 12,
  viewMode = "grid",
}: PaginationNavProps) {
  const shared = {
    search: searchQuery,
    categoryId,
    sortBy,
    sortType,
    pageSize,
    view: viewMode,
  }

  if (totalPages <= 0) return null

  return (
    <Card className="border-0 bg-white/80 py-0 shadow-sm backdrop-blur-sm dark:bg-slate-900/80">
      <CardContent className="flex flex-wrap items-center justify-between gap-4 p-4">
        <div className="flex items-center gap-3 text-sm text-muted-foreground">
          <span>
            第 {currentPage} 页，共 {totalPages} 页
          </span>
          <Settings2 className="size-3" />
          <span className="text-xs">每页</span>
          {[6, 12, 24].map((size) => (
            <Button
              key={size}
              variant={pageSize === size ? "default" : "outline"}
              size="sm"
              className="h-6 w-8 p-0 text-xs"
              asChild
            >
              <Link href={routes.blogList({ ...shared, page: 1, pageSize: size })}>{size}</Link>
            </Button>
          ))}
        </div>
        <div className="flex items-center gap-1">
          <Button variant="outline" size="icon-sm" disabled={currentPage <= 1} asChild={currentPage > 1}>
            {currentPage > 1 ? (
              <Link href={routes.blogList({ ...shared, page: currentPage - 1 })}>
                <ChevronLeft />
              </Link>
            ) : (
              <span>
                <ChevronLeft />
              </span>
            )}
          </Button>
          {pageNumbers(currentPage, totalPages).map((page, index) =>
            page === "ellipsis" ? (
              <span key={`e-${index}`} className="px-2 text-muted-foreground">
                ...
              </span>
            ) : (
              <Button
                key={page}
                variant={currentPage === page ? "default" : "outline"}
                size="sm"
                className="h-8 w-8 p-0"
                asChild
              >
                <Link href={routes.blogList({ ...shared, page })}>{page}</Link>
              </Button>
            ),
          )}
          <Button
            variant="outline"
            size="icon-sm"
            disabled={currentPage >= totalPages}
            asChild={currentPage < totalPages}
          >
            {currentPage < totalPages ? (
              <Link href={routes.blogList({ ...shared, page: currentPage + 1 })}>
                <ChevronRight />
              </Link>
            ) : (
              <span>
                <ChevronRight />
              </span>
            )}
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
