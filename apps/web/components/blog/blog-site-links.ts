"use client"
import { useI18n } from "@/components/app-providers"
import { routes } from "@/lib/routes"
export function useBlogSiteLinks() {
  const { t } = useI18n()
  return [
    { href: routes.home, label: t.home },
    { href: routes.blog, label: t.posts },
    { href: routes.photography, label: t.photography },
    { href: routes.links, label: t.links },
    { href: routes.about, label: t.about },
  ]
}
