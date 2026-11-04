import { getCategoryTree, listPhotos, listPosts } from "@/lib/api/queries"
import type { CategoryNodeResponse, PageResult } from "@/lib/api/types"
import { postHref, routes } from "@/lib/routes"
import { absoluteUrl } from "@/lib/site"

const chunkSize = 1000
const apiPageSize = 100
const header = '<?xml version="1.0" encoding="UTF-8"?>'
export function xmlEscape(value: string) {
  return value.replace(/[<>&"']/g, (char) => ({ "<": "&lt;", ">": "&gt;", "&": "&amp;", '"': "&quot;", "'": "&apos;" })[char]!)
}
function loc(path: string) { return `<loc>${xmlEscape(absoluteUrl(path))}</loc>` }
function sitemapPath(kind: string, page: number) { return `/sitemaps/${kind}/${page}.xml` }
function index(paths: string[]) {
  return `${header}<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">${paths.map((path) => `<sitemap>${loc(path)}</sitemap>`).join("")}</sitemapindex>`
}
function chunks(total: number, kind: string) {
  return Array.from({ length: Math.ceil(total / chunkSize) }, (_, i) => sitemapPath(kind, i + 1))
}

export async function sitemapIndex(imagesOnly = false) {
  const photos = await listPhotos({ page: 1, pageSize: apiPageSize })
  const imagePaths = chunks(photos.totalCount, "images")
  if (imagesOnly) return index(imagePaths)
  const posts = await listPosts({ page: 1, pageSize: apiPageSize })
  return index([sitemapPath("pages", 1), ...chunks(posts.totalCount, "posts"), ...imagePaths])
}

function flattenCategories(nodes: CategoryNodeResponse[]): CategoryNodeResponse[] {
  return nodes.flatMap((node) => [node, ...flattenCategories(node.nodes ?? [])])
}
function urlset(urls: string[], images = false) {
  return `${header}<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"${images ? ' xmlns:image="http://www.google.com/schemas/sitemap-image/1.1"' : ""}>${urls.join("")}</urlset>`
}
function url(path: string, modified?: string) {
  const date = modified && Number.isFinite(Date.parse(modified)) ? `<lastmod>${new Date(modified).toISOString()}</lastmod>` : ""
  return `<url>${loc(path)}${date}</url>`
}

async function chunk<T>(page: number, getPage: (page: number) => Promise<PageResult<T>>) {
  const firstPage = (page - 1) * (chunkSize / apiPageSize) + 1
  const first = await getPage(firstPage)
  if (firstPage > Math.max(1, first.totalPages) || !first.totalCount) return null
  const items = [...first.items]
  // Bounded concurrency; do not request the entire content database at once.
  for (let start = firstPage + 1; start <= Math.min(first.totalPages, firstPage + 9); start += 3) {
    const pages = await Promise.all(Array.from({ length: Math.min(3, first.totalPages - start + 1, firstPage + 10 - start) }, (_, i) => getPage(start + i)))
    items.push(...pages.flatMap((result) => result.items))
  }
  return items
}

export async function sitemapChunk(kind: string, page: number): Promise<string | null> {
  if (!Number.isSafeInteger(page) || page < 1 || page > 1_000_000) return null
  if (kind === "pages") {
    if (page !== 1) return null
    const [categories, photos] = await Promise.all([getCategoryTree(), listPhotos({ page: 1, pageSize: 48 })])
    const paths = [routes.home, routes.blog, routes.photography, routes.about, routes.links, routes.rss, "/photography/catalog",
      ...flattenCategories(categories).filter((node) => Number(node.tags?.[0]) > 0).map((node) => routes.blogList({ categoryId: node.id })),
      ...Array.from({ length: Math.max(0, photos.totalPages - 1) }, (_, i) => `/photography/catalog?page=${i + 2}`),
    ]
    return urlset([...new Set(paths)].map((path) => url(path)))
  }
  if (kind === "posts") {
    const posts = await chunk(page, (number) => listPosts({ page: number, pageSize: apiPageSize }))
    return posts ? urlset(posts.filter((post) => post.isPublish).map((post) => url(postHref(post), post.lastUpdateTime))) : null
  }
  if (kind === "images") {
    const photos = await chunk(page, (number) => listPhotos({ page: number, pageSize: apiPageSize }))
    return photos ? urlset(photos.map((photo) => {
      const filePath = photo.filePath.replaceAll("\\", "/").replace(/^\/+/, "").split("/").map(encodeURIComponent).join("/")
      return `<url>${loc(routes.photo(photo.id))}<image:image><image:loc>${xmlEscape(absoluteUrl(`/media/photography/${filePath}`))}</image:loc><image:title>${xmlEscape(photo.title)}</image:title></image:image></url>`
    }), true) : null
  }
  return null
}

export function xmlResponse(xml: string) {
  return new Response(xml, { headers: { "Content-Type": "application/xml; charset=utf-8", "Cache-Control": "public, max-age=0, s-maxage=300" } })
}
