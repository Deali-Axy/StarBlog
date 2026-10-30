"use client"

import { ImageOff, LoaderCircle, RotateCw } from "lucide-react"
import { useCallback, useEffect, useLayoutEffect, useRef, useState } from "react"

import { useI18n } from "@/components/app-providers"
import type { PhotoResponse } from "@/lib/api/types"
import { photoOriginal, photoThumbnail } from "@/lib/media"
import { LightboxImageCache, type DecodedPhotoImage } from "@/lib/photography/image-cache"
import { cn } from "@/lib/utils"

interface OriginalState {
  status: "loading" | "ready" | "failed"
  resource: DecodedPhotoImage | null
  cached: boolean
}

export function PhotoLightboxImage({ photo, previewSrc, previous, next, cache }: {
  photo: PhotoResponse
  previewSrc?: string
  previous: PhotoResponse | null
  next: PhotoResponse | null
  cache: LightboxImageCache
}) {
  const { t } = useI18n()
  const src = photoOriginal(photo.filePath)
  const thumbnail = previewSrc || photoThumbnail(photo.id, 800)
  const previousSrc = previous ? photoOriginal(previous.filePath) : null
  const nextSrc = next ? photoOriginal(next.filePath) : null
  const [original, setOriginal] = useState<OriginalState>(() => {
    const resource = cache.peek(src)
    return resource?.status === "ready"
      ? { status: "ready", resource, cached: true }
      : { status: "loading", resource: null, cached: false }
  })
  const [attempt, setAttempt] = useState(0)
  const [showIndicator, setShowIndicator] = useState(false)
  const [preview, setPreview] = useState({ src: thumbnail, ready: Boolean(previewSrc), failed: false })
  const previewReady = thumbnail === previewSrc || (preview.src === thumbnail && preview.ready)
  const previewFailed = preview.src === thumbnail && preview.failed
  const title = photo.title || t.photography

  useEffect(() => {
    let active = true
    const neighbours = [...new Set([previousSrc, nextSrc].filter((url): url is string => Boolean(url) && url !== src))]
    cache.retain([src, ...neighbours])
    const cached = cache.peek(src)?.status === "ready"
    const resource = cache.load(src)
    const timer = cached ? undefined : window.setTimeout(() => {
      if (active) setShowIndicator(true)
    }, 150)

    void resource.promise.then(() => {
      if (!active) return
      window.clearTimeout(timer)
      setShowIndicator(false)
      setOriginal((current) => current.resource === resource && current.status === "ready"
        ? current
        : { status: "ready", resource, cached })
      // Do not compete with the current original or preload after closing.
      for (const neighbour of neighbours) void cache.load(neighbour, "low").promise.catch(() => {})
    }, () => {
      if (!active) return
      window.clearTimeout(timer)
      setShowIndicator(false)
      setOriginal({ status: "failed", resource: null, cached: false })
    })

    return () => {
      active = false
      window.clearTimeout(timer)
    }
  }, [cache, src, previousSrc, nextSrc, attempt])

  const attachPreview = useCallback((image: HTMLImageElement | null) => {
    if (!image?.complete) return
    setPreview((current) => {
      const ready = image.naturalWidth > 0
      return current.src === thumbnail && current.ready === ready && current.failed === !ready
        ? current
        : { src: thumbnail, ready, failed: !ready }
    })
  }, [thumbnail])

  return (
    <div
      data-lightbox-stage
      data-lightbox-photo={photo.id}
      data-state={original.status}
      aria-busy={original.status === "loading"}
      className="relative h-[calc(100dvh-6rem)] w-full overflow-hidden bg-black text-white"
    >
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        ref={attachPreview}
        key={thumbnail}
        data-lightbox-preview
        src={thumbnail}
        alt={original.status === "ready" ? "" : title}
        aria-hidden={original.status === "ready" || undefined}
        width={photo.width || 1500}
        height={photo.height || 1000}
        decoding="async"
        onLoad={() => setPreview({ src: thumbnail, ready: true, failed: false })}
        onError={() => setPreview({ src: thumbnail, ready: false, failed: true })}
        className={cn("absolute inset-0 h-full w-full object-contain", !previewReady && "opacity-0")}
      />
      {original.status === "ready" && original.resource ? (
        <DecodedOriginal resource={original.resource} title={title} animate={!original.cached} />
      ) : null}
      {original.status === "loading" && showIndicator && !previewReady ? (
        <span data-lightbox-loading aria-hidden className="pointer-events-none absolute inset-0 grid place-items-center bg-black">
          <LoaderCircle className="size-5 text-white/55 motion-safe:animate-spin" />
        </span>
      ) : null}
      {original.status === "failed" ? (
        <div data-lightbox-error role="status" className={cn(
          "absolute inset-x-0 bottom-4 mx-auto flex w-fit max-w-full items-center gap-3 rounded-xl bg-black/85 px-4 py-2 text-sm text-white",
          previewFailed && "inset-0 flex-col justify-center rounded-none bg-black",
        )}>
          <ImageOff aria-hidden className="size-5 shrink-0 text-white/65" />
          <p>{t.photoLoadFailed}</p>
          <button
            type="button"
            aria-label={`${t.retry}: ${title}`}
            onClick={() => {
              setOriginal({ status: "loading", resource: null, cached: false })
              setShowIndicator(false)
              setAttempt((value) => value + 1)
            }}
            className="inline-flex min-h-11 shrink-0 items-center gap-2 rounded-lg px-3 text-white hover:bg-white/10 focus-visible:outline-white"
          >
            <RotateCw aria-hidden className="size-4" />{t.retry}
          </button>
        </div>
      ) : null}
    </div>
  )
}

function DecodedOriginal({ resource, title, animate }: {
  resource: DecodedPhotoImage
  title: string
  animate: boolean
}) {
  const frameRef = useRef<HTMLDivElement>(null)

  useLayoutEffect(() => {
    const frame = frameRef.current
    if (!frame) return
    const image = resource.image
    image.setAttribute("alt", title)
    image.setAttribute("draggable", "false")
    image.setAttribute("data-lightbox-original", "")
    image.setAttribute("class", cn("absolute inset-0 h-full w-full object-contain", animate && "motion-safe:animate-in motion-safe:fade-in-0 motion-safe:duration-200 motion-safe:ease-out motion-safe:fill-mode-both"))
    // Reuse the actual decoded element: mounting a second img could refetch it.
    frame.appendChild(image)
    return () => {
      if (image.parentElement === frame) frame.removeChild(image)
    }
  }, [resource, title, animate])

  return <div ref={frameRef} className="absolute inset-0" />
}
