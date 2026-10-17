"use client"

import { Award, ChevronLeft, ChevronRight, LoaderCircle, Share2, X } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Dialog } from "radix-ui"
import { useEffect, useRef, useState } from "react"
import { toast } from "sonner"

import { useI18n } from "@/components/app-providers"
import { PhotoImage } from "@/components/photography/photo-image"
import { AuthorMark, usePhotoStudio } from "@/components/photography/photo-studio"
import { getAdjacentPhoto } from "@/lib/api/queries"
import type { PhotoResponse } from "@/lib/api/types"
import { formatDate, formatRelativeTime } from "@/lib/format"
import { photoOriginal, photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { photoLocation } from "@/lib/photography/path"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"

const roundIcon = "grid size-11 shrink-0 place-items-center rounded-full bg-black/65 text-white ring-1 ring-white/20 transition hover:bg-black/85 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white disabled:pointer-events-none disabled:opacity-30"

export function PhotoDetail({ photo }: { photo: PhotoResponse }) {
  const { t, locale } = useI18n()
  const router = useRouter()
  const { photos, featured, featuredIds, filter, hasMore, loadMore, closePhoto, restoreWallFocus, authorName, lightbox, setLightbox } = usePhotoStudio()
  const closeRef = useRef<HTMLButtonElement>(null)
  const lightboxCloseRef = useRef<HTMLButtonElement>(null)
  const zoomRef = useRef<HTMLButtonElement>(null)
  const [navigating, setNavigating] = useState(false)
  const navigationPending = useRef(false)
  const [adjacent, setAdjacent] = useState<{ id: string; previous: PhotoResponse | null; next: PhotoResponse | null } | null>(null)

  const collection = filter === "featured" ? featured : photos
  const index = collection.findIndex((item) => item.id === photo.id)
  const previous = index >= 0 ? collection[index - 1] ?? null : adjacent?.id === photo.id ? adjacent.previous : null
  const next = index >= 0 ? collection[index + 1] ?? null : adjacent?.id === photo.id ? adjacent.next : null
  const canLoadNext = filter === "all" && index === photos.length - 1 && index >= 0 && hasMore
  const location = photoLocation(photo.location)

  useEffect(() => {
    if (index !== -1) return
    let cancelled = false
    void Promise.all([getAdjacentPhoto(photo.id, "previous"), getAdjacentPhoto(photo.id, "next")]).then(([previous, next]) => {
      if (!cancelled) setAdjacent({ id: photo.id, previous, next })
    }).catch(() => {
      if (!cancelled) toast.error(t.photosLoadFailed)
    })
    return () => { cancelled = true }
  }, [photo.id, index, t.photosLoadFailed])

  useEffect(() => {
    if (previous) router.prefetch(routes.photo(previous.id))
    if (next) router.prefetch(routes.photo(next.id))
  }, [previous, next, router])

  async function go(direction: "previous" | "next") {
    if (navigationPending.current) return
    const target = direction === "previous" ? previous : next
    if (target) {
      router.replace(routes.photo(target.id), { scroll: false })
    } else if (direction === "next" && canLoadNext) {
      navigationPending.current = true
      setNavigating(true)
      try {
        const added = await loadMore()
        const upcoming = added.find((item) => item.id !== photo.id)
        if (upcoming) router.replace(routes.photo(upcoming.id), { scroll: false })
        else toast.error(t.photosLoadFailed)
      } finally {
        navigationPending.current = false
        setNavigating(false)
      }
    }
  }

  function onKeyDown(event: React.KeyboardEvent) {
    if (event.target instanceof HTMLElement && event.target.closest("input, textarea, select, [contenteditable=true]")) return
    if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
      event.preventDefault()
      event.stopPropagation()
      void go(event.key === "ArrowLeft" ? "previous" : "next")
    }
  }

  async function share() {
    try {
      const url = window.location.href
      if (navigator.share) await navigator.share({ title: photo.title, url })
      else { await navigator.clipboard.writeText(url); toast.success(t.linkCopied) }
    } catch (error) {
      if (!(error instanceof DOMException && error.name === "AbortError")) toast.error(t.shareFailed)
    }
  }

  return (
    <Dialog.Root open onOpenChange={(open) => { if (!open) closePhoto() }}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/65" />
        <Dialog.Content aria-describedby={undefined} onOpenAutoFocus={(event) => { event.preventDefault(); closeRef.current?.focus({ preventScroll: true }) }} onCloseAutoFocus={(event) => { event.preventDefault(); restoreWallFocus() }} onKeyDown={onKeyDown} className="photography-theme fixed inset-0 z-50 overflow-y-auto text-foreground outline-none">
          <div className="flex min-h-full items-center justify-center sm:p-4 lg:px-16" onPointerDown={(event) => { if (event.target === event.currentTarget) closePhoto() }}>
            <div className="relative flex min-h-dvh w-full flex-col bg-background sm:min-h-0 sm:max-w-[1240px] sm:overflow-hidden sm:rounded-xl sm:shadow-2xl lg:grid lg:grid-cols-[minmax(0,1fr)_22rem]">
              <PhotoImage key={`preview-${photo.id}`} src={photoThumbnail(photo.id, 1600)} srcSet={photoThumbnailSet(photo.id)} sizes="(min-width: 1400px) 888px, (min-width: 1024px) calc(100vw - 480px), 100vw" alt={photo.title || t.photography} width={photo.width || 1500} height={photo.height || 1000} priority className="flex min-h-[220px] items-center justify-center bg-muted/40" imageClassName="h-auto max-h-[44dvh] w-full object-contain lg:max-h-[82dvh]">
                <button ref={zoomRef} type="button" aria-label={t.viewLargePhoto} onClick={() => setLightbox(true)} className="absolute inset-0 cursor-zoom-in" />
              </PhotoImage>
              <aside className="min-w-0 px-6 py-7 lg:max-h-[82dvh] lg:overflow-y-auto lg:border-l lg:border-border">
                <div className="flex items-center gap-3">
                  <AuthorMark name={authorName} className="size-10 text-sm" />
                  <p className="min-w-0 flex-1 truncate text-sm font-medium">{authorName}</p>
                  <button type="button" onClick={() => void share()} aria-label={t.share} className="photo-icon text-muted-foreground hover:text-foreground"><Share2 className="size-4" /></button>
                </div>
                <Dialog.Title asChild><h1 className="mt-7 break-words font-sans text-2xl leading-relaxed font-medium tracking-normal">{photo.title || t.photography}</h1></Dialog.Title>
                {location && location !== photo.title ? <p className="mt-3 break-words text-sm leading-7 text-muted-foreground">{location}</p> : null}
                <p className="mt-3 text-xs text-muted-foreground">{formatRelativeTime(photo.createTime, locale)}</p>
                {featuredIds.has(photo.id) ? <p className="mt-5 inline-flex items-center gap-1.5 rounded-lg bg-muted px-2.5 py-1.5 text-xs"><Award className="size-3.5" />{t.featured}</p> : null}
                <h2 className="mt-9 border-t border-border pt-6 font-sans text-sm font-medium tracking-normal">{t.photoDetails}</h2>
                <dl className="mt-5 space-y-4 text-sm">
                  {location ? <DetailRow label={t.locationLabel} value={location} /> : null}
                  {photo.width > 0 && photo.height > 0 ? <DetailRow label={t.dimensions} value={`${photo.width} × ${photo.height}`} /> : null}
                  <DetailRow label={t.uploadedAt} value={formatDate(photo.createTime)} />
                </dl>
              </aside>
            </div>
          </div>
          <Dialog.Close asChild><button ref={closeRef} type="button" aria-label={t.close} className={cn(roundIcon, "fixed top-3 right-3 z-20 sm:top-4 sm:right-4")}><X className="size-5" /></button></Dialog.Close>
          <NavControl direction="previous" label={t.previousPhoto} photo={previous} disabled={!previous || navigating} onClick={() => void go("previous")} />
          <NavControl direction="next" label={t.nextPhoto} photo={next} disabled={(!next && !canLoadNext) || navigating} loading={navigating} onClick={() => void go("next")} />
          <Dialog.Root open={lightbox} onOpenChange={setLightbox}>
            <Dialog.Portal>
              <Dialog.Overlay className="fixed inset-0 z-[60] bg-black" />
              <Dialog.Content aria-describedby={undefined} onKeyDown={onKeyDown} onOpenAutoFocus={(event) => { event.preventDefault(); lightboxCloseRef.current?.focus({ preventScroll: true }) }} onCloseAutoFocus={(event) => { event.preventDefault(); zoomRef.current?.focus({ preventScroll: true }) }} className="photography-theme fixed inset-0 z-[60] flex items-center justify-center bg-black p-3 text-white outline-none sm:px-16" onPointerDown={(event) => { if (event.target === event.currentTarget) setLightbox(false) }}>
                <Dialog.Title className="sr-only">{t.viewLargePhoto}: {photo.title || t.photography}</Dialog.Title>
                <PhotoImage key={`original-${photo.id}`} src={photoOriginal(photo.filePath)} alt={photo.title || t.photography} width={photo.width || 1500} height={photo.height || 1000} priority className="flex min-h-40 min-w-40 max-w-full items-center justify-center bg-black" imageClassName="h-auto max-h-[calc(100dvh-6rem)] w-auto max-w-full object-contain" />
                <Dialog.Close asChild><button ref={lightboxCloseRef} type="button" aria-label={t.closeLargePhoto} className={cn(roundIcon, "absolute top-3 right-3 z-20 sm:top-4 sm:right-4")}><X className="size-5" /></button></Dialog.Close>
                <NavControl lightbox direction="previous" label={t.previousPhoto} disabled={!previous || navigating} onClick={() => void go("previous")} />
                <NavControl lightbox direction="next" label={t.nextPhoto} disabled={(!next && !canLoadNext) || navigating} loading={navigating} onClick={() => void go("next")} />
                <p className="pointer-events-none absolute inset-x-4 bottom-4 truncate text-sm text-white/80 sm:inset-x-6">{photo.title}{location ? <span className="text-white/50"> · {location}</span> : null}</p>
              </Dialog.Content>
            </Dialog.Portal>
          </Dialog.Root>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return <div className="flex items-baseline justify-between gap-6"><dt className="shrink-0 text-muted-foreground">{label}</dt><dd className="min-w-0 break-words text-right">{value}</dd></div>
}

function NavControl({ direction, label, photo, disabled, loading, lightbox, onClick }: {
  direction: "previous" | "next"; label: string; photo?: PhotoResponse | null; disabled: boolean; loading?: boolean; lightbox?: boolean; onClick: () => void
}) {
  const className = cn(roundIcon, "fixed z-20 -translate-y-1/2", lightbox ? "top-1/2" : "top-[22dvh] lg:top-1/2", direction === "previous" ? "left-2 sm:left-4" : "right-2 sm:right-4")
  const icon = loading ? <LoaderCircle className="size-5 animate-spin" /> : direction === "previous" ? <ChevronLeft className="size-5" /> : <ChevronRight className="size-5" />
  if (photo && !disabled && !lightbox) return <Link href={routes.photo(photo.id)} replace scroll={false} aria-label={label} className={className}>{icon}</Link>
  return <button type="button" aria-label={label} disabled={disabled} onClick={onClick} className={className}>{icon}</button>
}
