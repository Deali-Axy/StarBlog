"use client"
import type { PhotoResponse } from "@/lib/api/types"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { ArrowUpRight, RefreshCw } from "lucide-react"
import Link from "next/link"
import { useHeroPhoto } from "../hero-photo-controller"
export function FullWidthHeroPhoto({ photo, onFailure }: {
  photo: PhotoResponse
  onFailure?: () => void
}) {
  const { imageRef, t, current, failed, setFailed, pending, location, hasLocation, swap } = useHeroPhoto({ photo, onFailure })
  return (<figure className="min-w-0 self-stretch md:mt-6">
    <div className="mb-3 flex items-baseline justify-between gap-4">
      <span className="text-xs tracking-widest"><span aria-hidden="true" className="pr-1.5 text-(--hero-ink)">↳</span> {t.photoNotebook}</span>
      <span className="text-xs text-muted-foreground">{t.photography}</span>
    </div>
    <Link href={routes.photo(current.id)} className="group/photo relative block overflow-hidden bg-muted" aria-label={`${t.viewLargePhoto}: ${current.title}`}>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img ref={imageRef} src={photoThumbnail(current.id, 1200)} srcSet={photoThumbnailSet(current.id)} sizes={"(min-width: 1440px) 1200px, 90vw"} alt={current.title} width={current.width} height={current.height} fetchPriority="high" onError={() => { setFailed(true); onFailure?.() }} className={cn("block aspect-5/4 h-auto w-full object-cover transition-[opacity,transform] duration-300 ease-in-out group-hover/photo:scale-105 md:aspect-3/2", "aspect-video! max-h-[65dvh]", failed && "hidden", pending && "opacity-70")} />
      {failed ? <span className="flex min-h-48 items-center justify-center p-6 text-sm text-muted-foreground">{t.photoLoadFailed}</span> : null}
      <span className="absolute right-0 bottom-0 grid size-11 place-items-center bg-background text-foreground"><ArrowUpRight aria-hidden="true" className="size-5" /></span>
    </Link>
    <figcaption className="flex items-baseline justify-between gap-4 border-b border-border py-3">
      <div aria-live="polite" aria-atomic="true" className="flex min-w-0 flex-wrap items-baseline gap-x-3 gap-y-1 text-xs">
        <span className="break-words">{current.title}</span>
        {hasLocation ? <span className="text-muted-foreground">/ {location}</span> : null}
      </div>
      <button type="button" className="inline-flex min-h-11 shrink-0 cursor-pointer items-center justify-center gap-2 text-xs hover:text-(--hero-ink) disabled:cursor-wait disabled:opacity-50" disabled={pending} aria-busy={pending} onClick={() => void swap()}>
        <RefreshCw aria-hidden="true" className={cn("size-3.5", pending && "motion-safe:animate-spin")} />{t.anotherPhoto}
      </button>
    </figcaption>
  </figure>)
}
