import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"
import { notFound } from "next/navigation"

import { PhotoDetail } from "@/components/photography/photo-detail"
import { ApiError } from "@/lib/api/types"
import { getPhoto } from "@/lib/api/queries"
import { photoThumbnail } from "@/lib/media"
import { routes } from "@/lib/routes"
import { JsonLd } from "@/components/seo/json-ld"
import { pageMetadata } from "@/lib/seo"
import { absoluteUrl, getSite } from "@/lib/site"

export const revalidate = 300

interface PhotoPageProps {
  params: Promise<{ id: string }>
}

export async function generateMetadata({ params }: PhotoPageProps): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  try {
    const photo = await getPhoto(id)
    return pageMetadata({
      locale, title: photo.title || t.photography,
      description: photo.location || site.description,
      path: routes.photo(photo.id), image: photoThumbnail(photo.id, 1200),
    })
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
}

export default async function PhotoPage({ params }: PhotoPageProps) {
  const { id } = await params
  let photo
  try {
    photo = await getPhoto(id)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
  const { locale } = await getI18n()
  const site = getSite(locale)
  return <>
    <JsonLd data={{
      "@context": "https://schema.org", "@type": "ImageObject",
      name: photo.title, description: photo.location || undefined,
      url: absoluteUrl(routes.photo(photo.id)),
      contentUrl: absoluteUrl(photoThumbnail(photo.id, 1600)),
      datePublished: photo.createTime,
      creator: { "@type": "Person", name: site.authorName, url: absoluteUrl(routes.about) },
      ...(photo.width > 0 && photo.height > 0 ? { width: photo.width, height: photo.height } : {}),
    }} />
    <PhotoDetail photo={photo} />
  </>
}
