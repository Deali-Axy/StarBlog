"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import type { CategoryWordCloudItem } from "@/lib/api/types"
import { routes } from "@/lib/routes"

export function CategoryWordCloud({ items }: { items: CategoryWordCloudItem[] }) {
  const { t } = useI18n()
  if (items.length === 0) return null
  const max = Math.max(...items.map((item) => item.count), 1)

  return (
    <section className="mt-20">
      <p className="kicker">{t.index}</p>
      <h2 className="mt-2 mb-6 text-2xl">{t.categories}</h2>
      <div className="flex flex-wrap items-baseline gap-x-5 gap-y-3">
        {items.map((item) => {
          const weight = item.count / max
          return (
            <Link
              key={item.id}
              href={routes.blogList({ categoryId: item.id })}
              className="text-muted-foreground transition-colors hover:text-primary"
              style={{ fontSize: `${0.85 + weight * 1.15}rem` }}
            >
              {item.name}
              <span className="ml-1 text-xs opacity-60">{item.count}</span>
            </Link>
          )
        })}
      </div>
    </section>
  )
}
