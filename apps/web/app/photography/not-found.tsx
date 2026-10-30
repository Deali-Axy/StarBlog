import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"

import { routes } from "@/lib/routes"
import { headingClassName, photoInsetClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export default async function PhotographyNotFound() {
  const { t } = await getI18n()
  return (
    <div className={cn(photoInsetClassName, "border-b border-border py-10")}>
      <p className="text-xs tracking-[0.16em] text-muted-foreground">404</p>
      <h1 className={cn(headingClassName, "mt-3 font-sans text-2xl font-medium tracking-normal text-foreground")}>{t.photoNotFound}</h1>
      <p className="mt-3 max-w-lg text-sm text-muted-foreground">{t.photoMissingHint}</p>
      <Link href={routes.photography} className="mt-6 inline-block text-sm underline-offset-4 hover:underline">
        {t.backToPhotos}
      </Link>
    </div>
  )
}
