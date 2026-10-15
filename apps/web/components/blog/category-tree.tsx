"use client"

import { useI18n } from "@/components/app-providers"

import { useState } from "react"
import Link from "next/link"
import { ChevronDown, ChevronRight } from "lucide-react"

import type { CategoryNodeResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

interface CategoryTreeProps {
  categoryNodes: CategoryNodeResponse[]
  currentCategoryId?: number
  searchQuery?: string
  className?: string
}

function CategoryTreeNode({
  node,
  currentCategoryId,
  searchQuery,
  depth = 0,
}: {
  node: CategoryNodeResponse
  currentCategoryId?: number
  searchQuery?: string
  depth?: number
}) {
  const { t } = useI18n()
  const [isExpanded, setIsExpanded] = useState(true)
  const hasChildren = Boolean(node.nodes?.length)
  const isSelected = currentCategoryId === node.id
  const count = node.tags?.[0]

  return (
    <div className="select-none">
      <div
        className="flex items-center gap-1 py-1.5"
        style={{ paddingLeft: `${depth * 12}px` }}
      >
        {hasChildren ? (
          <button
            type="button"
            onClick={() => setIsExpanded((value) => !value)}
            className="flex size-6 items-center justify-center text-muted-foreground"
            aria-label={isExpanded ? t.collapseCategory : t.expandCategory}
          >
            {isExpanded ? <ChevronDown className="size-3" /> : <ChevronRight className="size-3" />}
          </button>
        ) : (
          <span className="w-6" />
        )}
        <Link
          href={routes.blogList({ categoryId: node.id, search: searchQuery })}
          className={cn(
            "min-w-0 flex-1 truncate text-sm",
            isSelected ? "text-primary" : "text-muted-foreground hover:text-foreground",
          )}
        >
          {node.text}
        </Link>
        {count ? <span className="text-xs text-muted-foreground">{count}</span> : null}
      </div>
      {hasChildren && isExpanded
        ? node.nodes!.map((child) => (
            <CategoryTreeNode
              key={child.id}
              node={child}
              currentCategoryId={currentCategoryId}
              searchQuery={searchQuery}
              depth={depth + 1}
            />
          ))
        : null}
    </div>
  )
}

export function CategoryTree({
  categoryNodes,
  currentCategoryId,
  searchQuery,
  className,
}: CategoryTreeProps) {
  const { t } = useI18n()
  return (
    <nav className={cn(className)} aria-label={t.categoryTree}>
      <p className="kicker mb-4">{t.categories}</p>
      <Link
        href={routes.blogList({ search: searchQuery })}
        className={cn(
          "mb-2 block text-sm",
          !currentCategoryId ? "text-primary" : "text-muted-foreground hover:text-foreground",
        )}
      >
        {t.all}
      </Link>
      {categoryNodes.length > 0 ? (
        categoryNodes.map((node) => (
          <CategoryTreeNode
            key={node.id}
            node={node}
            currentCategoryId={currentCategoryId}
            searchQuery={searchQuery}
          />
        ))
      ) : (
        <p className="py-8 text-sm text-muted-foreground">{t.noCategories}</p>
      )}
    </nav>
  )
}
