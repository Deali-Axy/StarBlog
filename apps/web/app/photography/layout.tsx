import { PhotoStudio } from "@/components/photography/photo-studio"
import { listFeaturedPhotos, listPhotos } from "@/lib/api/queries"
import { photoPageSize } from "@/lib/photography/page-size"

export const revalidate = 300

export default async function PhotographyLayout({ children }: { children: React.ReactNode }) {
  const [page, featured] = await Promise.all([
    listPhotos({ page: 1, pageSize: photoPageSize }),
    listFeaturedPhotos(),
  ])

  return (
    <PhotoStudio initialPage={page} featured={featured}>
      {children}
    </PhotoStudio>
  )
}
