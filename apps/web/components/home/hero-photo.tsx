"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"
import { useState } from "react"
import { ArrowUpRight, RefreshCw } from "lucide-react"

import { getRandomPhoto } from "@/lib/api/queries"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { routes } from "@/lib/routes"
import type { PhotoResponse } from "@/lib/api/types"
import { cn } from "@/lib/utils"

function preloadThumbnail(id: string) {
  return new Promise<void>((resolve) => {
    const image = new Image()
    image.onload = () => resolve()
    image.onerror = () => resolve()
    image.src = photoThumbnail(id, 1200)
  })
}

export function HeroPhoto({ photo }: { photo: PhotoResponse }) {
  const { t } = useI18n()
  const [current, setCurrent] = useState(photo)
  const [pending, setPending] = useState(false)
  const location = current.location?.trim()
  const hasLocation = location && !/^(unknown|未知|未知地点)$/i.test(location)

  async function swap() {
    if (pending) return
    setPending(true)
    try {
      let next = current
      for (let attempt = 0; attempt < 4; attempt += 1) {
        const candidate = await getRandomPhoto()
        if (candidate.id !== current.id) {
          next = candidate
          break
        }
      }
      if (next.id === current.id) return
      await preloadThumbnail(next.id)
      setCurrent(next)
    } catch {
      // Keep the current photo so a failed request can be retried.
    } finally {
      setPending(false)
    }
  }

  return (
    <figure className="home-hero-photo">
      <div className="home-hero-photo-heading">
        <span className="home-hero-photo-label"><span aria-hidden="true">↳</span> {t.photoNotebook}</span>
        <span className="home-hero-photo-note">{t.photography}</span>
      </div>
      <Link href={routes.photo(current.id)} className="home-hero-photo-frame group" aria-label={`${t.viewLargePhoto}: ${current.title}`}>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={photoThumbnail(current.id, 1200)}
          srcSet={photoThumbnailSet(current.id)}
          sizes="(min-width: 1152px) 600px, (min-width: 768px) 52vw, calc(100vw - 32px)"
          alt={current.title}
          width={current.width}
          height={current.height}
          fetchPriority="high"
          className={cn("home-hero-photo-image", pending && "opacity-70")}
        />
        <span className="home-hero-photo-open"><ArrowUpRight aria-hidden="true" className="size-5" /></span>
      </Link>
      <figcaption className="home-hero-photo-caption">
        <div aria-live="polite" aria-atomic="true" className="flex min-w-0 flex-wrap items-baseline gap-x-3 gap-y-1 text-xs">
          <span className="break-words">{current.title}</span>
          {hasLocation ? <span className="text-muted-foreground">/ {location}</span> : null}
        </div>
        <button
          type="button"
          className="home-hero-photo-swap"
          disabled={pending}
          aria-busy={pending}
          onClick={() => void swap()}
        >
          <RefreshCw aria-hidden="true" className={cn("size-3.5", pending && "motion-safe:animate-spin")} />{t.anotherPhoto}
        </button>
      </figcaption>
    </figure>
  )
}
