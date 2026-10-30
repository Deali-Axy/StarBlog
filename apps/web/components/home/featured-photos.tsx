"use client"

import Link from "next/link"
import { useState } from "react"
import { ArrowUpRight, Camera } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { useHomeDesign } from "@/components/home/home-design-context"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { PhotoResponse } from "@/lib/api/types"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

function PhotoPrint({ photo, index, single }: { photo: PhotoResponse; index: number; single: boolean }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  const desk = variant === "desk"
  const [failed, setFailed] = useState(false)
  const location = photo.location?.trim()
  const hasLocation = location && !/^(unknown|未知|未知地点)$/i.test(location)
  return <figure className={cn("min-w-0", desk && "-rotate-[1.5deg] bg-card px-3 pt-3 pb-4 shadow-[3px_5px_0_var(--muted)] even:rotate-2 nth-[3n]:rotate-[.75deg] sm:pb-5", single && "mx-auto w-full max-w-[38rem]")}>
    <Link href={routes.photo(photo.id)} className="group relative block bg-card" aria-label={`${t.viewLargePhoto}: ${photo.title}`}>
      {failed ? <span className="flex min-h-40 flex-col items-center justify-center gap-3 p-4 text-xs text-muted-foreground"><Camera size={24} aria-hidden="true" /><span>{t.photoLoadFailed}</span></span> : (
        // eslint-disable-next-line @next/next/no-img-element
        <img className={cn("block h-auto w-full object-contain", desk ? "max-h-[25rem]" : "max-h-[23rem]")} src={photoThumbnail(photo.id, 480)} srcSet={photoThumbnailSet(photo.id)}
          sizes={single ? "(min-width: 768px) 600px, 80vw" : "auto, (min-width: 640px) 220px, 80vw"}
          alt={photo.title} width={photo.width || undefined} height={photo.height || undefined} loading="lazy" onError={() => setFailed(true)} />
      )}
      <span className="absolute right-2 bottom-2 grid size-8 place-items-center bg-background text-foreground transition-transform duration-180 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 motion-reduce:transition-none"><ArrowUpRight size={18} aria-hidden="true" /></span>
    </Link>
    <figcaption className={cn("flex gap-2 text-xs leading-[1.7] sm:gap-3", desk ? "mt-4 px-1" : "mt-3.5")}><span className="pt-0.5 font-mono text-[11px] text-muted-foreground">{String(index + 1).padStart(2, "0")}</span><div className="min-w-0 wrap-anywhere"><Link className="hover:text-primary" href={routes.photo(photo.id)}>{photo.title}</Link>{hasLocation ? <p className="mt-1 text-[11px] text-muted-foreground">{location}</p> : null}</div></figcaption>
  </figure>
}

export function FeaturedPhotos({ photos }: { photos: PhotoResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (!photos.length) return null
  return <section className={homeSectionClassName} aria-labelledby="home-photos-title">
    <HomeSectionHeading id="home-photos-title" eyebrow={variant === "desk" ? t.homeOffScreen : t.homeContactSheet}
      title={variant === "desk" ? t.homeDeskPhotosTitle : t.homePhotosTitle} href={routes.photography} action={t.allPhotos} />
    <div className={variant === "desk" ? "px-3 pt-3 pb-5" : "bg-muted p-4 sm:p-6"}>
      <div className={cn("flex items-center justify-between gap-4 text-[11px] tracking-[.1em] text-muted-foreground", variant === "desk" ? "mb-7" : "mb-6")}><span>{t.featuredPhotos}</span><Camera size={14} aria-hidden="true" /></div>
      <div className={cn("grid items-center", variant === "desk" ? "grid-cols-[repeat(auto-fill,minmax(min(100%,11rem),1fr))] gap-6 px-2 sm:gap-x-5 sm:gap-y-8" : "grid-cols-[repeat(auto-fill,minmax(min(100%,10rem),1fr))] gap-x-4 gap-y-6 sm:gap-5", photos.length === 1 && "grid-cols-1")}>
        {photos.map((photo, index) => <PhotoPrint key={photo.id} photo={photo} index={index} single={photos.length === 1} />)}
      </div>
    </div>
  </section>
}
