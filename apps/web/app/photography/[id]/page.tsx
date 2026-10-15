import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"
import { notFound } from "next/navigation"

import { PhotoDetail } from "@/components/photography/photo-detail"
import { ApiError } from "@/lib/api/types"
import { getPhoto } from "@/lib/api/queries"
import { photoThumbnail } from "@/lib/media"
import { routes } from "@/lib/routes"
import { getSite } from "@/lib/site"

export const revalidate = 300

interface PhotoPageProps {
  params: Promise<{ id: string }>
}

export async function generateMetadata({ params }: PhotoPageProps): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  try {
    const photo = await getPhoto(decodeURIComponent(id))
    return {
      title: photo.title,
      description: photo.location || site.description,
      alternates: { canonical: routes.photo(photo.id) },
      openGraph: {
        title: photo.title,
        description: photo.location || undefined,
        type: "article",
        url: routes.photo(photo.id),
        images: [{ url: photoThumbnail(photo.id, 1200), alt: photo.title }],
      },
      twitter: {
        card: "summary_large_image",
        title: photo.title,
        images: [photoThumbnail(photo.id, 1200)],
      },
    }
  } catch {
    return { title: t.photoNotFound }
  }
}

export default async function PhotoPage({ params }: PhotoPageProps) {
  const { id } = await params
  let photo
  try {
    photo = await getPhoto(decodeURIComponent(id))
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
  return <PhotoDetail photo={photo} />
}
