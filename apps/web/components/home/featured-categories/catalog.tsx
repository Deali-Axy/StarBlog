"use client"

import { useState } from "react"
import Link from "next/link"
import { ArrowRight } from "lucide-react"
import { AnimatePresence, motion, useReducedMotion } from "motion/react"

import { useI18n } from "@/components/app-providers"
import { resolveCategoryIcon } from "@/components/home/featured-categories/icons"
import { Button } from "@/components/ui/button"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { eyebrowClassName, headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function CatalogVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [activeId, setActiveId] = useState(categories[0]?.id ?? 0)
  const active = categories.find((item) => item.id === activeId) ?? categories[0]
  if (!active) return null
  const ActiveIcon = resolveCategoryIcon(active.iconCssClass, active.name)

  return (
    <div className="grid gap-6 lg:grid-cols-[minmax(0,0.95fr)_minmax(0,1.15fr)] lg:gap-10">
      <ul className="space-y-1 border-y border-border/70">
        {categories.map((category, index) => {
          const selected = category.id === activeId
          return (
            <li key={category.id} className="border-b border-border/50 last:border-b-0">
              <button
                type="button"
                className={cn(
                  "group flex w-full items-baseline justify-between gap-4 px-1 py-4 text-left transition-colors",
                  selected ? "text-foreground" : "text-muted-foreground hover:text-foreground",
                )}
                onMouseEnter={() => setActiveId(category.id)}
                onFocus={() => setActiveId(category.id)}
                onClick={() => setActiveId(category.id)}
                aria-pressed={selected}
              >
                <span className="flex min-w-0 items-baseline gap-3">
                  <span className="font-heading text-xs tracking-[0.14em] text-muted-foreground/80">
                    {String(index + 1).padStart(2, "0")}
                  </span>
                  <span className={cn("font-heading text-xl md:text-2xl", selected && "text-primary")}>
                    {category.name}
                  </span>
                </span>
                <motion.span
                  className="h-px origin-left flex-1 bg-border/80"
                  initial={false}
                  animate={{ opacity: selected ? 1 : 0.35, scaleX: selected ? 1 : 0.7 }}
                />
              </button>
            </li>
          )
        })}
      </ul>

      <div className="relative min-h-[18rem] overflow-hidden border border-border/70 bg-card/40 p-6 md:min-h-[22rem] md:p-8">
        <p className="mb-4 text-xs text-muted-foreground lg:hidden">{t.catalogHint}</p>
        <AnimatePresence mode="wait">
          <motion.div
            key={active.id}
            className="flex h-full flex-col justify-between"
            initial={reduceMotion ? false : { opacity: 0, y: 14, filter: "blur(4px)" }}
            animate={{ opacity: 1, y: 0, filter: "blur(0px)" }}
            exit={reduceMotion ? undefined : { opacity: 0, y: -10, filter: "blur(4px)" }}
            transition={{ duration: 0.28, ease: "easeOut" }}
          >
            <div>
              <div className="flex items-start justify-between gap-4">
                <p className={eyebrowClassName}>{t.paths}</p>
                <span className="flex size-12 items-center justify-center rounded-full border border-primary/30 bg-primary/10 text-primary">
                  <ActiveIcon className="size-5" aria-hidden />
                </span>
              </div>
              <h3 className={cn(headingClassName, "mt-6 font-heading text-3xl md:text-4xl")}>{active.name}</h3>
              {active.description ? (
                <p className="mt-4 max-w-xl text-sm leading-relaxed text-muted-foreground md:text-[0.95rem]">
                  {active.description}
                </p>
              ) : null}
            </div>
            <Button className="mt-8 w-fit" asChild>
              <Link href={routes.blogList({ categoryId: active.categoryId })}>
                {t.enterPath}
                <ArrowRight />
              </Link>
            </Button>
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  )
}
