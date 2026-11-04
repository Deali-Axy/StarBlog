import { pageMetadata, seoText } from "@/lib/seo"
import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { locale } = await getI18n()
  const text = seoText[locale]
  return pageMetadata({ locale, title: text.photography, description: text.photographyDescription, path: "/photography" })
}

export default async function PhotographyPage() {
  const { t } = await getI18n()
  return <h1 className={cn(headingClassName, "sr-only")}>{t.photography}</h1>
}
