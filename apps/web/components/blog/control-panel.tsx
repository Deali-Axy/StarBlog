import Link from "next/link"
import { ArrowUpDown, ChevronDown, ChevronUp, Grid3X3, List, RotateCcw } from "lucide-react"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

interface ControlPanelProps {
  currentSortBy: string
  currentView: "grid" | "list"
  currentSortType: "asc" | "desc"
  searchQuery?: string
  categoryId?: number
}

export function ControlPanel({
  currentSortBy,
  currentView,
  currentSortType,
  searchQuery,
  categoryId,
}: ControlPanelProps) {
  const shared = {
    search: searchQuery,
    categoryId,
    sortBy: currentSortBy,
    sortType: currentSortType,
    view: currentView,
  }

  return (
    <div className="flex flex-wrap items-center gap-2 py-1">
      <div className="flex items-center gap-1 text-xs">
        <ArrowUpDown className="size-4 text-muted-foreground" />
        <span className="hidden text-muted-foreground sm:inline">排序</span>
        {[
          { value: "lastUpdateTime", label: "更新时间" },
          { value: "creationTime", label: "创建时间" },
          { value: "title", label: "标题" },
        ].map((option) => (
          <Button
            key={option.value}
            variant="ghost"
            size="sm"
            className={`h-7 px-2 text-xs ${currentSortBy === option.value ? "bg-blue-50 text-blue-600 dark:bg-blue-950/20 dark:text-blue-400" : ""}`}
            asChild
          >
            <Link href={routes.blogList({ ...shared, sortBy: option.value })}>{option.label}</Link>
          </Button>
        ))}
        <Button
          variant="ghost"
          size="icon-sm"
          className={currentSortType === "desc" ? "bg-blue-50 text-blue-600" : ""}
          asChild
        >
          <Link href={routes.blogList({ ...shared, sortType: "desc" })} title="降序">
            <ChevronDown />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="icon-sm"
          className={currentSortType === "asc" ? "bg-blue-50 text-blue-600" : ""}
          asChild
        >
          <Link href={routes.blogList({ ...shared, sortType: "asc" })} title="升序">
            <ChevronUp />
          </Link>
        </Button>
      </div>
      <div className="mx-1 h-4 w-px bg-border" />
      <Button variant="ghost" size="sm" className="h-7 px-2 text-xs" asChild>
        <Link href={routes.blog}>
          <RotateCcw className="size-3" />
          清除
        </Link>
      </Button>
      <div className="mx-1 h-4 w-px bg-border" />
      <Button
        variant="ghost"
        size="icon-sm"
        className={currentView === "grid" ? "bg-blue-50 text-blue-600" : ""}
        asChild
      >
        <Link href={routes.blogList({ ...shared, view: "grid" })} title="网格视图">
          <Grid3X3 />
        </Link>
      </Button>
      <Button
        variant="ghost"
        size="icon-sm"
        className={currentView === "list" ? "bg-blue-50 text-blue-600" : ""}
        asChild
      >
        <Link href={routes.blogList({ ...shared, view: "list" })} title="列表视图">
          <List />
        </Link>
      </Button>
    </div>
  )
}
