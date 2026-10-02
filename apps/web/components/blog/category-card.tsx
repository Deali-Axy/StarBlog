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
  const content = (
    <div className="group border-t border-border/70 py-4">
      <h3 className="font-heading text-lg group-hover:text-primary">{name}</h3>
      {count !== undefined ? <p className="mt-1 text-sm text-muted-foreground">{count} 篇</p> : null}
    </div>
  )

  if (!categoryId) return content
  return <Link href={routes.blogList({ categoryId })}>{content}</Link>
}
