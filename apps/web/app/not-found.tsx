import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

export default async function NotFound() {
  const { t } = await getI18n()
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-xl flex-col justify-center px-4 py-16">
      <p className="kicker">404</p>
      <h1 className="mt-3 text-3xl">{t.notFound}</h1>
      <p className="mt-4 text-muted-foreground">{t.notFoundHint}</p>
      <div className="mt-8">
        <Button asChild>
          <Link href={routes.blog}>{t.backToPosts}</Link>
        </Button>
      </div>
    </div>
  )
}
