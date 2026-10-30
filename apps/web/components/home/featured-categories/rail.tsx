"use client"

import { useEffect, useRef, useState } from "react"
import Link from "next/link"
import { ArrowRight, ChevronLeft, ChevronRight } from "lucide-react"
import { animate, motion, useMotionValue, useReducedMotion } from "motion/react"

import { useI18n } from "@/components/app-providers"
import { resolveCategoryIcon } from "@/components/home/featured-categories/icons"
import { Button } from "@/components/ui/button"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { eyebrowClassName, headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const CARD_WIDTH = 280 // w-70
const CARD_GAP = 20 // gap-5
const STEP = CARD_WIDTH + CARD_GAP

export function RailVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [index, setIndex] = useState(0)
  const [viewport, setViewport] = useState(0)
  const trackRef = useRef<HTMLDivElement>(null)
  const x = useMotionValue(0)

  useEffect(() => {
    const node = trackRef.current
    if (!node) return
    const update = () => setViewport(node.clientWidth)
    update()
    const observer = new ResizeObserver(update)
    observer.observe(node)
    return () => observer.disconnect()
  }, [])

  function offsetFor(i: number) {
    const center = viewport / 2 - CARD_WIDTH / 2
    return center - i * STEP
  }

  useEffect(() => {
    if (!viewport) return
    const target = offsetFor(index)
    if (reduceMotion) {
      x.set(target)
      return
    }
    void animate(x, target, { type: "spring", stiffness: 280, damping: 32 })
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only re-center when index/viewport change
  }, [index, viewport, reduceMotion])

  function go(delta: number) {
    setIndex((value) => Math.min(categories.length - 1, Math.max(0, value + delta)))
  }

  return (
    <div className="space-y-5">
      <div ref={trackRef} className="relative overflow-hidden py-4">
        <motion.div
          className="flex touch-pan-y gap-5"
          style={{ x, width: categories.length * STEP }}
          drag={reduceMotion ? false : "x"}
          dragConstraints={{
            left: offsetFor(categories.length - 1),
            right: offsetFor(0),
          }}
          dragElastic={0.12}
          onDragEnd={(_, info) => {
            const current = x.get()
            const projected = current + info.velocity.x * 0.18
            const center = viewport / 2 - CARD_WIDTH / 2
            const raw = (center - projected) / STEP
            const next = Math.min(categories.length - 1, Math.max(0, Math.round(raw)))
            setIndex(next)
          }}
        >
          {categories.map((category, i) => {
            const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
            const active = i === index
            return (
              <motion.div
                key={category.id}
                className="w-70 shrink-0"
                animate={
                  reduceMotion
                    ? { scale: 1, opacity: 1 }
                    : { scale: active ? 1.04 : 0.92, opacity: active ? 1 : 0.62 }
                }
                transition={{ type: "spring", stiffness: 320, damping: 28 }}
              >
                <Link
                  href={routes.blogList({ categoryId: category.categoryId })}
                  className={cn(
                    "group flex h-[19rem] flex-col justify-between border bg-card/50 p-5 transition-colors",
                    active ? "border-primary/50" : "border-border/70 hover:border-primary/40",
                  )}
                  onFocus={() => setIndex(i)}
                >
                  <div className="flex items-start justify-between gap-3">
                    <p className={eyebrowClassName}>{String(i + 1).padStart(2, "0")}</p>
                    <Icon className="size-5 text-primary/80" aria-hidden />
                  </div>
                  <div>
                    <h3 className={cn(headingClassName, "font-heading text-2xl leading-snug group-hover:text-primary")}>{category.name}</h3>
                    {category.description ? (
                      <p className="mt-3 line-clamp-4 text-sm leading-relaxed text-muted-foreground">
                        {category.description}
                      </p>
                    ) : null}
                    <span className="mt-4 inline-flex items-center gap-1 text-xs text-muted-foreground group-hover:text-primary">
                      {t.enterPath}
                      <ArrowRight className="size-3.5" />
                    </span>
                  </div>
                </Link>
              </motion.div>
            )
          })}
        </motion.div>
      </div>

      <div className="flex items-center justify-between gap-4">
        <p className="text-xs text-muted-foreground">{t.railHint}</p>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label={t.flipPrev}
            onClick={() => go(-1)}
            disabled={index <= 0}
          >
            <ChevronLeft />
          </Button>
          <div className="min-w-16 text-center font-heading text-sm tabular-nums">
            {t.folioOf(index + 1, categories.length)}
          </div>
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label={t.flipNext}
            onClick={() => go(1)}
            disabled={index >= categories.length - 1}
          >
            <ChevronRight />
          </Button>
        </div>
      </div>
    </div>
  )
}
