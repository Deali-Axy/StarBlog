import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.photography }
}

export default async function PhotographyPage() {
  const { t } = await getI18n()
  return <h1 className="sr-only">{t.photography}</h1>
}
