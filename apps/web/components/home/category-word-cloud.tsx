"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import type { CategoryWordCloudItem } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { useHomeDesign } from "@/components/home/home-design-context"
import { cn } from "@/lib/utils"

export function CategoryWordCloud({ items }: { items: CategoryWordCloudItem[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (items.length === 0) return null
  const max = Math.max(...items.map((item) => item.count), 1)

  return (
    <section className={cn("mt-14 flex flex-wrap items-baseline gap-x-8 gap-y-6 border-y border-border py-5", variant === "desk" && "border-dashed")} aria-labelledby="home-interests-title">
      <h2 id="home-interests-title" className="shrink-0 text-xs font-normal text-muted-foreground">{t.homeInterests}</h2>
      <div className="flex min-w-0 flex-1 flex-wrap items-baseline gap-x-5 gap-y-3">
        {items.map((item) => {
          const weight = item.count / max
          return (
            <Link
              key={item.id}
              className="text-muted-foreground hover:text-primary"
              href={routes.blogList({ categoryId: item.id })}
              style={{ fontSize: `${0.8 + weight * 0.45}rem` }}
            >
              {item.name}
              <small className="ml-1 font-mono text-[11px]">{item.count}</small>
            </Link>
          )
        })}
      </div>
    </section>
  )
}
