"use client"

import { useEffect, useId, useRef, useState, type MouseEvent } from "react"
import { ArrowUp, ChevronDown, List } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import { currentHeadingIndex, headingIdFromHash, type ArticleHeading } from "@/lib/markdown"
import { useBlogLayout } from "@/components/blog/blog-layout-context"
import { cn } from "@/lib/utils"
import { articleDesigns, tocIndentClasses } from "./article-designs"
import nativeStyles from "./article-native.module.css"

export function SidebarTOC({ items, mobile = false }: { items: ArticleHeading[]; mobile?: boolean }) {
  const { t } = useI18n()
  const { variant } = useBlogLayout()
  const design = articleDesigns[variant]
  const navRef = useRef<HTMLElement>(null)
  const [activeId, setActiveId] = useState("")
  const [progress, setProgress] = useState(0)
  const listId = useId()
  const baseline = Math.min(...items.map(item => item.depth))

  useEffect(() => {
    const article = navRef.current?.closest("[data-blog-article-detail]")
    const body = article?.querySelector<HTMLElement>("[data-article-body]")
    const headings = items.map(item => document.getElementById(item.id)).filter((node): node is HTMLElement => Boolean(node))
    if (!body || !headings.length) return
    let frame = 0
    function update() {
      frame = 0
      const headerHeight = parseFloat(getComputedStyle(body!).getPropertyValue("--blog-header-height")) || 80
      const offset = headerHeight + 96
      const rect = body!.getBoundingClientRect()
      const atEnd = rect.bottom <= window.innerHeight
      const index = currentHeadingIndex(headings.map(heading => heading.getBoundingClientRect().top), offset, atEnd)
      setActiveId(headings[index]?.id ?? "")
      const distance = Math.max(1, rect.height - window.innerHeight + offset)
      setProgress(Math.round(Math.min(1, Math.max(0, (offset - rect.top) / distance)) * 100))
    }
    function schedule() { if (!frame) frame = requestAnimationFrame(update) }
    function restoreHash() {
      const id = headingIdFromHash(window.location.hash)
      const heading = headings.find(node => node.id === id)
      if (heading) heading.scrollIntoView({ block: "start", behavior: "instant" })
      schedule()
    }
    const observer = new ResizeObserver(schedule)
    observer.observe(body)
    window.addEventListener("scroll", schedule, { passive: true })
    window.addEventListener("resize", schedule)
    window.addEventListener("hashchange", restoreHash)
    // Covers direct hash links on first load, including percent-encoded Chinese IDs.
    if (window.location.hash) restoreHash()
    else schedule()
    return () => {
      cancelAnimationFrame(frame)
      observer.disconnect()
      window.removeEventListener("scroll", schedule)
      window.removeEventListener("resize", schedule)
      window.removeEventListener("hashchange", restoreHash)
    }
  }, [items])

  function navigate(event: MouseEvent<HTMLAnchorElement>, id: string) {
    if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    const heading = document.getElementById(id)
    if (!heading) return
    event.preventDefault()
    // Close the mobile contents before measuring the destination's position.
    const details = navRef.current?.closest("details")
    if (details) details.open = false
    const url = new URL(window.location.href)
    url.hash = encodeURIComponent(id)
    if (window.location.hash !== url.hash) window.history.pushState(null, "", url)
    heading.focus({ preventScroll: true })
    heading.scrollIntoView({ block: "start", behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })
    setActiveId(id)
  }

  if (!items.length) return null
  const nav = <nav ref={navRef} aria-label={t.toc} data-article-toc className={mobile ? "pb-4" : undefined}>
    {!mobile ? <div className={cn("flex justify-between gap-3 border-b border-foreground pb-4 text-xs", design.tocHeading)}>
      <span>{t.toc}</span><small className="font-mono text-xs text-muted-foreground">{String(items.length).padStart(2, "0")}</small>
    </div> : null}
    <ol id={listId} className={cn(
      "list-none overflow-y-auto overscroll-contain [scrollbar-width:thin]",
      mobile ? "max-h-80" : "mt-4 max-h-[calc(100dvh-var(--blog-header-height)-15rem)]",
      design.tocList,
    )}>
      {items.map((item, index) => <li key={item.id}>
        <a href={`#${encodeURIComponent(item.id)}`} onClick={event => navigate(event, item.id)}
          aria-current={activeId === item.id ? "location" : undefined}
          className={cn(
            "flex items-baseline gap-2 border-l-2 border-transparent px-2 py-2 text-xs leading-relaxed text-muted-foreground wrap-anywhere transition-colors hover:bg-muted hover:text-foreground aria-[current=location]:border-primary aria-[current=location]:bg-accent aria-[current=location]:text-foreground motion-reduce:transition-none",
            design.tocLink,
            tocIndentClasses[Math.min(tocIndentClasses.length - 1, Math.max(0, item.depth - baseline))],
          )}>
          <small className="shrink-0 font-mono text-xs opacity-60" aria-hidden="true">{String(index + 1).padStart(2, "0")}</small>
          <span>{item.text || t.untitledSection}</span>
        </a>
      </li>)}
    </ol>
    {!mobile ? <>
      <div className="mt-7 border-t border-border pt-4">
        <div className="flex justify-between gap-4 text-xs text-muted-foreground"><span>{t.readingProgress}</span><span>{progress}%</span></div>
        <progress className={cn("mt-3 block h-0.5 w-full appearance-none border-0 bg-border text-primary", nativeStyles.progress)} aria-label={t.readingProgress} max={100} value={progress} />
      </div>
      <button type="button" className="mt-4 inline-flex cursor-pointer items-center gap-2 py-2 text-xs text-muted-foreground hover:text-foreground"
        onClick={() => window.scrollTo({ top: 0, behavior: "instant" })}><ArrowUp size={14} aria-hidden="true" />{t.backToTop}</button>
    </> : null}
  </nav>

  return mobile ? <details className={cn("group/toc col-span-full row-start-3 mt-6 border-y border-border lg:hidden", nativeStyles.mobileContents, design.mobileToc)}>
    <summary className="flex min-h-12 cursor-pointer list-none items-center gap-2 text-xs">
      <List size={16} aria-hidden="true" /><span>{t.toc}</span>
      <small className="ml-auto font-mono text-xs text-muted-foreground">{t.sectionCount(items.length)}</small>
      <ChevronDown className="size-4 transition-transform group-open/toc:rotate-180 motion-reduce:transition-none" aria-hidden="true" />
    </summary>
    {nav}
  </details> : nav
}
