import type { Metadata } from "next"
import { Suspense } from "react"
import { SearchDesign } from "@/components/search/search-design"
import { getCategoryWordCloud, listPosts, searchPosts } from "@/lib/api/queries"
import { getI18n } from "@/lib/i18n-server"
import { pageMetadata, seoText } from "@/lib/seo"
import { routes } from "@/lib/routes"
import { parseSearchQuery, SEARCH_PAGE_SIZE as pageSize } from "@/lib/search-query"

export async function generateMetadata({ searchParams }: SearchPageProps): Promise<Metadata> {
  const { locale } = await getI18n()
  const { keyword, page } = parseSearchQuery(await searchParams)
  const text = seoText[locale]
  const title = `${keyword ? text.results(keyword) : text.search}${page > 1 ? ` · ${text.page(page)}` : ""}`
  return pageMetadata({ locale, title, description: text.searchDescription, path: routes.search, noindex: true })
}

interface SearchPageProps {
  searchParams: Promise<{ keyword?: string | string[]; page?: string | string[] }>
}

export default async function SearchPage({ searchParams }: SearchPageProps) {
  const { locale } = await getI18n()
  const { keyword, page } = parseSearchQuery(await searchParams)
  const [searchResponse, topicsResponse, recentResponse] = await Promise.allSettled([
    keyword ? searchPosts({ keyword, page, pageSize }, locale) : Promise.resolve([]),
    getCategoryWordCloud(),
    keyword ? Promise.resolve(null) : listPosts({ pageSize: 4, sortBy: "lastUpdateTime", sortType: "desc" }, locale),
  ])
  const results = searchResponse.status === "fulfilled" ? searchResponse.value : []
  const searchFailed = searchResponse.status === "rejected"
  const topics = topicsResponse.status === "fulfilled"
    ? topicsResponse.value.filter((topic) => topic.count > 0).sort((a, b) => b.count - a.count).slice(0, 8)
    : []
  const recent = recentResponse.status === "fulfilled" ? recentResponse.value?.items ?? [] : []

  return <Suspense><SearchDesign key={keyword} data={{ keyword, page, results, searchFailed, topics, recent, recentFailed: recentResponse.status === "rejected" }} /></Suspense>
}
