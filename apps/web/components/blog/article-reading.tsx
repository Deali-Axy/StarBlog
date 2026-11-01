"use client"
import { currentHeadingIndex, headingIdFromHash, type ArticleHeading } from "@/lib/markdown"
import { createContext, useContext, useEffect, useRef, useState, type MouseEvent, type ReactNode, type RefObject } from "react"
function useReading(items: ArticleHeading[], layoutKey: string, rootRef: RefObject<HTMLDivElement | null>) {
  const [activeId, setActiveId] = useState("")
  const [progress, setProgress] = useState(0)
  const baseline = Math.min(...items.map(item => item.depth), 6)

  useEffect(() => {
    const article = rootRef.current
    const body = article?.querySelector<HTMLElement>("[data-article-body]")
    const headings = items.map(item => document.getElementById(item.id)).filter((node): node is HTMLElement => Boolean(node))
    if(!body || !headings.length) return
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
    function schedule() { if(!frame) frame = requestAnimationFrame(update) }
    function restoreHash() {
      const id = headingIdFromHash(window.location.hash)
      const heading = headings.find(node => node.id === id)
      if(heading) heading.scrollIntoView({ block: "start", behavior: "instant" })
      schedule()
    }
    const observer = new ResizeObserver(schedule)
    observer.observe(body)
    window.addEventListener("scroll", schedule, { passive: true })
    window.addEventListener("resize", schedule)
    window.addEventListener("hashchange", restoreHash)
    // Covers direct hash links on first load, including percent-encoded Chinese IDs.
    if(window.location.hash) restoreHash()
    else schedule()
    return () => {
      cancelAnimationFrame(frame)
      observer.disconnect()
      window.removeEventListener("scroll", schedule)
      window.removeEventListener("resize", schedule)
      window.removeEventListener("hashchange", restoreHash)
    }
  }, [items, layoutKey, rootRef])

  function navigate(event: MouseEvent<HTMLAnchorElement>, id: string) {
    if(event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    const heading = document.getElementById(id)
    if(!heading) return
    event.preventDefault()
    // Close the mobile contents before measuring the destination's position.
    const details = event.currentTarget.closest("details")
    if(details) details.open = false
    const url = new URL(window.location.href)
    url.hash = encodeURIComponent(id)
    if(window.location.hash !== url.hash) window.history.pushState(null, "", url)
    heading.focus({ preventScroll: true })
    heading.scrollIntoView({ block: "start", behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })
    setActiveId(id)
  }

  return { items, baseline, activeId, progress, navigate }
}
const ReadingContext = createContext<ReturnType<typeof useReading> | null>(null)
export function ArticleReadingProvider({ items, layoutKey, children }: { items: ArticleHeading[]; layoutKey: string; children: ReactNode }) {
  const rootRef = useRef<HTMLDivElement>(null)
  const reading = useReading(items, layoutKey, rootRef)
  return <ReadingContext.Provider value={reading}><div ref={rootRef}>{children}</div></ReadingContext.Provider>
}
export function useArticleReading() { const state = useContext(ReadingContext); if(!state) throw new Error("Article reading context is required"); return state }
export const tocIndentClasses = ["ps-2", "ps-5", "ps-8", "ps-11", "ps-14", "ps-17"] as const
