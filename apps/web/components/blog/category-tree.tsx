"use client"

import { useState } from "react"
import Link from "next/link"
import { ChevronDown, ChevronRight, Folder, FolderOpen, Tag } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
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
  const [isExpanded, setIsExpanded] = useState(true)
  const hasChildren = Boolean(node.nodes?.length)
  const isSelected = currentCategoryId === node.id
  const count = node.tags?.[0]

  return (
    <div className="select-none">
      <div
        className={cn(
          "flex items-center gap-2 rounded-lg px-3 py-2 transition-colors hover:bg-muted",
          isSelected && "border-l-4 border-blue-500 bg-blue-50 dark:bg-blue-900/20",
        )}
        style={{ paddingLeft: `${depth * 16 + 12}px` }}
      >
        {hasChildren ? (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsExpanded((value) => !value)}
            className="size-6 p-0"
          >
            {isExpanded ? <ChevronDown className="size-3" /> : <ChevronRight className="size-3" />}
          </Button>
        ) : (
          <div className="w-6" />
        )}
        {hasChildren ? (
          isExpanded ? <FolderOpen className="size-4 text-blue-500" /> : <Folder className="size-4 text-blue-500" />
        ) : (
          <Tag className="size-4 text-muted-foreground" />
        )}
        <Link
          href={routes.blogList({ categoryId: node.id, search: searchQuery })}
          className={cn(
            "min-w-0 flex-1 truncate text-sm font-medium",
            isSelected ? "text-blue-600 dark:text-blue-400" : "text-foreground/80 hover:text-blue-600",
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
  return (
    <Card className={cn("border-0 bg-white/80 py-4 shadow-sm backdrop-blur-sm dark:bg-slate-900/80", className)}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-lg font-semibold">
          <Folder className="size-5 text-blue-500" />
          分类目录
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div
          className={cn(
            "mb-2 flex items-center gap-2 rounded-lg px-3 py-2 hover:bg-muted",
            !currentCategoryId && "border-l-4 border-blue-500 bg-blue-50 dark:bg-blue-900/20",
          )}
        >
          <div className="w-6" />
          <Tag className="size-4 text-muted-foreground" />
          <Link
            href={routes.blogList({ search: searchQuery })}
            className={cn(
              "flex-1 text-sm font-medium",
              !currentCategoryId ? "text-blue-600 dark:text-blue-400" : "hover:text-blue-600",
            )}
          >
            全部分类
          </Link>
        </div>
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
          <p className="py-8 text-center text-sm text-muted-foreground">暂无分类</p>
        )}
      </CardContent>
    </Card>
  )
}
