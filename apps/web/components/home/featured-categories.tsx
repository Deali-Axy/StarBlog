"use client"

import { useMemo, useState } from "react"
import Link from "next/link"
import {
  ArrowRight,
  Braces,
  Compass,
  Cpu,
  Monitor,
  Smartphone,
  Star,
  Terminal,
  type LucideIcon,
} from "lucide-react"

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
  if (/(ml|ai|llm|data|cpu|算法)/.test(key)) return Cpu
  if (/(linux|shell|devops|docker)/.test(key)) return Terminal
  return Compass
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

function PathVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()

  return (
    <ol className="relative space-y-0 border-l border-border/70 ml-3 md:ml-4">
      {categories.map((category, index) => {
        const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
        const n = String(index + 1).padStart(2, "0")
        return (
          <li key={category.id} className="relative pl-8 pb-10 last:pb-0 md:pl-10">
            <span
              className="absolute -left-[5px] top-1.5 size-2.5 rounded-full border-2 border-primary bg-background"
              aria-hidden
            />
            <Link
              href={routes.blogList({ categoryId: category.categoryId })}
              className="group block"
            >
              <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
                <span className="font-heading text-sm tracking-[0.14em] text-muted-foreground/80">
                  {n}
                </span>
                <Icon
                  className="size-4 text-primary/80 transition-colors group-hover:text-primary"
                  aria-hidden
                />
                <h3 className="font-heading text-xl transition-colors group-hover:text-primary md:text-2xl">
                  {category.name}
                </h3>
              </div>
              {category.description ? (
                <p className="mt-2 max-w-2xl text-sm leading-relaxed text-muted-foreground">
                  {category.description}
                </p>
              ) : null}
              <span className="mt-3 inline-flex items-center gap-1 text-xs text-muted-foreground transition-colors group-hover:text-primary">
                {t.enterPath}
                <ArrowRight className="size-3.5 transition-transform group-hover:translate-x-0.5" />
              </span>
            </Link>
          </li>
        )
      })}
    </ol>
  )
}

function MagazineVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()

  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-6 md:auto-rows-[minmax(9.5rem,auto)]">
      {categories.map((category, index) => {
        const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
        const featured = index === 0
        const spanClass =
          categories.length === 1
            ? "md:col-span-6"
            : categories.length === 2
              ? index === 0
                ? "md:col-span-4 md:row-span-2"
                : "md:col-span-2 md:row-span-2"
              : featured
                ? "md:col-span-4 md:row-span-2"
                : "md:col-span-2"

        return (
          <Link
            key={category.id}
            href={routes.blogList({ categoryId: category.categoryId })}
            className={cn(
              "group relative flex flex-col justify-between overflow-hidden border border-border/70 bg-card/40 p-5 transition-colors hover:border-primary/50 hover:bg-card/80 md:p-6",
              spanClass,
            )}
          >
            <div
              className="pointer-events-none absolute -right-6 -top-6 size-28 rounded-full bg-[radial-gradient(circle,oklch(0.82_0.10_78/0.18),transparent_70%)] opacity-70 transition-opacity group-hover:opacity-100 dark:bg-[radial-gradient(circle,oklch(0.82_0.1_78/0.12),transparent_70%)]"
              aria-hidden
            />
            <div className="relative flex items-start justify-between gap-3">
              <p className="kicker">{featured ? t.featured : t.paths}</p>
              <Icon
                className={cn(
                  "shrink-0 text-primary/70 transition-colors group-hover:text-primary",
                  featured ? "size-6" : "size-5",
                )}
                aria-hidden
              />
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
}

function starPositions(count: number) {
  if (STAR_LAYOUTS[count]) return STAR_LAYOUTS[count]!
  return Array.from({ length: count }, (_, i) => {
    const angle = (Math.PI * 2 * i) / count - Math.PI / 2
    return {
      x: 50 + Math.cos(angle) * 32,
      y: 48 + Math.sin(angle) * 28,
    }
  })
}

function StarVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const [activeId, setActiveId] = useState<number | null>(categories[0]?.id ?? null)
  const positions = useMemo(() => starPositions(categories.length), [categories.length])
  const active = categories.find((c) => c.id === activeId) ?? categories[0]

  const edges = useMemo(() => {
    if (categories.length < 2) return [] as [number, number][]
    return categories.slice(0, -1).map((_, i) => [i, i + 1] as [number, number])
  }, [categories])

  return (
    <div className="space-y-6">
      {/* Mobile / narrow: stacked path-like list */}
      <ul className="space-y-4 md:hidden">
        {categories.map((category) => {
          const Icon = resolveCategoryIcon(category.iconCssClass, category.name)
          return (
            <li key={category.id}>
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
                    <p className="mt-1 text-sm text-muted-foreground line-clamp-2">
                      {category.description}
                    </p>
                  ) : null}
                </div>
              </Link>
            </li>
          )
        })}
      </ul>

      {/* Desktop constellation */}
      <div className="hidden md:grid md:grid-cols-[minmax(0,1.4fr)_minmax(16rem,0.9fr)] md:gap-8 md:items-stretch">
        <div
          className="relative min-h-[22rem] overflow-hidden border border-border/70 bg-[radial-gradient(ellipse_at_50%_40%,oklch(0.82_0.10_78/0.10),transparent_55%)] dark:bg-[radial-gradient(ellipse_at_50%_40%,oklch(0.82_0.1_78/0.08),transparent_55%)]"
          onMouseLeave={() => setActiveId(categories[0]?.id ?? null)}
        >
          <svg className="absolute inset-0 size-full" aria-hidden>
            {edges.map(([a, b]) => {
              const from = positions[a]
              const to = positions[b]
              if (!from || !to) return null
              const lit =
                categories[a]?.id === activeId || categories[b]?.id === activeId
              return (
                <line
                  key={`${a}-${b}`}
                  x1={`${from.x}%`}
                  y1={`${from.y}%`}
                  x2={`${to.x}%`}
                  y2={`${to.y}%`}
                  className={cn(
                    "stroke-border transition-colors",
                    lit ? "stroke-primary/60" : "stroke-border/80",
                  )}
                  strokeWidth={lit ? 1.5 : 1}
                  strokeDasharray={lit ? undefined : "4 6"}
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
              <button
                key={category.id}
                type="button"
                className={cn(
                  "absolute -translate-x-1/2 -translate-y-1/2 text-left transition-transform",
                  isActive && "z-10 scale-105",
                )}
                style={{ left: `${pos.x}%`, top: `${pos.y}%` }}
                onMouseEnter={() => setActiveId(category.id)}
                onFocus={() => setActiveId(category.id)}
                aria-pressed={isActive}
              >
                <span
                  className={cn(
                    "flex items-center gap-2 rounded-full border bg-background/90 px-3 py-1.5 shadow-sm backdrop-blur-sm transition-colors",
                    isActive
                      ? "border-primary text-foreground"
                      : "border-border/80 text-muted-foreground hover:border-primary/50 hover:text-foreground",
                  )}
                >
                  <span
                    className={cn(
                      "flex size-6 items-center justify-center rounded-full",
                      isActive ? "bg-primary/15 text-primary" : "bg-muted text-muted-foreground",
                    )}
                  >
                    <Icon className="size-3.5" aria-hidden />
                  </span>
                  <span className="font-heading text-sm whitespace-nowrap">{category.name}</span>
                </span>
              </button>
            )
          })}
        </div>

        <aside className="flex flex-col justify-between border border-border/70 bg-card/40 p-6">
          {active ? (
            <>
              <div>
                <p className="kicker">{t.observations}</p>
                <h3 className="mt-3 font-heading text-2xl">{active.name}</h3>
                {active.description ? (
                  <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
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
            </>
          ) : null}
        </aside>
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
      {variant === "path" ? <PathVariant categories={categories} /> : null}
      {variant === "magazine" ? <MagazineVariant categories={categories} /> : null}
      {variant === "star" ? <StarVariant categories={categories} /> : null}
    </section>
  )
}
