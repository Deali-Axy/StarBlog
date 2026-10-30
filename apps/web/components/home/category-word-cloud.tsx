"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"

import type { CategoryWordCloudItem } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import styles from "./home-sections.module.css"

export function CategoryWordCloud({ items }: { items: CategoryWordCloudItem[] }) {
  const { t } = useI18n()
  if (items.length === 0) return null
  const max = Math.max(...items.map((item) => item.count), 1)

  return (
    <section className={styles.wordCloud} aria-labelledby="home-interests-title">
      <h2 id="home-interests-title">{t.homeInterests}</h2>
      <div className={styles.wordCloudItems}>
        {items.map((item) => {
          const weight = item.count / max
          return (
            <Link
              key={item.id}
              href={routes.blogList({ categoryId: item.id })}
              style={{ fontSize: `${0.8 + weight * 0.45}rem` }}
            >
              {item.name}
              <small>{item.count}</small>
            </Link>
          )
        })}
      </div>
    </section>
  )
}
