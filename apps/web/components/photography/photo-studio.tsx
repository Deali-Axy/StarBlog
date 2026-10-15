"use client"

import { useI18n } from "@/components/app-providers"
import { Camera } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import {
  createContext,
  useCallback,
  useContext,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
  type CSSProperties,
  type MouseEvent,
  type ReactNode,
} from "react"

import type { PageResult, PhotoResponse } from "@/lib/api/types"
import { getRandomPhoto, listPhotos } from "@/lib/api/queries"
import { photoThumbnail } from "@/lib/media"
import { layoutJustified, photoAspect, targetRowHeight } from "@/lib/photography/justify"
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
  hasMore: boolean
  loadMore: () => Promise<PhotoResponse[]>
  rememberWall: () => void
  leavePhoto: (event: { preventDefault(): void }) => void
  lightbox: boolean
  setLightbox: (open: boolean) => void
  authorName: string
  retainScroll: () => () => void
}

const PhotoStudioContext = createContext<PhotoStudioContextValue | null>(null)

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

function useLockBody(locked: boolean) {
  useLayoutEffect(() => {
    if (!locked) return
    const scrollY = window.scrollY
    const body = document.body
    const previous = {
      position: body.style.position,
      top: body.style.top,
      left: body.style.left,
      right: body.style.right,
      width: body.style.width,
      overflow: body.style.overflow,
    }
    body.style.position = "fixed"
    body.style.top = `-${scrollY}px`
    body.style.left = "0"
    body.style.right = "0"
    body.style.width = "100%"
    body.style.overflow = "hidden"
    return () => {
      body.style.position = previous.position
      body.style.top = previous.top
      body.style.left = previous.left
      body.style.right = previous.right
      body.style.width = previous.width
      body.style.overflow = previous.overflow
      window.scrollTo(0, scrollY)
    }
  }, [locked])
}

export function PhotoStudio({
  initialPage,
  featured: featuredPhotos,
  children,
}: {
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
  const [scrollLocked, setScrollLocked] = useState(false)
  const paging = useRef({ page: initialPage.page, hasMore: initialPage.page < initialPage.totalPages })
  const loading = useRef(false)
  const fromWall = useRef(false)
  const scrollHolders = useRef(0)

  const featured = useMemo(
    () => [...featuredPhotos].sort((a, b) => Date.parse(b.createTime) - Date.parse(a.createTime)),
    [featuredPhotos],
  )
  const featuredIds = useMemo(() => new Set(featured.map((photo) => photo.id)), [featured])

  useLockBody(scrollLocked)

  const retainScroll = useCallback(() => {
    scrollHolders.current += 1
    setScrollLocked(true)
    return () => {
      scrollHolders.current -= 1
      // The next photo mounts in the same commit. Wait so a handoff does not unlock the page.
      queueMicrotask(() => {
        if (scrollHolders.current !== 0) return
        setScrollLocked(false)
        setLightbox(false)
      })
    }
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

  const rememberWall = useCallback(() => {
    fromWall.current = true
  }, [])

  const leavePhoto = useCallback(
    (event: { preventDefault(): void }) => {
      setLightbox(false)
      if (!fromWall.current) return
      event.preventDefault()
      fromWall.current = false
      router.back()
    },
    [router],
  )

  async function openRandom() {
    if (openingRandom) return
    setOpeningRandom(true)
    try {
      const photo = await getRandomPhoto()
      rememberWall()
      router.push(routes.photo(photo.id), { scroll: false })
    } catch {
      // Stay on the wall. The random route still handles an empty library.
    } finally {
      setOpeningRandom(false)
    }
  }

  const value = useMemo<PhotoStudioContextValue>(
    () => ({
      photos,
      featured,
      featuredIds,
      filter,
      hasMore,
      loadMore,
      rememberWall,
      leavePhoto,
      lightbox,
      setLightbox,
      authorName: site.authorName,
      retainScroll,
    }),
    [photos, featured, featuredIds, filter, hasMore, loadMore, rememberWall, leavePhoto, lightbox, retainScroll],
  )

  return (
    <PhotoStudioContext.Provider value={value}>
      <div className="scheme-dark min-h-dvh bg-black text-white">
        {children}
        <div inert={scrollLocked || undefined}>
          <div className="flex flex-wrap items-center justify-between gap-3 px-3 py-3 sm:px-4">
            <div className="flex items-center gap-1.5">
              <FilterPill active={filter === "all"} onClick={() => setFilter("all")}>
                {t.all}
              </FilterPill>
              <FilterPill active={filter === "featured"} onClick={() => setFilter("featured")}>
                {t.featured}
              </FilterPill>
            </div>
            <div className="flex items-center gap-3 text-sm text-white/55">
              <span>{t.photoCount(filter === "featured" ? featured.length : totalCount)}</span>
              {totalCount > 0 ? (
                <button
                  type="button"
                  onClick={() => void openRandom()}
                  disabled={openingRandom}
                  className="rounded-full px-3 py-1 text-white/80 transition hover:bg-white/10 hover:text-white disabled:opacity-50"
                >
                  {t.randomPhoto}
                </button>
              ) : null}
            </div>
          </div>
          <PhotoWall
            photos={filter === "featured" ? featured : photos}
            featuredIds={featuredIds}
            authorName={site.authorName}
            onOpen={rememberWall}
            empty={filter === "featured" ? t.noPhotos : t.noPhotos}
          />
          {filter === "all" && hasMore ? (
            <LoadMore
              loading={loadingMore}
              failed={loadError}
              onLoad={() => void loadMore()}
              onVisible={() => void loadMore()}
            />
          ) : null}
        </div>
      </div>
    </PhotoStudioContext.Provider>
  )
}

function FilterPill({
  active,
  onClick,
  children,
}: {
  active: boolean
  onClick: () => void
  children: ReactNode
}) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onClick}
      className={cn(
        "rounded-full px-3.5 py-1.5 text-sm transition",
        active ? "bg-white text-black" : "text-white/70 hover:bg-white/10 hover:text-white",
      )}
    >
      {children}
    </button>
  )
}

function PhotoWall({
  photos,
  featuredIds,
  authorName,
  onOpen,
  empty,
}: {
  photos: PhotoResponse[]
  featuredIds: ReadonlySet<string>
  authorName: string
  onOpen: () => void
  empty: string
}) {
  const frameRef = useRef<HTMLDivElement>(null)
  const [width, setWidth] = useState(0)
  const { t } = useI18n()

  useLayoutEffect(() => {
    const frame = frameRef.current
    if (!frame) return
    const update = () => {
      const next = frame.clientWidth
      setWidth((current) => (current === next ? current : next))
    }
    update()
    const observer = new ResizeObserver(update)
    observer.observe(frame)
    return () => observer.disconnect()
  }, [photos.length])

  if (photos.length === 0) {
    return (
      <div className="px-4 py-24 text-center text-sm text-white/55">
        <Camera className="mx-auto mb-4 size-7 opacity-60" />
        <p>{empty}</p>
        <Link href={routes.home} className="mt-4 inline-block text-white/80 underline-offset-4 hover:underline">
          {t.backHome}
        </Link>
      </div>
    )
  }

  const aspects = photos.map((photo) => photoAspect(photo.width, photo.height))
  const layout = width > 0 ? layoutJustified(aspects, width, targetRowHeight(width), 8) : null

  return (
    <div className="px-2 pb-10 sm:px-3">
      <div ref={frameRef}>
        {layout ? (
          <div className="relative" style={{ height: layout.height }}>
            {layout.boxes.map((box) => {
              const photo = photos[box.index]
              if (!photo) return null
              return (
                <PhotoTile
                  key={photo.id}
                  photo={photo}
                  featured={featuredIds.has(photo.id)}
                  authorName={authorName}
                  priority={box.index < 8}
                  onOpen={onOpen}
                  className="absolute"
                  style={{ left: box.x, top: box.y, width: box.width, height: box.height }}
                />
              )
            })}
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
            {photos.map((photo, index) => (
              <PhotoTile
                key={photo.id}
                photo={photo}
                featured={featuredIds.has(photo.id)}
                authorName={authorName}
                priority={index < 8}
                onOpen={onOpen}
                style={{ aspectRatio: String(aspects[index]) }}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function PhotoTile({
  photo,
  featured,
  authorName,
  priority,
  onOpen,
  className,
  style,
}: {
  photo: PhotoResponse
  featured: boolean
  authorName: string
  priority: boolean
  onOpen: () => void
  className?: string
  style?: CSSProperties
}) {
  const { t } = useI18n()

  function onClick(event: MouseEvent<HTMLAnchorElement>) {
    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || event.button !== 0) return
    onOpen()
  }

  return (
    <Link
      href={routes.photo(photo.id)}
      scroll={false}
      onClick={onClick}
      style={style}
      className={cn(
        "group relative block overflow-hidden rounded-[10px] bg-neutral-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white",
        className,
      )}
    >
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src={photoThumbnail(photo.id, 800)}
        alt={photo.title || t.photography}
        loading={priority ? "eager" : "lazy"}
        className="absolute inset-0 h-full w-full object-cover"
      />
      <span
        aria-hidden
        className="pointer-events-none absolute inset-0 opacity-0 transition-opacity duration-200 group-hover:opacity-100 group-focus-visible:opacity-100"
      >
        <span className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/15 to-black/25" />
        {featured ? (
          <span className="absolute top-2.5 left-2.5 grid size-8 place-items-center rounded-lg bg-black/80 text-white">
            <AwardIcon />
          </span>
        ) : null}
        <span className="absolute inset-x-0 bottom-0 flex items-center gap-2 px-3 py-2.5">
          <AuthorMark name={authorName} />
          <span className="min-w-0 truncate text-sm font-medium text-white">{authorName}</span>
          <span className="ml-auto max-w-[46%] truncate text-right text-xs text-white/75">{photo.title}</span>
        </span>
      </span>
    </Link>
  )
}

function LoadMore({
  loading,
  failed,
  onLoad,
  onVisible,
}: {
  loading: boolean
  failed: boolean
  onLoad: () => void
  onVisible: () => void
}) {
  const { t } = useI18n()
  const ref = useRef<HTMLDivElement>(null)

  useLayoutEffect(() => {
    if (failed) return
    const el = ref.current
    if (!el) return
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) onVisible()
      },
      { rootMargin: "600px" },
    )
    observer.observe(el)
    return () => observer.disconnect()
  }, [failed, onVisible])

  return (
    <div ref={ref} className="flex justify-center px-4 pb-12">
      <button
        type="button"
        onClick={onLoad}
        disabled={loading}
        className="rounded-full px-4 py-2 text-sm text-white/70 transition hover:bg-white/10 hover:text-white disabled:opacity-60"
      >
        {loading ? t.loadingMore : failed ? t.retry : t.loadMore}
      </button>
    </div>
  )
}

export function AuthorMark({ name, className }: { name: string; className?: string }) {
  const letter = Array.from(name.trim())[0] || "?"
  return (
    <span
      aria-hidden
      className={cn(
        "grid size-7 shrink-0 place-items-center rounded-full bg-[#4a4036] text-xs font-medium text-white",
        className,
      )}
    >
      {letter}
    </span>
  )
}

function AwardIcon() {
  return (
    <svg viewBox="0 0 24 24" className="size-4" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <circle cx="12" cy="9" r="5.25" />
      <path d="M8.2 13.2 7 21l5-2.4L17 21l-1.2-7.8" strokeLinejoin="round" />
    </svg>
  )
}
