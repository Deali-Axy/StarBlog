"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import { routes } from "@/lib/routes"

export function CategoryCard({
  name,
  count,
  categoryId,
}: {
  name: string
  count?: number
  categoryId?: number
}) {
  const { t } = useI18n()
  const content = (
    <div className="group border-t border-border/70 py-4">
      <h3 className="font-heading text-lg group-hover:text-primary">{name}</h3>
      {count !== undefined ? <p className="mt-1 text-sm text-muted-foreground">{t.postCount(count)}</p> : null}
    </div>
  )

  if (!categoryId) return content
  return <Link href={routes.blogList({ categoryId })}>{content}</Link>
}
