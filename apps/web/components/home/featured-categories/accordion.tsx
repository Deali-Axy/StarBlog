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
import { cn } from "@/lib/utils"

export function AccordionVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [openId, setOpenId] = useState(categories[0]?.id ?? 0)

  return (
    <div className="space-y-3">
      <p className="text-xs text-muted-foreground md:hidden">{t.accordionHint}</p>

      {/* Desktop: horizontal accordion */}
      <div className="hidden md:flex md:h-[22rem] md:gap-2">
        {categories.map((category, index) => {
          const open = category.id === openId
          const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
          return (
            <motion.div
              key={category.id}
              className="relative overflow-hidden border border-border/70 bg-card/40"
              initial={false}
              animate={{ flexGrow: open ? 3.2 : 0.55, flexBasis: 0 }}
              transition={
                reduceMotion
                  ? { duration: 0 }
                  : { type: "spring", stiffness: 260, damping: 30 }
              }
              onMouseEnter={() => setOpenId(category.id)}
              onFocus={() => setOpenId(category.id)}
            >
              <button
                type="button"
                className="absolute inset-0 z-10 cursor-pointer"
                aria-expanded={open}
                aria-label={category.name}
                onClick={() => setOpenId(category.id)}
              />
              <div className={cn("relative flex h-full flex-col p-5", open ? "justify-between" : "justify-end")}>
                <div className={cn("flex items-start justify-between gap-2", !open && "flex-col-reverse items-start gap-4")}>
                  <p className="kicker">{String(index + 1).padStart(2, "0")}</p>
                  <Icon className={cn("text-primary/80", open ? "size-5" : "size-4")} aria-hidden />
                </div>

                <div className="relative z-20 pointer-events-none">
                  <h3
                    className={cn(
                      "font-heading leading-snug",
                      open ? "text-2xl md:text-3xl" : "text-sm writing-mode-vertical",
                    )}
                    style={!open ? { writingMode: "vertical-rl", transform: "rotate(180deg)" } : undefined}
                  >
                    {category.name}
                  </h3>
                  <AnimatePresence initial={false}>
                    {open ? (
                      <motion.div
                        key="body"
                        className="pointer-events-auto"
                        initial={reduceMotion ? false : { opacity: 0, y: 8 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={reduceMotion ? undefined : { opacity: 0, y: 6 }}
                        transition={{ duration: 0.22 }}
                      >
                        {category.description ? (
                          <p className="mt-3 max-w-md text-sm leading-relaxed text-muted-foreground">
                            {category.description}
                          </p>
                        ) : null}
                        <Button className="mt-5" asChild>
                          <Link href={routes.blogList({ categoryId: category.categoryId })}>
                            {t.enterPath}
                            <ArrowRight />
                          </Link>
                        </Button>
                      </motion.div>
                    ) : null}
                  </AnimatePresence>
                </div>
              </div>
            </motion.div>
          )
        })}
      </div>

      {/* Mobile: vertical accordion */}
      <ul className="space-y-2 md:hidden">
        {categories.map((category, index) => {
          const open = category.id === openId
          const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
          return (
            <li key={category.id} className="overflow-hidden border border-border/70 bg-card/40">
              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 px-4 py-3 text-left"
                aria-expanded={open}
                onClick={() => setOpenId(category.id)}
              >
                <span className="flex items-center gap-3">
                  <span className="font-heading text-xs tracking-[0.14em] text-muted-foreground">
                    {String(index + 1).padStart(2, "0")}
                  </span>
                  <span className="font-heading text-lg">{category.name}</span>
                </span>
                <Icon className="size-4 text-primary/80" aria-hidden />
              </button>
              <AnimatePresence initial={false}>
                {open ? (
                  <motion.div
                    initial={reduceMotion ? false : { height: 0, opacity: 0 }}
                    animate={{ height: "auto", opacity: 1 }}
                    exit={reduceMotion ? undefined : { height: 0, opacity: 0 }}
                    transition={{ duration: 0.25 }}
                    className="overflow-hidden"
                  >
                    <div className="space-y-4 px-4 pb-4">
                      {category.description ? (
                        <p className="text-sm leading-relaxed text-muted-foreground">{category.description}</p>
                      ) : null}
                      <Button size="sm" asChild>
                        <Link href={routes.blogList({ categoryId: category.categoryId })}>
                          {t.enterPath}
                          <ArrowRight />
                        </Link>
                      </Button>
                    </div>
                  </motion.div>
                ) : null}
              </AnimatePresence>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
