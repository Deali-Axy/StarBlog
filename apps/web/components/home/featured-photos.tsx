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
import styles from "./home-sections.module.css"

function PhotoPrint({ photo, index }: { photo: PhotoResponse; index: number }) {
  const { t } = useI18n()
  const [failed, setFailed] = useState(false)
  const location = photo.location?.trim()
  const hasLocation = location && !/^(unknown|未知|未知地点)$/i.test(location)
  return <figure className={styles.photoPrint}>
    <Link href={routes.photo(photo.id)} className={styles.photoLink} aria-label={`${t.viewLargePhoto}: ${photo.title}`}>
      {failed ? <span className={styles.photoFallback}><Camera size={24} aria-hidden="true" /><span>{t.photoLoadFailed}</span></span> : (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={photoThumbnail(photo.id, 800)} srcSet={photoThumbnailSet(photo.id)} sizes="(min-width: 1120px) 320px, (min-width: 640px) 30vw, 80vw"
          alt={photo.title} width={photo.width || undefined} height={photo.height || undefined} loading="lazy" onError={() => setFailed(true)} />
      )}
      <span className={styles.photoArrow}><ArrowUpRight size={18} aria-hidden="true" /></span>
    </Link>
    <figcaption><span className={styles.frameNumber}>{String(index + 1).padStart(2, "0")}</span><div><Link href={routes.photo(photo.id)}>{photo.title}</Link>{hasLocation ? <p>{location}</p> : null}</div></figcaption>
  </figure>
}

export function FeaturedPhotos({ photos }: { photos: PhotoResponse[] }) {
  const { t } = useI18n()
  const { variant } = useHomeDesign()
  if (!photos.length) return null
  return <section className={styles.section} aria-labelledby="home-photos-title">
    <HomeSectionHeading id="home-photos-title" eyebrow={variant === "desk" ? t.homeOffScreen : t.homeContactSheet}
      title={variant === "desk" ? t.homeDeskPhotosTitle : t.homePhotosTitle} href={routes.photography} action={t.allPhotos} />
    <div className={styles.photoSheet}>
      <div className={styles.sheetRunning}><span>{t.featuredPhotos}</span><Camera size={14} aria-hidden="true" /></div>
      <div className={styles.photoGrid} data-single={photos.length === 1 || undefined}>
        {photos.slice(0, 4).map((photo, index) => <PhotoPrint key={photo.id} photo={photo} index={index} />)}
      </div>
    </div>
  </section>
}
