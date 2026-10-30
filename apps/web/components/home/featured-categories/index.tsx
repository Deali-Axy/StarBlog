"use client"

import Link from "next/link"
import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { AccordionVariant } from "@/components/home/featured-categories/accordion"
import { CatalogVariant } from "@/components/home/featured-categories/catalog"
import { DeckVariant } from "@/components/home/featured-categories/deck"
import { RailVariant } from "@/components/home/featured-categories/rail"
import { TiltVariant } from "@/components/home/featured-categories/tilt"
import { Button } from "@/components/ui/button"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import type { FeaturedCategoriesVariant } from "@/lib/featured-categories-variant"
import { routes } from "@/lib/routes"
import { eyebrowClassName, headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const layouts = {
  deck: DeckVariant,
  rail: RailVariant,
  catalog: CatalogVariant,
  accordion: AccordionVariant,
  tilt: TiltVariant,
} satisfies Record<FeaturedCategoriesVariant, React.ComponentType<{ categories: FeaturedCategoryResponse[] }>>

export function FeaturedCategories({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()
  const { categories: variant } = useHomeDesign()
  if (categories.length === 0) return null
  const Layout = layouts[variant]
  return (
    <section className="mt-20" data-home-categories={variant} aria-labelledby="home-categories-title">
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className={eyebrowClassName}>{t.paths}</p>
          <h2 id="home-categories-title" className={cn(headingClassName, "mt-2 text-2xl")}>{t.featuredCategories}</h2>
        </div>
        <Button variant="ghost" size="sm" asChild><Link href={routes.blog}>{t.viewAll}</Link></Button>
      </div>
      <Layout categories={categories} />
    </section>
  )
}
