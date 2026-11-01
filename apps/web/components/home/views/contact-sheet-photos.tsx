"use client"
import { useI18n } from "@/components/app-providers"
import { HomeSectionHeading } from "@/components/home/section-heading"
import type { PhotoResponse } from "@/lib/api/types"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { routes } from "@/lib/routes"
import { homeSectionClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"
import { ArrowUpRight, Camera } from "lucide-react"
import Link from "next/link"
import { useHomePhotoState } from "../home-photo-state"
function ContactSheetPhotosPrint({ photo, index, single }: {
  photo: PhotoResponse
  index: number
  single: boolean
}) {
  const { t } = useI18n()
  const { failed, setFailed, location, hasLocation } = useHomePhotoState(photo)
  return <figure className={(cn("min-w-0", single && "mx-auto w-full max-w-[38rem]"))}>
    <Link href={routes.photo(photo.id)} className="group relative block bg-card" aria-label={`${t.viewLargePhoto}: ${photo.title}`}>
      {failed ? <span className="flex min-h-40 flex-col items-center justify-center gap-3 p-4 text-xs text-muted-foreground"><Camera size={24} aria-hidden="true" /><span>{t.photoLoadFailed}</span></span> : (
        // eslint-disable-next-line @next/next/no-img-element
        <img className={"block h-auto w-full object-contain max-h-[23rem]"} src={photoThumbnail(photo.id, 480)} srcSet={photoThumbnailSet(photo.id)} sizes={single ? "(min-width: 768px) 600px, 80vw" : "auto, (min-width: 640px) 220px, 80vw"} alt={photo.title} width={photo.width || undefined} height={photo.height || undefined} loading="lazy" onError={() => setFailed(true)} />)}
      <span className="absolute right-2 bottom-2 grid size-8 place-items-center bg-background text-foreground transition-transform duration-180 group-hover:translate-x-0.5 group-hover:-translate-y-0.5 motion-reduce:transition-none"><ArrowUpRight size={18} aria-hidden="true" /></span>
    </Link>
    <figcaption className={"flex gap-2 text-xs leading-[1.7] sm:gap-3 mt-3.5"}><span className="pt-0.5 font-mono text-[11px] text-muted-foreground">{String(index + 1).padStart(2, "0")}</span><div className="min-w-0 wrap-anywhere"><Link className="hover:text-primary" href={routes.photo(photo.id)}>{photo.title}</Link>{hasLocation ? <p style={{ zoom: "var(--home-scale)" }} className="mt-1 text-[11px] text-muted-foreground">{location}</p> : null}</div></figcaption>
  </figure>
}
export function ContactSheetPhotos({ photos }: {
  photos: PhotoResponse[]
}) {
  const { t } = useI18n()
  if(!photos.length)
    return null
  return <section data-home-section="photos" data-home-variant={"contactSheet"} className={homeSectionClassName} aria-labelledby="home-photos-title">
    <HomeSectionHeading id="home-photos-title" eyebrow={t.homeContactSheet} title={t.homePhotosTitle} href={routes.photography} action={t.allPhotos} />
    <div className={"bg-muted p-4 sm:p-6"}>
      <div className={"flex items-center justify-between gap-4 text-[11px] tracking-[.1em] text-muted-foreground mb-6"}><span>{t.featuredPhotos}</span><Camera size={14} aria-hidden="true" /></div>
      <div className={cn("grid items-center", "grid-cols-[repeat(auto-fill,minmax(min(100%,10rem),1fr))] gap-x-4 gap-y-6 sm:gap-5", photos.length === 1 && "grid-cols-1")}>
        {photos.map((photo, index) => <ContactSheetPhotosPrint key={photo.id} photo={photo} index={index} single={photos.length === 1} />)}
      </div>
    </div>
  </section>
}
