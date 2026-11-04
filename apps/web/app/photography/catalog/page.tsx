import type { Metadata } from "next"
import Link from "next/link"
import { notFound } from "next/navigation"
import { listPhotos } from "@/lib/api/queries"
import { getI18n } from "@/lib/i18n-server"
import { photoThumbnail } from "@/lib/media"
import { pageMetadata, seoText } from "@/lib/seo"
import { routes } from "@/lib/routes"

export const revalidate = 300
interface CatalogProps { searchParams: Promise<{ page?: string | string[] }> }

function pageNumber(value: string | string[] | undefined) {
  const number = typeof value === "string" ? Number(value) : 1
  return Number.isSafeInteger(number) && number > 0 && number <= 1_000_000 ? number : 1
}

function catalogHref(page: number) {
  return `/photography/catalog${page > 1 ? `?page=${page}` : ""}`
}

export async function generateMetadata({ searchParams }: CatalogProps): Promise<Metadata> {
  const { locale } = await getI18n()
  const page = pageNumber((await searchParams).page)
  const text = seoText[locale]
  const title = `${locale === "en" ? "Photo catalog" : "摄影目录"}${page > 1 ? ` · ${text.page(page)}` : ""}`
  return pageMetadata({ locale, title, description: text.photographyDescription, path: catalogHref(page) })
}

/** A paginated, server-rendered alternative to the interactive photo wall. */
export default async function PhotoCatalog({ searchParams }: CatalogProps) {
  const { locale, t } = await getI18n()
  const page = pageNumber((await searchParams).page)
  const photos = await listPhotos({ page, pageSize: 48 })
  if (page > Math.max(1, photos.totalPages)) notFound()
  return (
    <section className="mx-auto max-w-7xl px-4 py-10 sm:px-8">
      <Link href={routes.photography} className="text-sm text-primary underline underline-offset-4">{t.photography}</Link>
      <h1 className="mt-6 text-3xl font-medium">{locale === "en" ? "Photo catalog" : "摄影目录"}</h1>
      <p className="mt-3 text-sm text-muted-foreground">{t.photoCount(photos.totalCount)} · {t.pageOf(page, Math.max(1, photos.totalPages))}</p>
      <ul className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-4">
        {photos.items.map((photo) => <li key={photo.id}>
          <Link href={routes.photo(photo.id)} className="group block rounded-lg focus-visible:outline-2 focus-visible:outline-offset-4">
            {/* API thumbnails are already resized and cached; avoid a second image proxy. */}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={photoThumbnail(photo.id, 640)} alt={photo.title || t.photography} width={photo.width || 1500} height={photo.height || 1000} loading="lazy" className="aspect-[4/3] w-full rounded-lg bg-muted object-cover" />
            <h2 className="mt-3 break-words text-sm font-medium group-hover:text-primary">{photo.title || t.photography}</h2>
            {photo.location ? <p className="mt-1 break-words text-xs text-muted-foreground">{photo.location}</p> : null}
          </Link>
        </li>)}
      </ul>
      {!photos.items.length ? <p className="py-16 text-center text-muted-foreground">{t.noPhotos}</p> : null}
      <nav aria-label={locale === "en" ? "Photo catalog pages" : "摄影目录分页"} className="mt-10 flex items-center justify-center gap-6">
        {page > 1 ? <Link href={catalogHref(page - 1)} rel="prev" className="min-h-11 py-3 text-primary underline underline-offset-4">{t.previousPage}</Link> : null}
        <span className="text-sm">{t.pageOf(page, Math.max(1, photos.totalPages))}</span>
        {page < photos.totalPages ? <Link href={catalogHref(page + 1)} rel="next" className="min-h-11 py-3 text-primary underline underline-offset-4">{t.nextPage}</Link> : null}
      </nav>
    </section>
  )
}
