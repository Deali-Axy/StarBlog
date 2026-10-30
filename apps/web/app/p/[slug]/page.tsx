import { redirect } from "next/navigation"

import { blogStyleHref, resolveBlogVariant } from "@/lib/blog-variant"
import { routes } from "@/lib/routes"

interface LegacyPostRedirectProps {
  params: Promise<{ slug: string }>
  searchParams: Promise<{ lang?: string; bv?: string }>
}

export default async function LegacyPostRedirect({ params, searchParams }: LegacyPostRedirectProps) {
  const { slug } = await params
  const { lang, bv } = await searchParams
  const href = `${routes.post(decodeURIComponent(slug))}${lang ? `?lang=${encodeURIComponent(lang)}` : ""}`
  redirect(bv ? blogStyleHref(href, resolveBlogVariant(bv)) : href)
}
