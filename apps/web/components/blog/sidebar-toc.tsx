"use client"

import { useI18n } from "@/components/app-providers"

import { useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { BookOpen, ChevronRight } from "lucide-react"

import type { TocNode } from "@/lib/api/types"
import { cn } from "@/lib/utils"

export interface FlatTocItem {
  text: string
  href: string
  depth: number
}

export function flattenToc(nodes: TocNode[] | null | undefined, depth = 2): FlatTocItem[] {
  if (!nodes?.length) return []
  return nodes.flatMap((node) => {
    const current: FlatTocItem[] = node.href
      ? [{ text: node.text ?? "", href: node.href, depth }]
      : []
    return [...current, ...flattenToc(node.nodes, depth + 1)]
  })
}

export function SidebarTOC({
  items,
  className,
  title,
  maxDepth = 4,
}: {
  items: FlatTocItem[]
  className?: string
  title?: string
  maxDepth?: number
}) {
  const { t } = useI18n()
  const filtered = useMemo(
    () => items.filter((item) => item.depth <= maxDepth && item.href),
    [items, maxDepth],
  )
  const [activeId, setActiveId] = useState("")

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries.filter((entry) => entry.isIntersecting)
        if (visible.length === 0) return
        const top = visible.reduce((prev, current) =>
          prev.boundingClientRect.top < current.boundingClientRect.top ? prev : current,
        )
        setActiveId(top.target.id)
      },
      { rootMargin: "-20% 0% -35% 0%", threshold: [0, 0.25, 0.5, 1] },
    )

    filtered.forEach((item) => {
      const id = item.href.replace("#", "")
      const element = document.getElementById(id)
      if (element) observer.observe(element)
    })

    return () => observer.disconnect()
  }, [filtered])

  if (filtered.length === 0) return null

  return (
    <nav className={cn("space-y-1", className)}>
      <div className="mb-3 flex items-center gap-2">
        <BookOpen className="size-4 text-primary" />
        <h3 className="text-sm font-semibold">{title ?? t.toc}</h3>
      </div>
      <div className="space-y-1">
        {filtered.map((item, index) => {
          const id = item.href.replace("#", "")
          const isActive = activeId === id
          const indent = Math.max(0, item.depth - 2)
          return (
            <Link
              key={`${id}-${index}`}
              href={item.href}
              className={cn(
                "flex items-start gap-2 rounded-md border-l-2 border-transparent px-2 py-1.5 text-sm transition-colors hover:bg-accent/50",
                isActive && "border-l-primary bg-accent font-medium text-primary",
                !isActive && "text-muted-foreground hover:text-foreground",
              )}
              style={{ paddingLeft: `${8 + indent * 12}px` }}
            >
              {indent > 0 ? <ChevronRight className="mt-0.5 size-3 shrink-0" /> : null}
              <span className="leading-relaxed">{item.text}</span>
            </Link>
          )
        })}
      </div>
      <p className="mt-4 border-t border-border/50 pt-3 text-center text-xs text-muted-foreground">
        {t.sectionCount(filtered.length)}
      </p>
    </nav>
  )
}
