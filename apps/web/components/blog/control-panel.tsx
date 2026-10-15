"use client"

import { useI18n } from "@/components/app-providers"

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
  const { t } = useI18n()
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
        <span className="hidden text-muted-foreground sm:inline">{t.sort}</span>
        {[
          { value: "lastUpdateTime", label: t.updated },
          { value: "creationTime", label: t.created },
          { value: "title", label: t.title },
        ].map((option) => (
          <Button
            key={option.value}
            variant="ghost"
            size="sm"
            className={`h-7 px-2 text-xs ${currentSortBy === option.value ? "bg-accent text-primary" : ""}`}
            asChild
          >
            <Link href={routes.blogList({ ...shared, sortBy: option.value })}>{option.label}</Link>
          </Button>
        ))}
        <Button
          variant="ghost"
          size="icon-sm"
          className={currentSortType === "desc" ? "bg-accent text-primary" : ""}
          asChild
        >
          <Link href={routes.blogList({ ...shared, sortType: "desc" })} title={t.descending}>
            <ChevronDown />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="icon-sm"
          className={currentSortType === "asc" ? "bg-accent text-primary" : ""}
          asChild
        >
          <Link href={routes.blogList({ ...shared, sortType: "asc" })} title={t.ascending}>
            <ChevronUp />
          </Link>
        </Button>
      </div>
      <div className="mx-1 h-4 w-px bg-border" />
      <Button variant="ghost" size="sm" className="h-7 px-2 text-xs" asChild>
        <Link href={routes.blog}>
          <RotateCcw className="size-3" />
          {t.clear}
        </Link>
      </Button>
      <div className="mx-1 h-4 w-px bg-border" />
      <Button
        variant="ghost"
        size="icon-sm"
        className={currentView === "grid" ? "bg-accent text-primary" : ""}
        asChild
      >
        <Link href={routes.blogList({ ...shared, view: "grid" })} title={t.gridView}>
          <Grid3X3 />
        </Link>
      </Button>
      <Button
        variant="ghost"
        size="icon-sm"
        className={currentView === "list" ? "bg-accent text-primary" : ""}
        asChild
      >
        <Link href={routes.blogList({ ...shared, view: "list" })} title={t.listView}>
          <List />
        </Link>
      </Button>
    </div>
  )
}
