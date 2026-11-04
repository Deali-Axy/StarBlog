import type { Metadata } from "next"

import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"
import { eyebrowClassName, headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.notFound, robots: { index: false, follow: true } }
}

export default async function NotFound() {
  const { t } = await getI18n()
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-xl flex-col justify-center px-4 py-16">
      <p className={eyebrowClassName}>404</p>
      <h1 className={cn(headingClassName, "mt-3 text-3xl")}>{t.notFound}</h1>
      <p className="mt-4 text-muted-foreground">{t.notFoundHint}</p>
      <div className="mt-8">
        <Button asChild>
          <Link href={routes.blog}>{t.backToPosts}</Link>
        </Button>
      </div>
    </div>
  )
}
