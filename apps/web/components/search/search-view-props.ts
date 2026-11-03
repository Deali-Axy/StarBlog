import type { ReactNode } from "react"
import type { CategoryWordCloudItem, PublishedPostSummary, SearchResultResponse } from "@/lib/api/types"
import type { messages } from "@/lib/i18n"

export type SearchPageData = {
  keyword: string
  page: number
  results: SearchResultResponse[]
  searchFailed: boolean
  topics: CategoryWordCloudItem[]
  recent: PublishedPostSummary[]
  recentFailed: boolean
}

export type SearchViewProps = SearchPageData & {
  t: (typeof messages)["zh"]
  form: ReactNode
  pageHref: (page: number) => string
  keywordHref: (keyword: string) => string
}
