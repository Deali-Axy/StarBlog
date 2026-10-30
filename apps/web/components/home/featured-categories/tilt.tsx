"use client"

import { useRef, type MouseEvent } from "react"
import Link from "next/link"
import { ArrowRight } from "lucide-react"
import {
  motion,
  useMotionTemplate,
  useMotionValue,
  useReducedMotion,
  useSpring,
  useTransform,
} from "motion/react"

import { useI18n } from "@/components/app-providers"
import { resolveCategoryIcon } from "@/components/home/featured-categories/icons"
import type { FeaturedCategoryResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { eyebrowClassName, headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

function TiltCard({
  category,
  index,
  featured,
}: {
  category: FeaturedCategoryResponse
  index: number
  featured: boolean
}) {
  const { t } = useI18n()
  const reduceMotion = useReducedMotion()
  const ref = useRef<HTMLAnchorElement>(null)
  const Icon = resolveCategoryIcon(category.iconCssClass, category.name)

  const rawX = useMotionValue(0)
  const rawY = useMotionValue(0)
  const x = useSpring(rawX, { stiffness: 220, damping: 22 })
  const y = useSpring(rawY, { stiffness: 220, damping: 22 })
  const rotateX = useTransform(y, [-0.5, 0.5], [10, -10])
  const rotateY = useTransform(x, [-0.5, 0.5], [-12, 12])
  const sheenX = useTransform(x, [-0.5, 0.5], [0, 100])
  const sheenY = useTransform(y, [-0.5, 0.5], [0, 100])
  const sheen = useMotionTemplate`radial-gradient(420px circle at ${sheenX}% ${sheenY}%, oklch(0.82 0.10 78 / 0.22), transparent 55%)`

  function onMove(event: MouseEvent<HTMLAnchorElement>) {
    if (reduceMotion || !ref.current) return
    const rect = ref.current.getBoundingClientRect()
    rawX.set((event.clientX - rect.left) / rect.width - 0.5)
    rawY.set((event.clientY - rect.top) / rect.height - 0.5)
  }

  function onLeave() {
    rawX.set(0)
    rawY.set(0)
  }

  return (
    <motion.div
      className={cn(
        "min-h-0 perspective-[1000px]",
        featured ? "md:col-span-2 md:row-span-2" : "md:col-span-1",
      )}
      initial={reduceMotion ? false : { opacity: 0, y: 18 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: reduceMotion ? 0 : index * 0.05, type: "spring", stiffness: 300, damping: 28 }}
    >
      <motion.div
        style={
          reduceMotion
            ? undefined
            : {
                rotateX,
                rotateY,
              }
        }
        className={cn("h-full", !reduceMotion && "transform-3d")}
      >
        <Link
          ref={ref}
          href={routes.blogList({ categoryId: category.categoryId })}
          className={cn(
            "group relative flex h-full min-h-[11rem] flex-col justify-between overflow-hidden border border-border/70 bg-card/50 p-5 transition-colors hover:border-primary/45 md:p-6",
            featured && "md:min-h-[22rem]",
          )}
          onMouseMove={onMove}
          onMouseLeave={onLeave}
        >
          <motion.div
            className="pointer-events-none absolute inset-0 opacity-0 transition-opacity duration-300 group-hover:opacity-100"
            style={reduceMotion ? undefined : { background: sheen }}
            aria-hidden
          />
          <div className="relative flex items-start justify-between gap-3">
            <p className={eyebrowClassName}>{featured ? t.featured : t.paths}</p>
            <Icon className={cn("text-primary/80", featured ? "size-6" : "size-4")} aria-hidden />
          </div>
          <div className="relative mt-4 translate-z-6">
            <h3
              className={cn(
                headingClassName,
                "font-heading leading-snug group-hover:text-primary",
                featured ? "text-2xl md:text-3xl" : "text-lg",
              )}
            >
              {category.name}
            </h3>
            {category.description ? (
              <p
                className={cn(
                  "mt-2 text-sm leading-relaxed text-muted-foreground",
                  featured ? "line-clamp-5" : "line-clamp-3",
                )}
              >
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
    </motion.div>
  )
}

export function TiltVariant({ categories }: { categories: FeaturedCategoryResponse[] }) {
  const { t } = useI18n()

  return (
    <div className="space-y-4">
      <p className="text-xs text-muted-foreground">{t.tiltHint}</p>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 md:grid-cols-3 md:auto-rows-[minmax(11rem,auto)]">
        {categories.map((category, index) => (
          <TiltCard key={category.id} category={category} index={index} featured={index === 0} />
        ))}
      </div>
    </div>
  )
}
