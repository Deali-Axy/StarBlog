"use client"
import { DropdownMenu } from "@/components/ui/dropdown-menu"
import { BLOG_PREVIEW, type BlogVariant } from "@/lib/blog-variant"
import { useState, type ComponentType } from "react"
import { useBlogLayout } from "./blog-layout-context"
import type { BlogPreviewProps } from "./blog-preview-props"
import { ArchivePreview } from "./views/archive-preview"
import { FolioPreview } from "./views/folio-preview"
import { IndexPreview } from "./views/index-preview"
const previews = { library: IndexPreview, editorial: FolioPreview, studio: ArchivePreview } satisfies Record<BlogVariant, ComponentType<BlogPreviewProps>>
export function BlogDesignSwitcher() {
  const { variant } = useBlogLayout(); const [open, setOpen] = useState(false)
  if(!BLOG_PREVIEW) return null
  const Preview = previews[variant]
  function switchVariant(next: BlogVariant) {
    const url = new URL(window.location.href); url.searchParams.set("bv", next)
    window.history.pushState(null, "", `${url.pathname}${url.search}${url.hash}`); setOpen(false)
  }
  return <DropdownMenu modal={false} open={open} onOpenChange={setOpen}><Preview value={variant} onValueChange={switchVariant} onCloseAutoFocus={event => { event.preventDefault(); requestAnimationFrame(() => document.querySelector<HTMLButtonElement>("[data-blog-preview-trigger]")?.focus()) }} /></DropdownMenu>
}
