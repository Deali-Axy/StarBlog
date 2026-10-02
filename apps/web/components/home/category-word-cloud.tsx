import Link from "next/link"

import type { CategoryWordCloudItem } from "@/lib/api/types"
import { routes } from "@/lib/routes"

export function CategoryWordCloud({ items }: { items: CategoryWordCloudItem[] }) {
  if (items.length === 0) return null
  const max = Math.max(...items.map((item) => item.count), 1)

  return (
    <section className="mt-16">
      <h2 className="mb-4 text-2xl font-bold">分类词云</h2>
      <div className="flex flex-wrap items-center gap-x-4 gap-y-3 rounded-2xl border border-border/70 bg-card p-6">
        {items.map((item) => {
          const weight = item.count / max
          return (
            <Link
              key={item.id}
              href={routes.blogList({ categoryId: item.id })}
              className="text-muted-foreground transition-colors hover:text-blue-600"
              style={{ fontSize: `${0.85 + weight * 1.35}rem`, opacity: 0.65 + weight * 0.35 }}
            >
              {item.name}
              <span className="ml-1 text-xs opacity-70">{item.count}</span>
            </Link>
          )
        })}
      </div>
    </section>
  )
}
