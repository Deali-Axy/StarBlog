import { PhotoStudio } from "@/components/photography/photo-studio"
import { PhotographyUnavailable } from "@/components/photography/photography-unavailable"
import { listFeaturedPhotos, listPhotos } from "@/lib/api/queries"
import { photoPageSize } from "@/lib/photography/page-size"

export const revalidate = 300

export default async function PhotographyLayout({ children }: { children: React.ReactNode }) {
  const result = await Promise.all([
    listPhotos({ page: 1, pageSize: photoPageSize }),
    listFeaturedPhotos(),
  ]).catch(() => null)
  if (!result) return <PhotographyUnavailable />
  const [page, featured] = result

  return (
    <PhotoStudio initialPage={page} featured={featured}>
      {children}
    </PhotoStudio>
  )
}
