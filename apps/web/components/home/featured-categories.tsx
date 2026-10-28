"use client"

import { useState } from "react"
import Link from "next/link"
import {
  ArrowRight,
  BookOpen,
  Braces,
  ChevronLeft,
  ChevronRight,
  Compass,
  Cpu,
  HardDrive,
  Monitor,
  Server,
  Smartphone,
  Terminal,
  type LucideIcon,
} from "lucide-react"
import { animate, motion, useMotionValue, useReducedMotion, useTransform, type PanInfo } from "motion/react"

import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

function resolveCategoryIcon(iconCssClass: string, name: string): LucideIcon {
  const key = `${iconCssClass} ${name}`.toLowerCase()
  if (/(python|django|flask)/.test(key)) return Terminal
  if (/(microsoft|asp|dotnet|\.net|csharp|c#)/.test(key)) return Monitor
  if (/(android|flutter|mobile|kotlin|ios)/.test(key)) return Smartphone
  if (/(react|vue|js|javascript|typescript|node|前端)/.test(key)) return Braces
  if (/(hard-drive|backup|保全|zfs|数据)/.test(key)) return HardDrive
  if (/(starblog|blog|笔记|book)/.test(key)) return BookOpen
  if (/(server|devops|docker|linux|shell)/.test(key)) return Server
  if (/(ml|ai|llm|cpu|算法)/.test(key)) return Cpu
  return Compass
}

const DECK_SWIPE_OFFSET = 140
const DECK_SWIPE_VELOCITY = 500

function DeckCardFace({
  category,
  index,
  total,
}: {
  category: FeaturedCategoryResponse
  index: number
  total: number
}) {
  const { t } = useI18n()
  const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
  const n = String(index + 1).padStart(2, "0")

  return (
    <div className="relative flex h-full flex-col justify-between overflow-hidden border border-border/80 bg-card p-6 shadow-[0_18px_40px_-28px_rgba(40,30,10,0.45)] md:p-8">
      <div
        className="pointer-events-none absolute inset-y-0 left-0 w-2 bg-gradient-to-r from-foreground/8 to-transparent"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute -right-10 -top-12 size-40 rounded-full bg-[radial-gradient(circle,oklch(0.82_0.10_78/0.22),transparent_68%)] dark:bg-[radial-gradient(circle,oklch(0.82_0.1_78/0.14),transparent_68%)]"
        aria-hidden
      />

      <div className="relative flex items-start justify-between gap-3">
        <div>
          <p className="kicker">{t.paths}</p>
          <p className="mt-2 font-heading text-xs tracking-[0.18em] text-muted-foreground">
            {t.folioOf(index + 1, total)}
          </p>
        </div>
        <span className="flex size-11 items-center justify-center rounded-full border border-primary/30 bg-primary/10 text-primary">
          <Icon className="size-5" aria-hidden />
        </span>
      </div>

      <div className="relative mt-8">
        <p className="font-heading text-sm text-muted-foreground/80">{n}</p>
        <h3 className="mt-1 font-heading text-3xl leading-tight md:text-4xl">{category.name}</h3>
        {category.description ? (
          <p className="mt-4 line-clamp-4 max-w-lg text-sm leading-relaxed text-muted-foreground">
            {category.description}
          </p>
        ) : null}
      </div>

      <div className="relative mt-8">
        <Button asChild>
          <Link href={routes.blogList({ categoryId: category.categoryId })} onPointerDown={(e) => e.stopPropagation()}>
            {t.enterPath}
            <ArrowRight />
          </Link>
        </Button>
      </div>
    </div>
  )
}

function DeckVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [index, setIndex] = useState(0)
  const [busy, setBusy] = useState(false)
  const x = useMotionValue(0)
  const rotate = useTransform(x, [-220, 0, 220], [-14, 0, 14])
  const opacity = useTransform(x, [-220, -80, 0, 80, 220], [0.35, 1, 1, 1, 0.35])

  const total = categories.length
  const current = categories[index]
  if (!current || total === 0) return null

  function wrap(next: number) {
    return ((next % total) + total) % total
  }

  async function flipTo(nextIndex: number, exitDirection: 1 | -1) {
    const target = wrap(nextIndex)
    if (target === index || busy) return
    setBusy(true)
    try {
      if (!reduceMotion) {
        const distance = typeof window !== "undefined" ? Math.min(window.innerWidth * 0.55, 520) : 420
        await animate(x, exitDirection * distance, { duration: 0.22, ease: "easeIn" })
      }
      setIndex(target)
      x.set(0)
    } finally {
      setBusy(false)
    }
  }

  function go(delta: number) {
    void flipTo(index + delta, delta >= 0 ? -1 : 1)
  }

  async function onDragEnd(_: unknown, info: PanInfo) {
    if (busy) return
    const shouldFlip =
      Math.abs(info.offset.x) > DECK_SWIPE_OFFSET || Math.abs(info.velocity.x) > DECK_SWIPE_VELOCITY

    if (!shouldFlip) {
      await animate(x, 0, { type: "spring", stiffness: 420, damping: 32 })
      return
    }

    if (info.offset.x < 0 || info.velocity.x < 0) await flipTo(index + 1, -1)
    else await flipTo(index - 1, 1)
  }

  const stackPreview = [0, 1, 2]
    .map((offset) => {
      const i = wrap(index + offset)
      return { offset, category: categories[i]!, absoluteIndex: i }
    })
    .reverse()

  return (
    <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-end">
      <div className="relative mx-auto w-full max-w-xl">
        <div className="relative h-[22rem] md:h-[24rem]" style={{ perspective: 1400 }}>
          {stackPreview.map(({ offset, category, absoluteIndex }) => {
            const isTop = offset === 0
            const depth = offset

            if (!isTop) {
              return (
                <motion.div
                  key={`stack-${category.id}-${absoluteIndex}`}
                  className="pointer-events-none absolute inset-0"
                  aria-hidden
                  initial={false}
                  animate={
                    reduceMotion
                      ? { scale: 1 - depth * 0.04, y: depth * 14, opacity: 1 - depth * 0.18 }
                      : {
                          scale: 1 - depth * 0.045,
                          y: depth * 16,
                          rotate: depth * -1.6,
                          opacity: 1 - depth * 0.16,
                        }
                  }
                  transition={{ type: "spring", stiffness: 260, damping: 28 }}
                  style={{ zIndex: 10 - depth, transformOrigin: "50% 100%" }}
                >
                  <DeckCardFace category={category} index={absoluteIndex} total={total} />
                </motion.div>
              )
            }

            return (
              <motion.div
                key={`top-${category.id}-${index}`}
                className="absolute inset-0 cursor-grab touch-pan-y active:cursor-grabbing"
                style={{ x, rotate, opacity, zIndex: 30, transformOrigin: "50% 100%" }}
                drag={reduceMotion || busy ? false : "x"}
                dragConstraints={{ left: 0, right: 0 }}
                dragElastic={0.82}
                onDragEnd={onDragEnd}
                initial={reduceMotion ? false : { opacity: 0.55, scale: 0.97, y: 14 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                transition={{ type: "spring", stiffness: 320, damping: 28 }}
              >
                <DeckCardFace category={category} index={index} total={total} />
              </motion.div>
            )
          })}
        </div>
      </div>

      <div className="flex flex-col items-center gap-4 lg:items-stretch lg:pb-2">
        <p className="text-center text-xs text-muted-foreground lg:text-left">{t.flipHint}</p>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label={t.flipPrev}
            onClick={() => go(-1)}
            disabled={total <= 1}
          >
            <ChevronLeft />
          </Button>
          <div className="min-w-16 text-center font-heading text-sm tabular-nums">{t.folioOf(index + 1, total)}</div>
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label={t.flipNext}
            onClick={() => go(1)}
            disabled={total <= 1}
          >
            <ChevronRight />
          </Button>
        </div>
        <div className="flex justify-center gap-1.5 lg:justify-start" aria-hidden>
          {categories.map((category, i) => (
            <button
              key={category.id}
              type="button"
              className={cn(
                "h-1.5 rounded-full transition-all",
                i === index ? "w-5 bg-primary" : "w-1.5 bg-border hover:bg-muted-foreground/50",
              )}
              onClick={() => flipTo(i, i > index ? -1 : 1)}
              aria-label={t.folioOf(i + 1, total)}
            />
          ))}
        </div>
      </div>
    </div>
  )
}

export function FeaturedCategories({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()

  if (categories.length === 0) return null

  return (
    <section className="mt-20">
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="kicker">{t.paths}</p>
          <h2 className="mt-2 text-2xl">{t.featuredCategories}</h2>
        </div>
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>{t.viewAll}</Link>
        </Button>
      </div>
      <DeckVariant categories={categories} />
    </section>
  )
}
