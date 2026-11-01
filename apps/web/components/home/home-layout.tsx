"use client"
import { useI18n } from "@/components/app-providers"
import type { ComponentType, ReactNode } from "react"
import { useHomeDesign } from "./home-design-context"
import { ColophonFooter } from "./views/colophon-footer"
import { CompactNavigation } from "./views/compact-navigation"
import { ContactFooter } from "./views/contact-footer"
import { NumberedNavigation } from "./views/numbered-navigation"
import { SidebarFrame } from "./views/sidebar-frame"
import { TopbarFrame } from "./views/topbar-frame"
type Config = ReturnType<typeof useHomeDesign>["config"]
const frames = { sidebar: SidebarFrame, topbar: TopbarFrame } satisfies Record<Config["frame"], ComponentType<{
  navigation: ReactNode
  footer: ReactNode
  children: ReactNode
}>>
const navigations = { numbered: NumberedNavigation, compact: CompactNavigation } satisfies Record<Config["navigation"], ComponentType>
const footers = { colophon: ColophonFooter, contact: ContactFooter } satisfies Record<Config["footer"], ComponentType>
export function HomeLayout({ children }: {
  children: ReactNode
}) {
  const { t } = useI18n()
  const { config } = useHomeDesign()
  const Frame = frames[config.frame], Navigation = navigations[config.navigation], Footer = footers[config.footer]
  return <div data-home-layout={config.frame} id="home-top"><a href="#home-content" className="fixed top-4 left-4 z-50 -translate-y-[calc(100%+2rem)] bg-foreground px-4 py-3 text-background focus:translate-y-0">{t.skipToContent}</a><Frame navigation={<Navigation />} footer={<Footer />}>{children}</Frame></div>
}
