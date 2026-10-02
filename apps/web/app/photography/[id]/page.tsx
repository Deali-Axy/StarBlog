import Link from "next/link"
import type { Metadata } from "next"
import { notFound } from "next/navigation"
import { Shuffle } from "lucide-react"

import { Button } from "@/components/ui/button"
import { ApiError } from "@/lib/api/types"
import { getAdjacentPhoto, getPhoto } from "@/lib/api/queries"
import { formatDate } from "@/lib/format"
import { photoOriginal, photoThumbnail } from "@/lib/media"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"

export const revalidate = 300

interface PhotoPageProps {
  params: Promise<{ id: string }>
}

export async function generateMetadata({ params }: PhotoPageProps): Promise<Metadata> {
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
    return { title: "图片不存在" }
  }
}

export default async function PhotoPage({ params }: PhotoPageProps) {
  const { id } = await params
  const photoId = decodeURIComponent(id)
  let photo
  try {
    photo = await getPhoto(photoId)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }

  const [previous, next] = await Promise.all([
    getAdjacentPhoto(photo.id, "previous"),
    getAdjacentPhoto(photo.id, "next"),
  ])

  return (
    <div className="bg-black text-[#f3ece0]">
      <div className="mx-auto w-full max-w-6xl px-4 py-8">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3 text-sm">
          <Link href={routes.photography} className="text-white/60 hover:text-white">
            返回摄影
          </Link>
          <div className="flex gap-2">
            {previous ? (
              <Button variant="ghost" size="sm" className="text-white/80 hover:text-white" asChild>
                <Link href={routes.photo(previous.id)}>上一张</Link>
              </Button>
            ) : (
              <Button variant="ghost" size="sm" disabled>
                上一张
              </Button>
            )}
            {next ? (
              <Button variant="ghost" size="sm" className="text-white/80 hover:text-white" asChild>
                <Link href={routes.photo(next.id)}>下一张</Link>
              </Button>
            ) : (
              <Button variant="ghost" size="sm" disabled>
                下一张
              </Button>
            )}
            <Button variant="ghost" size="sm" className="text-white/80 hover:text-white" asChild>
              <Link href={routes.photographyRandom}>
                <Shuffle />
                随机
              </Link>
            </Button>
          </div>
        </div>
        <figure>
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={photoOriginal(photo.filePath)}
            alt={photo.title}
            className="mx-auto max-h-[82vh] w-auto max-w-full object-contain"
          />
          <figcaption className="mt-6 flex flex-wrap items-baseline justify-between gap-3">
            <div>
              <h1 className="text-xl font-normal text-white">{photo.title}</h1>
              <p className="mt-1 text-sm text-white/55">
                {photo.location || "未知地点"}
                {" · "}
                {formatDate(photo.createTime, "YYYY-MM-DD HH:mm")}
              </p>
            </div>
          </figcaption>
        </figure>
      </div>
    </div>
  )
}
