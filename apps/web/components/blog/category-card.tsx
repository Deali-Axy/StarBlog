import Link from "next/link"
import { Folder } from "lucide-react"

import { routes } from "@/lib/routes"
import { categoryColor, cn } from "@/lib/utils"

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
    <div className="group flex cursor-pointer items-center gap-4 rounded-xl border border-border/70 bg-card p-5 shadow-sm transition-all hover:shadow-lg">
      <div
        className={cn(
          "flex size-12 items-center justify-center rounded-lg text-white transition-transform group-hover:scale-110",
          categoryColor(name),
        )}
      >
        <Folder className="size-6" />
      </div>
      <div>
        <h3 className="font-semibold group-hover:text-blue-600 dark:group-hover:text-blue-400">{name}</h3>
        {count !== undefined ? (
          <p className="text-sm text-muted-foreground">{count} 篇文章</p>
        ) : null}
      </div>
    </div>
  )

  if (!categoryId) return content
  return <Link href={routes.blogList({ categoryId })}>{content}</Link>
}
