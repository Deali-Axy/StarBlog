import Link from "next/link"
import type { Metadata } from "next"
import { notFound } from "next/navigation"
import { ArrowLeft, Shuffle } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-800">
      <div className="mx-auto grid w-full max-w-6xl gap-8 px-4 py-10 lg:grid-cols-[minmax(0,1fr)_280px]">
        <div>
          <div className="mb-4 flex items-center justify-between gap-3">
            <h1 className="text-2xl font-bold">{photo.title}</h1>
            <Button variant="outline" asChild>
              <Link href={routes.photography}>
                <ArrowLeft />
                返回列表
              </Link>
            </Button>
          </div>
          <div className="overflow-hidden rounded-2xl border border-border/70 bg-card shadow-lg">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={photoOriginal(photo.filePath)} alt={photo.title} className="w-full object-contain" />
          </div>
        </div>
        <aside className="space-y-4 lg:sticky lg:top-24 lg:self-start">
          <Card className="border-0 bg-white/80 py-4 shadow-sm dark:bg-slate-900/80">
            <CardContent className="space-y-2">
              <h2 className="text-lg font-semibold">{photo.title}</h2>
              <p className="text-sm text-muted-foreground">拍摄地点：{photo.location || "未知"}</p>
              <p className="text-sm text-muted-foreground">时间：{formatDate(photo.createTime, "YYYY-MM-DD HH:mm")}</p>
            </CardContent>
          </Card>
          <div className="grid gap-2">
            {previous ? (
              <Button variant="outline" asChild>
                <Link href={routes.photo(previous.id)}>上一张</Link>
              </Button>
            ) : (
              <Button variant="outline" disabled>
                上一张
              </Button>
            )}
            {next ? (
              <Button variant="outline" asChild>
                <Link href={routes.photo(next.id)}>下一张</Link>
              </Button>
            ) : (
              <Button variant="outline" disabled>
                下一张
              </Button>
            )}
            <Button variant="secondary" asChild>
              <Link href={routes.photographyRandom}>
                <Shuffle />
                随机推荐
              </Link>
            </Button>
          </div>
        </aside>
      </div>
    </div>
  )
}
