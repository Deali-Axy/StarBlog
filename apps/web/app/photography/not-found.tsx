import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"

import { routes } from "@/lib/routes"

export default async function PhotographyNotFound() {
  const { t } = await getI18n()
  return (
    <div className="border-b border-white/10 px-4 py-10 sm:px-6">
      <p className="text-xs tracking-[0.16em] text-white/45">404</p>
      <h1 className="mt-3 font-sans text-2xl font-medium tracking-normal text-white">{t.photoNotFound}</h1>
      <p className="mt-3 max-w-lg text-sm text-white/60">{t.photoMissingHint}</p>
      <Link href={routes.photography} className="mt-6 inline-block text-sm text-white/80 underline-offset-4 hover:underline">
        {t.backToPhotos}
      </Link>
    </div>
  )
}
