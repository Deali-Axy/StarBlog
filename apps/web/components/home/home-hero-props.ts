import type { PhotoResponse, PublishedPostSummary, SiteOverviewResponse } from "@/lib/api/types"
import type { Locale } from "@/lib/i18n"
import type { getSite } from "@/lib/site"
export type HomeHeroProps = {
  locale: Locale
  site: ReturnType<typeof getSite>
  overview: SiteOverviewResponse | null
  photo?: PhotoResponse | null
  topPost?: PublishedPostSummary | null
}
