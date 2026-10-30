"use client"

import Link from "next/link"
import { createContext, useContext, type ComponentProps } from "react"
import { blogStyleHref, type BlogVariant } from "@/lib/blog-variant"

export const BlogLayoutContext = createContext<{ variant: BlogVariant } | null>(null)

export function useBlogLayout() {
  const context = useContext(BlogLayoutContext)
  if (!context) throw new Error("Blog layout is required")
  return context
}

/** Only blog-to-blog navigation inherits the preview; canonical article URLs stay unchanged. */
export function BlogStyleLink({ href, ...props }: Omit<ComponentProps<typeof Link>, "href"> & { href: string }) {
  const context = useContext(BlogLayoutContext)
  return <Link {...props} href={context ? blogStyleHref(href, context.variant) : href} />
}
