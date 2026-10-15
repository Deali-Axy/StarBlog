"use client"

import { useI18n } from "@/components/app-providers"

import Link from "next/link"
import { useState } from "react"

import { Button } from "@/components/ui/button"
import { getRandomPhoto } from "@/lib/api/queries"
import { photoThumbnail } from "@/lib/media"
import { routes } from "@/lib/routes"
import type { PhotoResponse } from "@/lib/api/types"
import { cn } from "@/lib/utils"

function preloadThumbnail(id: string) {
  return new Promise<void>((resolve) => {
    const image = new Image()
    image.onload = () => resolve()
    image.onerror = () => resolve()
    image.src = photoThumbnail(id, 900)
  })
}

export function HeroPhoto({ photo }: { photo: PhotoResponse }) {
  const { t } = useI18n()
  const [current, setCurrent] = useState(photo)
  const [pending, setPending] = useState(false)

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
    <figure className="hidden md:block">
      <Link href={routes.photo(current.id)} className="group block">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={photoThumbnail(current.id, 900)}
          alt={current.title}
          className={cn("h-80 w-full object-cover transition-opacity duration-300", pending && "opacity-70")}
        />
      </Link>
      <figcaption aria-live="polite" className="mt-3 flex items-center justify-between gap-3">
        <div className="flex min-w-0 items-baseline gap-x-4 text-xs text-muted-foreground">
          <span className="truncate">{current.title}</span>
          <span className="truncate">{current.location}</span>
        </div>
        <Button
          type="button"
          variant="link"
          size="sm"
          className="h-auto shrink-0 px-0 text-xs font-normal text-muted-foreground hover:text-foreground"
          disabled={pending}
          onClick={() => void swap()}
        >
          {t.anotherPhoto}
        </Button>
      </figcaption>
    </figure>
  )
}
