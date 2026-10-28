"use client"

import { useEffect, useMemo, useState } from "react"
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
  Star,
  Terminal,
  type LucideIcon,
} from "lucide-react"
import {
  AnimatePresence,
  animate,
  motion,
  useMotionValue,
  useReducedMotion,
  useTransform,
  type PanInfo,
} from "motion/react"

import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import {
  FEATURED_CATEGORIES_PREVIEW,
  FEATURED_CATEGORIES_VARIANT,
  FEATURED_CATEGORIES_VARIANT_OPTIONS,
  type FeaturedCategoriesVariant,
} from "@/lib/featured-categories-variant"
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

function magazineSpanClass(index: number, count: number) {
  if (count === 1) return "md:col-span-6"
  if (count === 2) return index === 0 ? "md:col-span-4 md:row-span-2" : "md:col-span-2 md:row-span-2"
  if (index === 0) return "md:col-span-4 md:row-span-2"
  return "md:col-span-2"
}

function SectionHeader({
  variant,
  onVariantChange,
  preview,
}: {
  variant: FeaturedCategoriesVariant
  onVariantChange: (next: FeaturedCategoriesVariant) => void
  preview: boolean
}) {
  const { t, locale } = useI18n()

  return (
    <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <p className="kicker">{t.paths}</p>
        <h2 className="mt-2 text-2xl">{t.featuredCategories}</h2>
      </div>
      {preview ? (
        <div
          className="flex flex-wrap gap-1 rounded-md border border-border/80 bg-card/60 p-1"
          role="tablist"
          aria-label={locale === "zh" ? "精选分类布局预览" : "Featured categories layout preview"}
        >
          {FEATURED_CATEGORIES_VARIANT_OPTIONS.map((option) => {
            const active = option.id === variant
            return (
              <button
                key={option.id}
                type="button"
                role="tab"
                aria-selected={active}
                onClick={() => onVariantChange(option.id)}
                className={cn(
                  "rounded px-2.5 py-1.5 text-xs transition-colors",
                  active
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:text-foreground",
                )}
              >
                {locale === "zh" ? option.labelZh : option.labelEn}
              </button>
            )
          })}
        </div>
      ) : (
        <Button variant="ghost" size="sm" asChild>
          <Link href={routes.blog}>{t.viewAll}</Link>
        </Button>
      )}
    </div>
  )
}

function MagazineVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [hoveredId, setHoveredId] = useState<number | null>(null)

  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-6 md:auto-rows-[minmax(9.5rem,auto)]">
      {categories.map((category, index) => {
        const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
        const featured = index === 0
        const dimmed = hoveredId !== null && hoveredId !== category.id
        const delay = reduceMotion ? 0 : index * 0.07

        return (
          <motion.div
            key={category.id}
            className={cn("min-h-0", magazineSpanClass(index, categories.length))}
            initial={reduceMotion ? false : { opacity: 0, y: 24 }}
            animate={{
              opacity: dimmed ? 0.5 : 1,
              y: 0,
              scale: dimmed ? 0.985 : 1,
            }}
            whileHover={reduceMotion ? undefined : { y: -5, scale: 1.02, zIndex: 5 }}
            whileTap={reduceMotion ? undefined : { scale: 0.985 }}
            onHoverStart={() => setHoveredId(category.id)}
            onHoverEnd={() => setHoveredId(null)}
            transition={{
              opacity: { duration: 0.25, delay: hoveredId === null ? delay : 0 },
              y: { type: "spring", stiffness: 320, damping: 28, delay },
              scale: { type: "spring", stiffness: 380, damping: 30 },
            }}
          >
            <Link
              href={routes.blogList({ categoryId: category.categoryId })}
              className="group relative flex h-full min-h-[9.5rem] flex-col justify-between overflow-hidden border border-border/70 bg-card/40 p-5 transition-colors hover:border-primary/50 hover:bg-card/80 md:p-6"
            >
              <div
                className="pointer-events-none absolute -right-6 -top-6 size-28 rounded-full bg-[radial-gradient(circle,oklch(0.82_0.10_78/0.18),transparent_70%)] opacity-70 transition-opacity group-hover:opacity-100 dark:bg-[radial-gradient(circle,oklch(0.82_0.1_78/0.12),transparent_70%)]"
                aria-hidden
              />
              <div className="relative flex items-start justify-between gap-3">
                <p className="kicker">{featured ? t.featured : t.paths}</p>
                <motion.span
                  className="inline-flex"
                  animate={
                    reduceMotion
                      ? undefined
                      : hoveredId === category.id
                        ? { rotateY: 180 }
                        : { rotateY: 0 }
                  }
                  transition={{ type: "spring", stiffness: 220, damping: 18 }}
                  style={{ transformStyle: "preserve-3d" }}
                >
                  <Icon
                    className={cn(
                      "shrink-0 text-primary/70 transition-colors group-hover:text-primary",
                      featured ? "size-6" : "size-5",
                    )}
                    aria-hidden
                  />
                </motion.span>
              </div>
              <div className="relative mt-4">
                <h3
                  className={cn(
                    "font-heading leading-snug transition-colors group-hover:text-primary",
                    featured ? "text-2xl md:text-3xl" : "text-lg md:text-xl",
                  )}
                >
                  {category.name}
                </h3>
                {category.description ? (
                  <p
                    className={cn(
                      "mt-2 text-sm leading-relaxed text-muted-foreground",
                      featured ? "line-clamp-4 max-w-xl" : "line-clamp-3",
                    )}
                  >
                    {category.description}
                  </p>
                ) : null}
                <span className="mt-4 inline-flex items-center gap-1 text-xs text-muted-foreground transition-colors group-hover:text-primary">
                  {t.enterPath}
                  <ArrowRight className="size-3.5 transition-transform group-hover:translate-x-0.5" />
                </span>
              </div>
            </Link>
          </motion.div>
        )
      })}
    </div>
  )
}

/** Deterministic constellation anchors (percent of chart box). */
const STAR_LAYOUTS: Record<number, { x: number; y: number }[]> = {
  1: [{ x: 50, y: 48 }],
  2: [
    { x: 28, y: 42 },
    { x: 72, y: 42 },
  ],
  3: [
    { x: 18, y: 38 },
    { x: 52, y: 22 },
    { x: 78, y: 58 },
  ],
  4: [
    { x: 18, y: 28 },
    { x: 48, y: 18 },
    { x: 78, y: 32 },
    { x: 42, y: 68 },
  ],
  5: [
    { x: 16, y: 30 },
    { x: 42, y: 16 },
    { x: 72, y: 24 },
    { x: 58, y: 58 },
    { x: 28, y: 68 },
  ],
  6: [
    { x: 14, y: 28 },
    { x: 40, y: 14 },
    { x: 68, y: 22 },
    { x: 82, y: 48 },
    { x: 52, y: 68 },
    { x: 24, y: 62 },
  ],
  7: [
    { x: 14, y: 30 },
    { x: 36, y: 14 },
    { x: 60, y: 16 },
    { x: 84, y: 32 },
    { x: 74, y: 60 },
    { x: 44, y: 74 },
    { x: 18, y: 58 },
  ],
  8: [
    { x: 12, y: 34 },
    { x: 30, y: 14 },
    { x: 54, y: 12 },
    { x: 78, y: 22 },
    { x: 88, y: 48 },
    { x: 70, y: 72 },
    { x: 40, y: 76 },
    { x: 16, y: 60 },
  ],
}

const STAR_TOUR_MS = 4500

function starPositions(count: number) {
  if (STAR_LAYOUTS[count]) return STAR_LAYOUTS[count]!
  return Array.from({ length: count }, (_, i) => {
    const angle = (Math.PI * 2 * i) / count - Math.PI / 2
    return {
      x: 50 + Math.cos(angle) * 34,
      y: 48 + Math.sin(angle) * 30,
    }
  })
}

function StarVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const [activeId, setActiveId] = useState<number | null>(categories[0]?.id ?? null)
  const [paused, setPaused] = useState(false)
  const positions = useMemo(() => starPositions(categories.length), [categories.length])
  const active = categories.find((c) => c.id === activeId) ?? categories[0]

  const edges = useMemo(() => {
    if (categories.length < 2) return [] as [number, number][]
    // Ring edges keep denser maps readable.
    return categories.map((_, i) => [i, (i + 1) % categories.length] as [number, number])
  }, [categories])

  useEffect(() => {
    if (reduceMotion || paused || categories.length < 2) return
    const timer = window.setInterval(() => {
      setActiveId((current) => {
        const idx = Math.max(
          0,
          categories.findIndex((item) => item.id === current),
        )
        return categories[(idx + 1) % categories.length]!.id
      })
    }, STAR_TOUR_MS)
    return () => window.clearInterval(timer)
  }, [reduceMotion, paused, categories])

  return (
    <div className="space-y-6">
      <ul className="space-y-3 md:hidden">
        {categories.map((category, index) => {
          const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
          return (
            <motion.li
              key={category.id}
              initial={reduceMotion ? false : { opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: reduceMotion ? 0 : index * 0.05, type: "spring", stiffness: 320, damping: 28 }}
            >
              <Link
                href={routes.blogList({ categoryId: category.categoryId })}
                className="group flex gap-3 border-t border-border/70 pt-4"
              >
                <span className="mt-1 flex size-8 shrink-0 items-center justify-center rounded-full border border-primary/40 text-primary">
                  <Star className="size-3.5 fill-current" aria-hidden />
                </span>
                <div>
                  <div className="flex items-center gap-2">
                    <Icon className="size-4 text-primary/80" aria-hidden />
                    <h3 className="font-heading text-lg group-hover:text-primary">{category.name}</h3>
                  </div>
                  {category.description ? (
                    <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{category.description}</p>
                  ) : null}
                </div>
              </Link>
            </motion.li>
          )
        })}
      </ul>

      <div className="hidden md:grid md:grid-cols-[minmax(0,1.4fr)_minmax(16rem,0.9fr)] md:items-stretch md:gap-8">
        <div
          className="relative min-h-[24rem] overflow-hidden border border-border/70 bg-[radial-gradient(ellipse_at_50%_40%,oklch(0.82_0.10_78/0.10),transparent_55%)] dark:bg-[radial-gradient(ellipse_at_50%_40%,oklch(0.82_0.1_78/0.08),transparent_55%)]"
          onMouseEnter={() => setPaused(true)}
          onMouseLeave={() => setPaused(false)}
        >
          <svg className="absolute inset-0 size-full" aria-hidden>
            {edges.map(([a, b], edgeIndex) => {
              const from = positions[a]
              const to = positions[b]
              if (!from || !to) return null
              const lit = categories[a]?.id === activeId || categories[b]?.id === activeId
              return (
                <motion.line
                  key={`${a}-${b}`}
                  x1={`${from.x}%`}
                  y1={`${from.y}%`}
                  x2={`${to.x}%`}
                  y2={`${to.y}%`}
                  className={cn(lit ? "stroke-primary/70" : "stroke-border/70")}
                  strokeWidth={lit ? 1.75 : 1}
                  strokeDasharray={lit ? undefined : "4 6"}
                  initial={reduceMotion ? false : { pathLength: 0, opacity: 0 }}
                  animate={{ pathLength: 1, opacity: 1 }}
                  transition={{ duration: 0.7, delay: reduceMotion ? 0 : 0.08 * edgeIndex, ease: "easeOut" }}
                />
              )
            })}
          </svg>

          {categories.map((category, index) => {
            const pos = positions[index]
            if (!pos) return null
            const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
            const isActive = category.id === activeId
            return (
              <motion.button
                key={category.id}
                type="button"
                className="absolute -translate-x-1/2 -translate-y-1/2 text-left"
                initial={
                  reduceMotion
                    ? { left: `${pos.x}%`, top: `${pos.y}%`, opacity: 1, scale: 1 }
                    : { opacity: 0, scale: 0.4, left: "50%", top: "48%" }
                }
                animate={{
                  opacity: isActive ? 1 : 0.72,
                  scale: isActive ? 1.08 : 1,
                  left: `${pos.x}%`,
                  top: `${pos.y}%`,
                  zIndex: isActive ? 10 : 1,
                }}
                transition={{
                  type: "spring",
                  stiffness: 280,
                  damping: 22,
                  delay: reduceMotion ? 0 : 0.05 * index,
                }}
                onMouseEnter={() => {
                  setPaused(true)
                  setActiveId(category.id)
                }}
                onFocus={() => {
                  setPaused(true)
                  setActiveId(category.id)
                }}
                aria-pressed={isActive}
              >
                <span
                  className={cn(
                    "flex items-center gap-2 rounded-full border bg-background/90 px-3 py-1.5 shadow-sm backdrop-blur-sm transition-colors",
                    isActive
                      ? "border-primary text-foreground shadow-[0_0_0_4px_color-mix(in_oklab,var(--primary)_18%,transparent)]"
                      : "border-border/80 text-muted-foreground hover:border-primary/50 hover:text-foreground",
                  )}
                >
                  <motion.span
                    className={cn(
                      "flex size-6 items-center justify-center rounded-full",
                      isActive ? "bg-primary/15 text-primary" : "bg-muted text-muted-foreground",
                    )}
                    animate={!reduceMotion && isActive ? { scale: [1, 1.12, 1] } : { scale: 1 }}
                    transition={
                      !reduceMotion && isActive
                        ? { repeat: Infinity, duration: 2.4, ease: "easeInOut" }
                        : { duration: 0.2 }
                    }
                  >
                    <Icon className="size-3.5" aria-hidden />
                  </motion.span>
                  <span className="font-heading whitespace-nowrap text-sm">{category.name}</span>
                </span>
              </motion.button>
            )
          })}
        </div>

        <aside className="relative overflow-hidden border border-border/70 bg-card/40 p-6">
          <AnimatePresence mode="wait">
            {active ? (
              <motion.div
                key={active.id}
                className="flex h-full flex-col justify-between"
                initial={reduceMotion ? false : { opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                exit={reduceMotion ? undefined : { opacity: 0, y: -10 }}
                transition={{ duration: 0.28, ease: "easeOut" }}
              >
                <div>
                  <p className="kicker">{t.observations}</p>
                  <h3 className="mt-3 font-heading text-2xl">{active.name}</h3>
                  {active.description ? (
                    <p className="mt-3 text-sm leading-relaxed text-muted-foreground">{active.description}</p>
                  ) : null}
                </div>
                <Button className="mt-8 w-fit" asChild>
                  <Link href={routes.blogList({ categoryId: active.categoryId })}>
                    {t.enterPath}
                    <ArrowRight />
                  </Link>
                </Button>
              </motion.div>
            ) : null}
          </AnimatePresence>
        </aside>
      </div>
    </div>
  )
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
    // Forward pages exit to the left (negative x), like a turned leaf.
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

    // Swipe left → next; swipe right → previous.
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

export function FeaturedCategories({
  categories,
  initialVariant = FEATURED_CATEGORIES_VARIANT,
  preview = FEATURED_CATEGORIES_PREVIEW,
}: {
  categories: FeaturedCategoryResponse[]
  initialVariant?: FeaturedCategoriesVariant
  preview?: boolean
}) {
  const [variant, setVariant] = useState<FeaturedCategoriesVariant>(initialVariant)

  function handleVariantChange(next: FeaturedCategoriesVariant) {
    setVariant(next)
    if (!preview || typeof window === "undefined") return
    const url = new URL(window.location.href)
    url.searchParams.set("fc", next)
    window.history.replaceState(null, "", `${url.pathname}${url.search}${url.hash}`)
  }

  if (categories.length === 0) return null

  return (
    <section className="mt-20">
      <SectionHeader variant={variant} onVariantChange={handleVariantChange} preview={preview} />
      {variant === "deck" ? <DeckVariant categories={categories} /> : null}
      {variant === "magazine" ? <MagazineVariant categories={categories} /> : null}
      {variant === "star" ? <StarVariant categories={categories} /> : null}
    </section>
  )
}
