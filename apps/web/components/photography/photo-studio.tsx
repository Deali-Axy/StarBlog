"use client"

import { Award, Camera, Check, Grid2X2, LayoutGrid, LoaderCircle, Shuffle } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { createContext, useCallback, useContext, useLayoutEffect, useMemo, useRef, useState, useSyncExternalStore, type CSSProperties, type ReactNode } from "react"
import { toast } from "sonner"

import { useI18n } from "@/components/app-providers"
import { PhotoImage } from "@/components/photography/photo-image"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { getRandomPhoto, listPhotos } from "@/lib/api/queries"
import type { PageResult, PhotoResponse } from "@/lib/api/types"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { layoutJustified, photoAspect, targetRowHeight } from "@/lib/photography/justify"
import { densityScale, layoutMasonry, photoColumns, type PhotoDensity, type PhotoView } from "@/lib/photography/layout"
import { photoPageSize } from "@/lib/photography/page-size"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"

type PhotoFilter = "all" | "featured"

interface PhotoStudioContextValue {
  photos: PhotoResponse[]
  featured: PhotoResponse[]
  featuredIds: ReadonlySet<string>
  filter: PhotoFilter
  view: PhotoView
  density: PhotoDensity
  setView: (view: PhotoView) => void
  setDensity: (density: PhotoDensity) => void
  hasMore: boolean
  loadMore: () => Promise<PhotoResponse[]>
  rememberWall: (id?: string) => void
  closePhoto: () => void
  restoreWallFocus: () => void
  lightbox: boolean
  setLightbox: (open: boolean) => void
  authorName: string
}

const PhotoStudioContext = createContext<PhotoStudioContextValue | null>(null)
const preferenceEvent = "starblog:photography-preferences"
let memoryPreferences = "masonry:standard"
let storageUnavailable = false

function subscribePreferences(callback: () => void) {
  window.addEventListener("storage", callback)
  window.addEventListener(preferenceEvent, callback)
  return () => {
    window.removeEventListener("storage", callback)
    window.removeEventListener(preferenceEvent, callback)
  }
}

function preferenceSnapshot() {
  if (storageUnavailable) return memoryPreferences
  try {
    const view = localStorage.getItem("starblog.photography.view")
    const density = localStorage.getItem("starblog.photography.density")
    return `${view === "justified" ? view : "masonry"}:${density === "compact" || density === "comfortable" ? density : "standard"}`
  } catch {
    return memoryPreferences
  }
}

export function usePhotoStudio() {
  const value = useContext(PhotoStudioContext)
  if (!value) throw new Error("usePhotoStudio must be used inside PhotoStudio")
  return value
}

function dedupePhotos(photos: PhotoResponse[]) {
  const seen = new Set<string>()
  return photos.filter((photo) => {
    if (seen.has(photo.id)) return false
    seen.add(photo.id)
    return true
  })
}

export function PhotoStudio({ initialPage, featured: featuredPhotos, children }: {
  initialPage: PageResult<PhotoResponse>
  featured: PhotoResponse[]
  children: ReactNode
}) {
  const { t } = useI18n()
  const router = useRouter()
  const [photos, setPhotos] = useState(initialPage.items)
  const [totalCount, setTotalCount] = useState(initialPage.totalCount)
  const [hasMore, setHasMore] = useState(initialPage.page < initialPage.totalPages)
  const [filter, setFilter] = useState<PhotoFilter>("all")
  const [loadingMore, setLoadingMore] = useState(false)
  const [loadError, setLoadError] = useState(false)
  const [openingRandom, setOpeningRandom] = useState(false)
  const [lightbox, setLightbox] = useState(false)
  const preferences = useSyncExternalStore(subscribePreferences, preferenceSnapshot, () => "masonry:standard")
  const [view, density] = preferences.split(":") as [PhotoView, PhotoDensity]
  const paging = useRef({ page: initialPage.page, hasMore: initialPage.page < initialPage.totalPages })
  const loading = useRef(false)
  const fromWall = useRef(false)
  const wallPosition = useRef({ scrollY: 0, trigger: null as HTMLElement | null })
  const anchor = useRef<{ id: string; top: number } | null>(null)

  const featured = useMemo(() => [...featuredPhotos].sort((a, b) => Date.parse(b.createTime) - Date.parse(a.createTime)), [featuredPhotos])
  const featuredIds = useMemo(() => new Set(featured.map((photo) => photo.id)), [featured])

  const changePreferences = useCallback((nextView: PhotoView, nextDensity: PhotoDensity) => {
    if (`${nextView}:${nextDensity}` === preferenceSnapshot()) return
    const boundary = document.querySelector("[data-photo-toolbar]")?.getBoundingClientRect().bottom ?? 72
    const visible = Array.from(document.querySelectorAll<HTMLElement>("[data-photo-id]")).find((node) => node.getBoundingClientRect().bottom > boundary)
    if (visible) anchor.current = { id: visible.id, top: visible.getBoundingClientRect().top }
    memoryPreferences = `${nextView}:${nextDensity}`
    try {
      localStorage.setItem("starblog.photography.view", nextView)
      localStorage.setItem("starblog.photography.density", nextDensity)
    } catch { storageUnavailable = true }
    window.dispatchEvent(new Event(preferenceEvent))
  }, [])

  const setView = useCallback((next: PhotoView) => changePreferences(next, density), [changePreferences, density])
  const setDensity = useCallback((next: PhotoDensity) => changePreferences(view, next), [changePreferences, view])
  const restoreAnchor = useCallback(() => {
    if (!anchor.current) return
    const node = document.getElementById(anchor.current.id)
    if (node) window.scrollBy(0, node.getBoundingClientRect().top - anchor.current.top)
    anchor.current = null
  }, [])

  const loadMore = useCallback(async () => {
    if (loading.current || !paging.current.hasMore) return []
    loading.current = true
    setLoadingMore(true)
    setLoadError(false)
    try {
      const nextPage = await listPhotos({ page: paging.current.page + 1, pageSize: photoPageSize })
      paging.current = { page: nextPage.page, hasMore: nextPage.page < nextPage.totalPages }
      setPhotos((current) => dedupePhotos([...current, ...nextPage.items]))
      setTotalCount(nextPage.totalCount)
      setHasMore(paging.current.hasMore)
      return nextPage.items
    } catch {
      setLoadError(true)
      return []
    } finally {
      loading.current = false
      setLoadingMore(false)
    }
  }, [])

  const rememberWall = useCallback((id?: string) => {
    fromWall.current = true
    wallPosition.current = {
      scrollY: window.scrollY,
      trigger: id ? document.getElementById(`photo-link-${id}`) : document.activeElement as HTMLElement | null,
    }
  }, [])

  const closePhoto = useCallback(() => {
    if (fromWall.current) router.back()
    else router.replace(routes.photography, { scroll: false })
  }, [router])

  const restoreWallFocus = useCallback(() => {
    requestAnimationFrame(() => {
      // A route replacement unmounts the old dialog while the next mounts.
      // Preserve the viewing session until the user actually leaves a photo.
      if (window.location.pathname.startsWith(`${routes.photography}/`)) return
      setLightbox(false)
      if (!fromWall.current) return
      fromWall.current = false
      if (window.location.pathname !== routes.photography) return
      window.scrollTo(0, wallPosition.current.scrollY)
      wallPosition.current.trigger?.focus({ preventScroll: true })
    })
  }, [])

  async function openRandom() {
    if (openingRandom) return
    setOpeningRandom(true)
    rememberWall()
    try {
      const photo = await getRandomPhoto()
      router.push(routes.photo(photo.id), { scroll: false })
    } catch {
      toast.error(t.randomPhotoFailed)
    } finally {
      setOpeningRandom(false)
    }
  }

  const value = useMemo<PhotoStudioContextValue>(() => ({
    photos, featured, featuredIds, filter, view, density, setView, setDensity, hasMore, loadMore,
    rememberWall, closePhoto, restoreWallFocus, lightbox, setLightbox, authorName: site.authorName,
  }), [photos, featured, featuredIds, filter, view, density, setView, setDensity, hasMore, loadMore, rememberWall, closePhoto, restoreWallFocus, lightbox])

  return (
    <PhotoStudioContext.Provider value={value}>
      {children}
      <div className="min-h-[calc(100dvh-12rem)] bg-background text-foreground">
        <div data-photo-toolbar className="photo-inset sticky top-16 z-30 grid grid-cols-2 items-center gap-y-3 bg-background/95 py-4 backdrop-blur-md sm:top-[72px] lg:grid-cols-[1fr_auto_1fr] lg:py-5">
          <span className="text-xs tabular-nums text-muted-foreground" aria-live="polite">{t.photoCount(filter === "featured" ? featured.length : totalCount)}</span>
          <div className="flex items-center justify-end gap-1 lg:justify-center">
            {(["all", "featured"] as const).map((item) => (
              <button key={item} type="button" aria-pressed={filter === item} onClick={() => setFilter(item)} className={cn("min-h-11 rounded-xl px-5 text-sm font-medium transition", filter === item ? "bg-muted text-foreground" : "text-muted-foreground hover:text-foreground")}>
                {item === "all" ? t.all : t.featured}
              </button>
            ))}
          </div>
          <div className="col-span-2 flex items-center justify-between gap-2 lg:col-span-1 lg:justify-end lg:gap-4">
            <button type="button" onClick={() => void openRandom()} disabled={openingRandom || totalCount === 0} className="inline-flex min-h-11 items-center gap-2 rounded-xl px-2 text-xs text-muted-foreground transition hover:bg-muted hover:text-foreground disabled:opacity-40" aria-label={t.randomPhoto}>
              {openingRandom ? <LoaderCircle className="size-4 animate-spin" /> : <Shuffle className="size-4" />}<span className="hidden min-[400px]:inline">{t.randomPhoto}</span>
            </button>
            <div role="group" aria-label={t.photoView} className="flex rounded-xl bg-muted p-1">
              <ViewButton active={view === "masonry"} label={t.masonryView} onClick={() => setView("masonry")}><LayoutGrid className="size-4" /></ViewButton>
              <ViewButton active={view === "justified"} label={t.justifiedView} onClick={() => setView("justified")}><Grid2X2 className="size-4" /></ViewButton>
            </div>
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <span className="hidden sm:inline">{t.photoDensity}</span>
              <Select value={density} onValueChange={(value) => setDensity(value as PhotoDensity)}>
                <SelectTrigger aria-label={t.photoDensity} className="min-w-20 rounded-xl bg-background px-3 text-xs text-foreground data-[size=default]:h-11 dark:bg-background dark:hover:bg-muted">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent position="popper" align="end" sideOffset={4} className="photography-theme rounded-xl">
                  <SelectGroup>
                    <SelectItem value="comfortable" className="min-h-11 rounded-lg text-xs">{t.comfortableDensity}</SelectItem>
                    <SelectItem value="standard" className="min-h-11 rounded-lg text-xs">{t.standardDensity}</SelectItem>
                    <SelectItem value="compact" className="min-h-11 rounded-lg text-xs">{t.compactDensity}</SelectItem>
                  </SelectGroup>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
        <PhotoWall photos={filter === "featured" ? featured : photos} view={view} density={density} featuredIds={featuredIds} authorName={site.authorName} onOpen={rememberWall} onLayout={restoreAnchor} empty={filter === "featured" ? t.noFeaturedPhotos : t.noPhotos} />
        {filter === "all" && photos.length > 0 ? (
          <div className="photo-inset flex flex-col items-center gap-3 py-10" aria-busy={loadingMore}>
            {loadError ? <p role="alert" className="text-sm text-muted-foreground">{t.photosLoadFailed}</p> : null}
            {hasMore ? <button type="button" onClick={() => void loadMore()} disabled={loadingMore} className="inline-flex min-h-11 items-center gap-2 rounded-xl border border-border px-7 text-sm font-medium transition hover:bg-muted disabled:opacity-50">
              {loadingMore ? <LoaderCircle className="size-4 animate-spin" /> : null}{loadingMore ? t.loadingMore : loadError ? t.retry : t.loadMore}
            </button> : <p className="flex items-center gap-2 text-xs text-muted-foreground"><Check className="size-3.5" />{t.allPhotosLoaded}</p>}
          </div>
        ) : null}
      </div>
    </PhotoStudioContext.Provider>
  )
}

function ViewButton({ active, label, onClick, children }: { active: boolean; label: string; onClick: () => void; children: ReactNode }) {
  return <button type="button" aria-label={label} title={label} aria-pressed={active} onClick={onClick} className={cn("grid size-9 place-items-center rounded-lg transition", active ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground")}>{children}</button>
}

function PhotoWall({ photos, view, density, featuredIds, authorName, onOpen, onLayout, empty }: {
  photos: PhotoResponse[]; view: PhotoView; density: PhotoDensity; featuredIds: ReadonlySet<string>; authorName: string
  onOpen: (id?: string) => void; onLayout: () => void; empty: string
}) {
  const frameRef = useRef<HTMLDivElement>(null)
  const [frame, setFrame] = useState({ width: 0, viewport: 0 })
  const [naturalAspects, setNaturalAspects] = useState<Record<string, number>>({})
  const { t } = useI18n()

  useLayoutEffect(() => {
    const node = frameRef.current
    if (!node) return
    const update = () => setFrame((current) => current.width === node.clientWidth && current.viewport === window.innerWidth ? current : { width: node.clientWidth, viewport: window.innerWidth })
    update()
    const observer = new ResizeObserver(update)
    observer.observe(node)
    window.addEventListener("resize", update)
    return () => { observer.disconnect(); window.removeEventListener("resize", update) }
  }, [])

  const rememberDimensions = useCallback((id: string, width: number, height: number) => {
    if (width <= 0 || height <= 0) return
    const aspect = width / height
    setNaturalAspects((current) => current[id] === aspect ? current : { ...current, [id]: aspect })
  }, [])
  const aspects = useMemo(() => photos.map((photo) => naturalAspects[photo.id] ?? photoAspect(photo.width, photo.height)), [photos, naturalAspects])
  const layout = useMemo(() => {
    if (!frame.width) return null
    const gap = frame.viewport < 640 ? 12 : 24
    return view === "masonry" ? layoutMasonry(aspects, frame.width, photoColumns(frame.viewport, density), gap) : layoutJustified(aspects, frame.width, targetRowHeight(frame.width) * densityScale(density), gap)
  }, [aspects, frame, view, density])

  useLayoutEffect(() => { if (layout) onLayout() }, [layout, onLayout])

  return (
    <div className="photo-inset pb-4">
      <div ref={frameRef} data-photo-wall data-view={view} data-density={density}>
        {photos.length === 0 ? <div className="py-28 text-center text-sm text-muted-foreground"><Camera className="mx-auto mb-4 size-8 opacity-50" /><p>{empty}</p><Link href={routes.home} className="mt-5 inline-block underline underline-offset-4">{t.backHome}</Link></div> : layout ? (
          <div className="relative" style={{ height: layout.height }}>
            {layout.boxes.map((box) => <PhotoTile key={photos[box.index].id} photo={photos[box.index]} featured={featuredIds.has(photos[box.index].id)} authorName={authorName} priority={box.index < photoColumns(frame.viewport, density)} onOpen={onOpen} onDimensions={rememberDimensions} style={{ position: "absolute", left: box.x, top: box.y, width: box.width, height: box.height }} sizes={`${Math.ceil(box.width)}px`} />)}
          </div>
        ) : <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-6 lg:grid-cols-4 2xl:grid-cols-5">
          {photos.map((photo, index) => <PhotoTile key={photo.id} photo={photo} featured={featuredIds.has(photo.id)} authorName={authorName} priority={index < 5} onOpen={onOpen} onDimensions={rememberDimensions} style={{ aspectRatio: String(aspects[index]) }} sizes="(min-width: 1536px) 20vw, (min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw" />)}
        </div>}
      </div>
    </div>
  )
}

function PhotoTile({ photo, featured, authorName, priority, onOpen, onDimensions, style, sizes }: {
  photo: PhotoResponse; featured: boolean; authorName: string; priority: boolean; onOpen: (id?: string) => void; style: CSSProperties; sizes: string
  onDimensions: (id: string, width: number, height: number) => void
}) {
  const { t } = useI18n()
  const title = photo.title || t.photography
  return (
    <div id={`photo-${photo.id}`} data-photo-id={photo.id} className="group relative" style={style}>
      <PhotoImage src={photoThumbnail(photo.id, 800)} srcSet={photoThumbnailSet(photo.id)} sizes={sizes} alt={title} width={photo.width || 1500} height={photo.height || 1000} priority={priority} onDimensions={photo.width > 0 && photo.height > 0 ? undefined : (width, height) => onDimensions(photo.id, width, height)} className="h-full w-full">
        <Link id={`photo-link-${photo.id}`} href={routes.photo(photo.id)} scroll={false} aria-label={title} onClick={(event) => { if (!event.metaKey && !event.ctrlKey && !event.shiftKey && !event.altKey && event.button === 0) onOpen(photo.id) }} className="absolute inset-0 block focus-visible:outline-offset-[-3px]">
          {featured ? <span className="absolute top-3 left-3 grid size-7 place-items-center bg-black/55 text-white" title={t.featured} aria-label={t.featured}><Award className="size-4" /></span> : null}
          <span className="photo-touch-caption pointer-events-none absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/75 to-transparent px-3 pt-10 pb-3 text-white opacity-0 transition-opacity duration-200 group-hover:opacity-100 group-focus-within:opacity-100">
            <span className="block truncate text-sm font-medium">{title}</span>
            <span className="mt-1 hidden items-center gap-1.5 text-xs text-white/80 sm:flex"><AuthorMark name={authorName} className="size-5 text-[10px]" />{authorName}</span>
          </span>
        </Link>
      </PhotoImage>
    </div>
  )
}

export function AuthorMark({ name, className }: { name: string; className?: string }) {
  return <span aria-hidden className={cn("grid size-7 shrink-0 place-items-center rounded-full bg-muted text-xs font-medium text-foreground", className)}>{Array.from(name.trim())[0] || "?"}</span>
}
