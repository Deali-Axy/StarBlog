"use client"
import { useI18n } from "@/components/app-providers"
import type { CategoryWordCloudItem } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import Link from "next/link"
export function TagInterests({ items }: {
  items: CategoryWordCloudItem[]
}) {
  const { t } = useI18n()
  if(items.length === 0)
    return null
  return (<section data-home-section="interests" data-home-variant={"tags"} className={"flex flex-wrap items-baseline gap-x-8 gap-y-6 border-y border-border py-5 border-dashed mt-[calc(4rem*var(--home-density))] first:mt-[calc(2.5rem*var(--home-density))] sm:mt-[calc(5.5rem*var(--home-density))]"} aria-labelledby="home-interests-title">
    <h2 style={{ zoom: "var(--home-scale)" }} id="home-interests-title" className="shrink-0 text-xs font-normal text-muted-foreground">{t.homeInterests}</h2>
    <div className="flex min-w-0 flex-1 flex-wrap items-baseline gap-x-5 gap-y-3">
      {items.map((item) => {
        return (<Link key={item.id} className={"inline-flex min-h-11 items-center text-muted-foreground hover:text-primary rounded-full border border-border bg-card px-4"} href={routes.blogList({ categoryId: item.id })} style={{ fontSize: `${0.8125}rem` }}>
          {item.name}
          <small className="ml-1 font-mono text-[11px]">{item.count}</small>
        </Link>)
      })}
    </div>
  </section>)
}
