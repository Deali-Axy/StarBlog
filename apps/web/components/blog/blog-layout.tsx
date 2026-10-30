"use client"

import { usePathname, useSearchParams } from "next/navigation"
import type { ReactNode } from "react"
import { BlogLayoutContext } from "@/components/blog/blog-layout-context"
import { BlogDesignSwitcher } from "@/components/blog/blog-variant-switcher"
import { IndexHeader, IndexFooter } from "@/components/blog/index-chrome"
import { FolioHeader, FolioFooter } from "@/components/blog/folio-chrome"
import { ArchiveHeader, ArchiveFooter } from "@/components/blog/archive-chrome"
import { resolveBlogVariant } from "@/lib/blog-variant"
import { blogThemeClassName } from "@/lib/blog-theme"
import { cn } from "@/lib/utils"
import styles from "./blog-chrome.module.css"

const chrome = {
  library: { Header: IndexHeader, Footer: IndexFooter },
  editorial: { Header: FolioHeader, Footer: FolioFooter },
  studio: { Header: ArchiveHeader, Footer: ArchiveFooter },
}

export function BlogLayout({ children }: { children: ReactNode }) {
  const variant = resolveBlogVariant(useSearchParams().get("bv"))
  const pathname = usePathname()
  const { Header, Footer } = chrome[variant]
  // Keep the main subtree stable when styles switch, preserving category/search UI state.
  return <BlogLayoutContext.Provider value={{ variant }}>
    <div data-blog-chrome={variant} className={cn(blogThemeClassName(variant), styles.layout)}>
      <Header />
      <main className={styles.main}>
        {pathname !== "/blog" ? <div className={styles.detailSwitcher}><BlogDesignSwitcher /></div> : null}
        {children}
      </main>
      <Footer />
    </div>
  </BlogLayoutContext.Provider>
}
