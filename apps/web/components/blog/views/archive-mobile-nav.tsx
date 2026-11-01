"use client"
import { useI18n } from "@/components/app-providers"
import { BlogStyleLink } from "@/components/blog/blog-layout-context"
import { Sheet, SheetClose, SheetContent, SheetOverlay, SheetPortal, SheetTitle, SheetTrigger } from "@/components/ui/sheet"
import { blogThemeClassName } from "@/lib/blog-theme"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"
import { cn } from "@/lib/utils"
import { ArrowUpRight, Menu, X } from "lucide-react"
import { useState } from "react"
import { useBlogSiteLinks } from "../blog-site-links"
export function ArchiveMobileNav() {
  const { t } = useI18n()
  const links = useBlogSiteLinks()
  const [open, setOpen] = useState(false)
  return <Sheet open={open} onOpenChange={setOpen}>
    <SheetTrigger asChild><button type="button" className={"items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5 hidden max-[63.9375rem]:inline-flex"} aria-label={t.siteNavigation}><Menu className="size-5" /></button></SheetTrigger>
    <SheetPortal>
      <SheetOverlay className={"fixed [z-index:50] [inset:0] [background:rgb(0_0_0_/_.4)] [backdrop-filter:blur(4px)]"} />
      <SheetContent data-blog-menu="studio" className={cn(blogThemeClassName("studio"), "fixed [z-index:50] top-4 right-4 left-4 max-h-[none] overflow-y-auto p-6 [border:1px_solid_var(--foreground)] bg-card [inset:0_0_0_auto] [width:min(340px,_calc(100vw_-_32px))] [border-color:var(--border)] max-[39.9375rem]:p-4.5 motion-reduce:[&_*]:[scroll-behavior:auto]! motion-reduce:[&_*]:[transition:none]! motion-reduce:[&_*]:[animation:none]!")}>
        <div className={"flex justify-between items-center gap-6 pb-5 [border-bottom:2px_solid_var(--foreground)] text-xs"}><SheetTitle>{t.siteNavigation}</SheetTitle><SheetClose asChild><button type="button" aria-label={t.close} className={"inline-flex items-center justify-center shrink-0 min-w-9.5 w-9.5 h-9.5 border-0 rounded-none p-0 [color:inherit] [background:transparent] [box-shadow:none] cursor-pointer [letter-spacing:0] [&:hover]:bg-muted [&:hover]:text-foreground max-[39.9375rem]:min-w-8.5 max-[39.9375rem]:w-8.5 max-[39.9375rem]:h-9.5"}><X className="size-5" /></button></SheetClose></div>
        <nav aria-label={t.siteNavigation} className={"flex flex-col [margin-block:12px] [&_a]:flex [&_a]:items-baseline [&_a]:gap-4 [&_a]:py-4.5 [&_a]:[border-bottom:1px_solid_var(--border)] [&_a]:text-3xl [&_a]:font-bold [&_small]:[font:10px_ui-monospace,_monospace] [&_small]:text-muted-foreground [&_svg]:ml-auto [&_a[aria-current=page]_span]:bg-accent"}>
          {links.map(({ href, label }, index) => <BlogStyleLink key={href} href={href} aria-current={href === routes.blog ? "page" : undefined} onClick={() => setOpen(false)}>
            <small aria-hidden="true">0{index + 1}</small><span>{label}</span><ArrowUpRight className="size-5" aria-hidden="true" />
          </BlogStyleLink>)}
        </nav>
        <p className={"mt-6 [font:10px_ui-monospace,_monospace] text-muted-foreground"}>{site.name} / {site.authorName}</p>
      </SheetContent>
    </SheetPortal>
  </Sheet>
}
