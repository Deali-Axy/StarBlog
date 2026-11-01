"use client"
import { useI18n } from "@/components/app-providers"
import { usePathname } from "next/navigation"
import { ArchiveBackToTop, ArchiveCopyright, ArchiveFooterNav, ArchiveHeaderTools, ArchiveHeaderWordmark, ArchiveSiteNav } from "./views/archive-chrome-parts"
export function ArchiveHeader() {
  const { t } = useI18n()
  const pathname = usePathname()
  return <header data-blog-header="studio" className={"sticky top-0 [z-index:40] bg-card text-foreground [border-bottom:1px_solid_var(--border)]"}>
    <div className={"flex items-stretch gap-6 [width:min(100%_-_48px,_1360px)] mx-auto min-h-16 max-[74.9375rem]:gap-4.5 max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:min-h-16 max-[39.9375rem]:gap-2"}>
      <div className={"flex items-center gap-3.5 shrink-0 [&_>_span]:pl-3.5 [&_>_span]:[border-left:1px_solid_var(--border)] [&_>_span]:text-muted-foreground [&_>_span]:[font:9px_ui-monospace,_monospace] [&_>_span]:[letter-spacing:.06em] max-[74.9375rem]:[&_>_span]:hidden"}><ArchiveHeaderWordmark /><span>{pathname === "/blog" ? t.archiveChromeIndex : t.archiveChromeReading}</span></div>
      <ArchiveSiteNav />
      <ArchiveHeaderTools />
    </div>
  </header>
}
export function ArchiveFooter() {
  const { t } = useI18n()
  return <footer data-blog-footer="studio" className={"[border-top:1px_solid_var(--border)] bg-card py-3.5"}>
    <div className={"flex flex-wrap items-center justify-between gap-4 [width:min(100%_-_48px,_1360px)] mx-auto [font:10px_ui-monospace,_monospace] max-[63.9375rem]:gap-3 max-[39.9375rem]:[width:calc(100%_-_32px)] max-[39.9375rem]:text-[.5625rem]"}>
      <div className={"flex flex-wrap items-center gap-2.5 [&_>_span:first-child]:w-1.5 [&_>_span:first-child]:h-1.5 [&_>_span:first-child]:bg-primary [&_>_span:last-child]:text-muted-foreground [&_>_span:last-child]:ml-1.5 max-[39.9375rem]:w-full max-[39.9375rem]:gap-2"}><span aria-hidden="true" />{t.archiveWorkbench}<ArchiveCopyright /></div>
      <ArchiveFooterNav />
      <ArchiveBackToTop />
    </div>
  </footer>
}
