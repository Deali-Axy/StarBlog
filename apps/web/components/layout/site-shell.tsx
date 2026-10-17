"use client"

import { usePathname } from "next/navigation"
import type { ReactNode } from "react"

import { PhotographyFooter, PhotographyHeader } from "@/components/photography/photography-chrome"
import { SiteFooter } from "@/components/layout/site-footer"
import { SiteHeader } from "@/components/layout/site-header"
import { isPhotographyPath } from "@/lib/photography/path"
import { cn } from "@/lib/utils"

export function SiteShell({ children }: { children: ReactNode }) {
  const photography = isPhotographyPath(usePathname())
  return (
    <div className={cn("flex min-h-dvh flex-col", photography && "photography-theme bg-background text-foreground")}>
      {photography ? <PhotographyHeader /> : <SiteHeader />}
      <main className="flex-1">{children}</main>
      {photography ? <PhotographyFooter /> : <SiteFooter />}
    </div>
  )
}
