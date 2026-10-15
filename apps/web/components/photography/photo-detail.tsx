"use client"

import { useI18n } from "@/components/app-providers"
import { ChevronLeft, ChevronRight, Share2, X } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useRef, useState } from "react"
import { toast } from "sonner"

import { AuthorMark, usePhotoStudio } from "@/components/photography/photo-studio"
import { getAdjacentPhoto } from "@/lib/api/queries"
import type { PhotoResponse } from "@/lib/api/types"
import { formatDate, formatRelativeTime } from "@/lib/format"
import { photoOriginal, photoThumbnail } from "@/lib/media"
import { photoLocation } from "@/lib/photography/path"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

const roundIcon =
  "grid size-10 place-items-center rounded-full bg-black/75 text-white ring-1 ring-white/20 transition hover:bg-white/15 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white disabled:pointer-events-none disabled:opacity-30"

export function PhotoDetail({ photo }: { photo: PhotoResponse }) {
  const { t, locale } = useI18n()
  const router = useRouter()
  const {
    photos,
    featured,
    featuredIds,
    filter,
    hasMore,
    loadMore,
    leavePhoto,
    lightbox,
    setLightbox,
    authorName,
    retainScroll,
  } = usePhotoStudio()
  const dialogRef = useRef<HTMLDivElement>(null)
  const lightboxRef = useRef<HTMLDivElement>(null)
  const closeRef = useRef<HTMLAnchorElement>(null)
  const lightboxCloseRef = useRef<HTMLButtonElement>(null)
  const [adjacent, setAdjacent] = useState<{
    id: string
    previous: PhotoResponse | null
    next: PhotoResponse | null
  } | null>(null)

  const collection = filter === "featured" ? featured : photos
  const index = collection.findIndex((item) => item.id === photo.id)
  const listedPrevious = index > 0 ? (collection[index - 1] ?? null) : null
  const listedNext = index >= 0 && index < collection.length - 1 ? (collection[index + 1] ?? null) : null
  const previous = index >= 0 ? listedPrevious : adjacent?.id === photo.id ? adjacent.previous : null
  const next = index >= 0 ? listedNext : adjacent?.id === photo.id ? adjacent.next : null
  const canLoadNext = filter === "all" && index >= 0 && index === photos.length - 1 && hasMore
  const location = photoLocation(photo.location)
  const featuredPhoto = featuredIds.has(photo.id)
  const dimensions = photo.width > 0 && photo.height > 0 ? `${photo.width} × ${photo.height}` : ""

  useEffect(() => retainScroll(), [retainScroll])

  useEffect(() => {
    const img = new Image()
    img.src = photoOriginal(photo.filePath)
  }, [photo.filePath])

  useEffect(() => {
    if (index !== -1) return
    let cancelled = false
    void Promise.all([
      getAdjacentPhoto(photo.id, "previous"),
      getAdjacentPhoto(photo.id, "next"),
    ]).then(([previousPhoto, nextPhoto]) => {
      if (!cancelled) setAdjacent({ id: photo.id, previous: previousPhoto, next: nextPhoto })
    })
    return () => {
      cancelled = true
    }
  }, [photo.id, index])

  useEffect(() => {
    if (previous) router.prefetch(routes.photo(previous.id))
    if (next) router.prefetch(routes.photo(next.id))
  }, [previous, next, router])

  function go(direction: "previous" | "next") {
    const target = direction === "previous" ? previous : next
    if (target) {
      router.replace(routes.photo(target.id), { scroll: false })
      return
    }
    if (direction === "next" && canLoadNext) {
      void loadMore().then((added) => {
        const upcoming = added.find((item) => item.id !== photo.id)
        if (upcoming) router.replace(routes.photo(upcoming.id), { scroll: false })
      })
    }
  }

  useEffect(() => {
    const node = lightbox ? lightboxCloseRef.current : closeRef.current
    node?.focus({ preventScroll: true })
  }, [photo.id, lightbox])

  useEffect(() => {
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") {
        event.preventDefault()
        if (lightbox) setLightbox(false)
        else closeRef.current?.click()
        return
      }
      if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
        event.preventDefault()
        go(event.key === "ArrowLeft" ? "previous" : "next")
        return
      }
      if (event.key === "Tab") trapFocus(event, lightbox ? lightboxRef.current : dialogRef.current)
    }
    window.addEventListener("keydown", onKey)
    return () => window.removeEventListener("keydown", onKey)
  })

  function share() {
    const url = window.location.href
    if (navigator.share) {
      void navigator.share({ title: photo.title, url }).catch(() => {})
      return
    }
    void navigator.clipboard.writeText(url)
    toast.success(t.linkCopied)
  }

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/80">
        <div
          ref={dialogRef}
          role="dialog"
          aria-modal="true"
          aria-labelledby="photo-dialog-title"
          inert={lightbox || undefined}
          className="flex h-full items-center justify-center sm:p-4 lg:px-16"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) closeRef.current?.click()
          }}
        >
          <div className="relative z-10 flex h-full w-full flex-col overflow-hidden bg-[#141414] sm:h-auto sm:max-h-[calc(100dvh-2rem)] sm:max-w-[min(1240px,calc(100vw-8rem))] sm:rounded-2xl sm:ring-1 sm:ring-white/10 lg:grid lg:grid-cols-[minmax(0,1fr)_22.5rem]">
            <div className="flex items-center justify-center bg-[#141414]">
              <button
                type="button"
                aria-label={t.viewLargePhoto}
                onClick={() => setLightbox(true)}
                className="flex w-full cursor-zoom-in items-center justify-center p-3 sm:p-4"
              >
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={photoThumbnail(photo.id, 1800)}
                  alt={photo.title || t.photography}
                  className="max-h-[42dvh] w-full object-contain lg:max-h-[min(78dvh,820px)]"
                />
              </button>
            </div>
            <aside className="relative min-h-0 flex-1 lg:border-l lg:border-white/10">
              <div className="px-5 py-5 sm:px-6 lg:absolute lg:inset-0 lg:overflow-y-auto">
              <div className="flex items-center gap-3">
                <AuthorMark name={authorName} className="size-10 text-sm" />
                <p className="min-w-0 flex-1 truncate text-sm font-medium">{authorName}</p>
                <button
                  type="button"
                  onClick={share}
                  aria-label={t.share}
                  className="grid size-9 place-items-center rounded-full text-white/80 transition hover:bg-white/10 hover:text-white"
                >
                  <Share2 className="size-4" />
                </button>
              </div>
              <h1 id="photo-dialog-title" className="mt-6 font-sans text-xl font-medium tracking-normal text-white">
                {photo.title || t.photography}
              </h1>
              {location && location !== photo.title ? (
                <p className="mt-3 text-sm leading-7 text-white/80">{location}</p>
              ) : null}
              <p className="mt-3 text-xs text-white/45">{formatRelativeTime(photo.createTime, locale)}</p>
              {featuredPhoto ? (
                <p className="mt-4 inline-flex items-center gap-1.5 text-xs text-white/70">
                  <AwardMark />
                  {t.featured}
                </p>
              ) : null}
              <h2 className="mt-8 font-sans text-sm font-medium tracking-normal text-white">{t.photoDetails}</h2>
              <dl className="mt-4 space-y-3 text-sm">
                {location ? <DetailRow label={t.locationLabel} value={location} /> : null}
                {dimensions ? <DetailRow label={t.dimensions} value={dimensions} /> : null}
                <DetailRow label={t.capturedAt} value={formatDate(photo.createTime)} />
              </dl>
              </div>
            </aside>
          </div>
          <Link
            ref={closeRef}
            href={routes.photography}
            replace
            scroll={false}
            aria-label={t.close}
            onClick={leavePhoto}
            className={cn(roundIcon, "absolute top-3 right-3 z-20 sm:top-4 sm:right-4")}
          >
            <X className="size-5" />
          </Link>
          <NavControl
            direction="previous"
            label={t.previousPhoto}
            photo={previous}
            disabled={!previous}
            onFallback={() => go("previous")}
          />
          <NavControl
            direction="next"
            label={t.nextPhoto}
            photo={next}
            disabled={!next && !canLoadNext}
            onFallback={() => go("next")}
          />
        </div>
      </div>
      {lightbox ? (
        <PhotoLightbox
          panelRef={lightboxRef}
          closeRef={lightboxCloseRef}
          photo={photo}
          location={location}
          previous={previous}
          next={next}
          canLoadNext={canLoadNext}
          onClose={() => setLightbox(false)}
          onPrevious={() => go("previous")}
          onNext={() => go("next")}
        />
      ) : null}
    </>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-baseline justify-between gap-6">
      <dt className="shrink-0 text-white/45">{label}</dt>
      <dd className="min-w-0 text-right text-white/85">{value}</dd>
    </div>
  )
}

function NavControl({
  direction,
  label,
  photo,
  disabled,
  onFallback,
}: {
  direction: "previous" | "next"
  label: string
  photo: PhotoResponse | null
  disabled: boolean
  onFallback: () => void
}) {
  const className = cn(
    roundIcon,
    "absolute z-20 size-11 -translate-y-1/2 top-[21dvh] lg:top-1/2",
    direction === "previous" ? "left-2 sm:left-4" : "right-2 sm:right-4",
  )
  const icon = direction === "previous" ? <ChevronLeft className="size-5" /> : <ChevronRight className="size-5" />
  if (!photo) {
    return (
      <button type="button" aria-label={label} disabled={disabled} onClick={onFallback} className={className}>
        {icon}
      </button>
    )
  }
  return (
    <Link
      href={routes.photo(photo.id)}
      replace
      scroll={false}
      aria-label={label}
      className={className}
    >
      {icon}
    </Link>
  )
}

function PhotoLightbox({
  panelRef,
  closeRef,
  photo,
  location,
  previous,
  next,
  canLoadNext,
  onClose,
  onPrevious,
  onNext,
}: {
  panelRef: React.RefObject<HTMLDivElement | null>
  closeRef: React.RefObject<HTMLButtonElement | null>
  photo: PhotoResponse
  location: string
  previous: PhotoResponse | null
  next: PhotoResponse | null
  canLoadNext: boolean
  onClose: () => void
  onPrevious: () => void
  onNext: () => void
}) {
  const { t } = useI18n()
  return (
    <div
      ref={panelRef}
      role="dialog"
      aria-modal="true"
      aria-label={t.viewLargePhoto}
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose()
      }}
    >
      <button
        ref={closeRef}
        type="button"
        aria-label={t.closeLargePhoto}
        onClick={onClose}
        className={cn(roundIcon, "absolute top-3 right-3 z-20 sm:top-4 sm:right-4")}
      >
        <X className="size-5" />
      </button>
      <LightboxNav label={t.previousPhoto} direction="previous" disabled={!previous} onClick={onPrevious} />
      <LightboxNav label={t.nextPhoto} direction="next" disabled={!next && !canLoadNext} onClick={onNext} />
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src={photoOriginal(photo.filePath)}
        alt={photo.title || t.photography}
        className="max-h-[calc(100dvh-4.5rem)] max-w-[calc(100vw-1.5rem)] object-contain sm:max-w-[calc(100vw-7rem)]"
      />
      <p className="pointer-events-none absolute bottom-4 left-4 text-sm text-white/80 drop-shadow-[0_1px_2px_rgba(0,0,0,0.8)] sm:left-6">
        {photo.title}
        {location ? <span className="text-white/40"> · {location}</span> : null}
      </p>
    </div>
  )
}

function LightboxNav({
  label,
  direction,
  disabled,
  onClick,
}: {
  label: string
  direction: "previous" | "next"
  disabled: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className={cn(
        roundIcon,
        "absolute top-1/2 z-20 size-11 -translate-y-1/2",
        direction === "previous" ? "left-2 sm:left-4" : "right-2 sm:right-4",
      )}
    >
      {direction === "previous" ? <ChevronLeft className="size-5" /> : <ChevronRight className="size-5" />}
    </button>
  )
}

function AwardMark() {
  return (
    <svg viewBox="0 0 24 24" className="size-3.5" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <circle cx="12" cy="9" r="5.25" />
      <path d="M8.2 13.2 7 21l5-2.4L17 21l-1.2-7.8" strokeLinejoin="round" />
    </svg>
  )
}

function trapFocus(event: KeyboardEvent, root: HTMLElement | null) {
  if (!root) return
  const nodes = [...root.querySelectorAll<HTMLElement>("a[href], button:not(:disabled), [tabindex]:not([tabindex='-1'])")]
  const list = nodes.filter((node) => !node.hasAttribute("disabled") && node.tabIndex !== -1)
  if (list.length === 0) return
  const first = list[0]
  const last = list[list.length - 1]
  if (!first || !last) return
  if (event.shiftKey && document.activeElement === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault()
    first.focus()
  }
}
