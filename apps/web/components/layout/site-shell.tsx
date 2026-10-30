"use client"

import { usePathname } from "next/navigation"
import type { ReactNode } from "react"

import { PhotographyFooter, PhotographyHeader } from "@/components/photography/photography-chrome"
import { BlogLayout } from "@/components/blog/blog-layout"
import { SiteFooter } from "@/components/layout/site-footer"
import { SiteHeader } from "@/components/layout/site-header"
import { isPhotographyPath } from "@/lib/photography/path"
import { isBlogPath } from "@/lib/blog-variant"
import { photographyThemeClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function SiteShell({ children }: { children: ReactNode }) {
  const pathname = usePathname()
  const photography = isPhotographyPath(pathname)
  if (isBlogPath(pathname)) return <BlogLayout>{children}</BlogLayout>
  return (
    <div data-photography-theme={photography || undefined} className={cn("flex min-h-dvh flex-col", photography && [photographyThemeClassName, "bg-background text-foreground"])}>
      {photography ? <PhotographyHeader /> : <SiteHeader />}
      <main className="flex-1">{children}</main>
      {photography ? <PhotographyFooter /> : <SiteFooter />}
    </div>
  )
}
