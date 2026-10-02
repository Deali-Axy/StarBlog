import Link from "next/link"
import type { Metadata } from "next"
import { Camera } from "lucide-react"

import { Button } from "@/components/ui/button"
import { listPhotos } from "@/lib/api/queries"
import { formatDate } from "@/lib/format"
import { photoThumbnail } from "@/lib/media"
import { routes } from "@/lib/routes"

export const revalidate = 300

export const metadata: Metadata = {
  title: "摄影",
}

interface PhotographyPageProps {
  searchParams: Promise<{ page?: string; pageSize?: string }>
}

export default async function PhotographyPage({ searchParams }: PhotographyPageProps) {
  const params = await searchParams
  const page = Math.max(1, Number(params.page) || 1)
  const pageSize = Math.min(48, Math.max(1, Number(params.pageSize) || 12))
  const photos = await listPhotos({ page, pageSize })

  return (
    <div className="page-wrap py-12">
      <div className="mb-10 flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="kicker">暗房</p>
          <h1 className="mt-2 text-3xl">摄影</h1>
          <p className="mt-2 text-sm text-muted-foreground">{photos.totalCount} 张</p>
        </div>
        <Button variant="outline" asChild>
          <Link href={routes.photographyRandom}>随机一张</Link>
        </Button>
      </div>

      {photos.items.length === 0 ? (
        <div className="border-y border-dashed border-border py-20 text-center text-muted-foreground">
          <Camera className="mx-auto mb-4 size-8 opacity-50" />
          暗房里还没有照片。
          <div className="mt-4">
            <Button variant="outline" asChild>
              <Link href={routes.home}>返回首页</Link>
            </Button>
          </div>
        </div>
      ) : (
        <div className="columns-1 gap-6 sm:columns-2 lg:columns-3 xl:columns-4">
          {photos.items.map((photo) => (
            <Link key={photo.id} href={routes.photo(photo.id)} className="mb-6 block break-inside-avoid">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={photoThumbnail(photo.id, 720)}
                alt={photo.title}
                className="w-full object-cover"
                style={
                  photo.width > 0 && photo.height > 0
                    ? { aspectRatio: `${photo.width} / ${photo.height}` }
                    : undefined
                }
              />
              <p className="mt-2 text-sm">{photo.title}</p>
              <p className="text-xs text-muted-foreground">
                {photo.location}
                {photo.location ? " · " : ""}
                {formatDate(photo.createTime)}
              </p>
            </Link>
          ))}
        </div>
      )}

      {photos.totalPages > 1 ? (
        <nav className="mt-10 flex items-center justify-center gap-2">
          {page > 1 ? (
            <Button variant="outline" asChild>
              <Link href={routes.photographyList({ page: page - 1, pageSize })}>上一页</Link>
            </Button>
          ) : null}
          <span className="px-3 text-sm text-muted-foreground">
            {photos.page} / {photos.totalPages}
          </span>
          {page < photos.totalPages ? (
            <Button variant="outline" asChild>
              <Link href={routes.photographyList({ page: page + 1, pageSize })}>下一页</Link>
            </Button>
          ) : null}
        </nav>
      ) : null}
    </div>
  )
}
