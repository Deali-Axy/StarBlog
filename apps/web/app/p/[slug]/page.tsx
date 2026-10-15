import { redirect } from "next/navigation"

import { routes } from "@/lib/routes"

interface LegacyPostRedirectProps {
  params: Promise<{ slug: string }>
  searchParams: Promise<{ lang?: string }>
}

export default async function LegacyPostRedirect({ params, searchParams }: LegacyPostRedirectProps) {
  const { slug } = await params
  const { lang } = await searchParams
  redirect(`${routes.post(decodeURIComponent(slug))}${lang ? `?lang=${encodeURIComponent(lang)}` : ""}`)
}
