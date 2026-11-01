"use client"

import { ArchiveFooter, ArchiveHeader } from "@/components/blog/archive-chrome"
import { BlogLayoutContext } from "@/components/blog/blog-layout-context"
import { BlogDesignSwitcher } from "@/components/blog/blog-variant-switcher"
import { FolioFooter, FolioHeader } from "@/components/blog/folio-chrome"
import { IndexFooter, IndexHeader } from "@/components/blog/index-chrome"
import { blogThemeClassName } from "@/lib/blog-theme"
import type { BlogVariant } from "@/lib/blog-variant"
import { resolveBlogVariant } from "@/lib/blog-variant"
import { cn } from "@/lib/utils"
import { useSearchParams } from "next/navigation"
import { useLayoutEffect, useRef, type ComponentType, type ReactNode } from "react"

const chrome = {
  library: { Header: IndexHeader, Footer: IndexFooter },
  editorial: { Header: FolioHeader, Footer: FolioFooter },
  studio: { Header: ArchiveHeader, Footer: ArchiveFooter },
} satisfies Record<BlogVariant, { Header: ComponentType; Footer: ComponentType }>

export function BlogLayout({ children }: { children: ReactNode }) {
  const variant = resolveBlogVariant(useSearchParams().get("bv"))
  const { Header, Footer } = chrome[variant]
  const rootRef = useRef<HTMLDivElement>(null)
  useLayoutEffect(() => {
    const root = rootRef.current, header = root?.querySelector<HTMLElement>("[data-blog-header]")
    if(!root || !header) return
    const update = () => root.style.setProperty("--blog-header-height", `${header.getBoundingClientRect().height}px`)
    update(); const observer = new ResizeObserver(update); observer.observe(header)
    return () => observer.disconnect()
  }, [variant])
  // Keep the main subtree stable when styles switch, preserving category/search UI state.
  return <BlogLayoutContext.Provider value={{ variant }}>
    <div ref={rootRef} data-blog-chrome={variant} className={cn(blogThemeClassName(variant), "flex min-h-dvh flex-col [--blog-header-height:80px] motion-reduce:[&_*]:scroll-auto! motion-reduce:[&_*]:transition-none! motion-reduce:[&_*]:animate-none!")}>
      <Header />
      <main className="min-w-0 flex-1">
        {children}
      </main>
      <Footer />
      <BlogDesignSwitcher />
    </div>
  </BlogLayoutContext.Provider>
}
