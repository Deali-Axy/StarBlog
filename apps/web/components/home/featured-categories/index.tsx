"use client"

import { useState } from "react"
import Link from "next/link"

import { useI18n } from "@/components/app-providers"
import { AccordionVariant } from "@/components/home/featured-categories/accordion"
import { CatalogVariant } from "@/components/home/featured-categories/catalog"
import { DeckVariant } from "@/components/home/featured-categories/deck"
import { RailVariant } from "@/components/home/featured-categories/rail"
import { TiltVariant } from "@/components/home/featured-categories/tilt"
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

export function FeaturedCategories({
  categories,
  initialVariant = FEATURED_CATEGORIES_VARIANT,
  preview = FEATURED_CATEGORIES_PREVIEW,
}: {
  categories: FeaturedCategoryResponse[]
  initialVariant?: FeaturedCategoriesVariant
  preview?: boolean
}) {
  const { t, locale } = useI18n()
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
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="kicker">{t.paths}</p>
          <h2 className="mt-2 text-2xl">{t.featuredCategories}</h2>
        </div>
        {preview ? (
          <div
            className="flex max-w-full flex-wrap gap-1 rounded-md border border-border/80 bg-card/60 p-1"
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
                  onClick={() => handleVariantChange(option.id)}
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

      {variant === "deck" ? <DeckVariant categories={categories} /> : null}
      {variant === "rail" ? <RailVariant categories={categories} /> : null}
      {variant === "catalog" ? <CatalogVariant categories={categories} /> : null}
      {variant === "accordion" ? <AccordionVariant categories={categories} /> : null}
      {variant === "tilt" ? <TiltVariant categories={categories} /> : null}
    </section>
  )
}
