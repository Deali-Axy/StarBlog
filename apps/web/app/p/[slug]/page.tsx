import { redirect } from "next/navigation"

import { routes } from "@/lib/routes"

interface LegacyPostRedirectProps {
  params: Promise<{ slug: string }>
}

export default async function LegacyPostRedirect({ params }: LegacyPostRedirectProps) {
  const { slug } = await params
  redirect(routes.post(decodeURIComponent(slug)))
}
