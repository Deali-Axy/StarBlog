"use client"

import { useI18n } from "@/components/app-providers"
import { Sheet, SheetClose, SheetContent, SheetOverlay, SheetTitle, SheetTrigger } from "@/components/ui/sheet"
import { cn } from "@/lib/utils"
import { ArrowUp, List, MessageCircle, Share2, X } from "lucide-react"
import { useEffect, useRef, useState } from "react"
import { useArticleActions } from "./article-actions"
import { tocIndentClasses, useArticleReading } from "./article-reading"
import { useBlogLayout } from "./blog-layout-context"
import { useCommentSessionState } from "./comment-section"

const controlClassName = "inline-flex min-h-11 cursor-pointer items-center justify-center gap-2 px-3 text-xs hover:bg-accent hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"

export function ArticleCommentLink({ className }: { className?: string }) {
  const { t } = useI18n()
  const { total, commentsQuery } = useCommentSessionState()
  return <a href="#article-comments" className={cn(controlClassName, className)} onClick={event => {
    if(event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    const section = document.getElementById("article-comments")
    if(!section) return
    event.preventDefault()
    const url = new URL(window.location.href)
    url.hash = "article-comments"
    if(window.location.hash !== url.hash) window.history.pushState(null, "", url)
    section.focus({ preventScroll: true })
    section.scrollIntoView({ block: "start", behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })
  }}>
    <MessageCircle className="size-4 shrink-0" aria-hidden="true" />
    <span>{t.discussion}</span>
    {commentsQuery.isSuccess ? <span className="font-mono tabular-nums">{total}</span> : null}
  </a>
}

function useInlineActionsVisible() {
  const { variant } = useBlogLayout()
  const [visible, setVisible] = useState(false)
  useEffect(() => {
    const targets = document.querySelectorAll('[data-article-comments], [data-article-interactions="end"]')
    const visibleTargets = new Set<Element>()
    const observer = new IntersectionObserver(entries => {
      for(const entry of entries) {
        if(entry.isIntersecting) visibleTargets.add(entry.target)
        else visibleTargets.delete(entry.target)
      }
      setVisible(visibleTargets.size > 0)
    })
    targets.forEach(target => observer.observe(target))
    return () => observer.disconnect()
  }, [variant])
  return visible
}

/** Behaviors are shared; each layout chooses the desktop placement. */
export function ArticleInteractions({ title, className, end = false }: { title: string; className?: string; end?: boolean }) {
  const { t } = useI18n()
  const { share } = useArticleActions()
  const inlineVisible = useInlineActionsVisible()
  const sidebarControlClassName = "justify-start px-0 hover:bg-transparent [&>svg]:size-3.5"
  return <div data-article-interactions={end ? "end" : "sidebar"} className={cn("text-muted-foreground", className)}>
    <div className={cn("flex flex-wrap items-center", end ? "gap-1" : "gap-x-6", !end && inlineVisible && "invisible pointer-events-none")}>
      <ArticleCommentLink className={!end ? sidebarControlClassName : undefined} />
      <button type="button" className={cn(controlClassName, !end && sidebarControlClassName)} onClick={() => void share(title)}><Share2 className="size-4 shrink-0" aria-hidden="true" />{t.share}</button>
    </div>
    {!end ? <button type="button" className={cn(controlClassName, sidebarControlClassName)} onClick={() => window.scrollTo({ top: 0, behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" })}><ArrowUp className="size-3.5 shrink-0" aria-hidden="true" />{t.backToTop}</button> : null}
  </div>
}

export function ArticleMobileActions({ title }: { title: string }) {
  const { t } = useI18n()
  const { variant } = useBlogLayout()
  const { share } = useArticleActions()
  const { items, baseline, activeId, navigateTo } = useArticleReading()
  const pendingHeading = useRef<string | null>(null)
  const [contentsOpen, setContentsOpen] = useState(false)
  const inlineVisible = useInlineActionsVisible()

  useEffect(() => {
    const desktop = window.matchMedia("(min-width: 64rem)")
    const closeOnDesktop = () => { if(desktop.matches) setContentsOpen(false) }
    desktop.addEventListener("change", closeOnDesktop)
    return () => desktop.removeEventListener("change", closeOnDesktop)
  }, [])

  return <div data-article-mobile-actions className="lg:hidden [body:has([data-comment-form]:focus-within)_&]:invisible">
    <div aria-hidden="true" className="h-[calc(4rem+env(safe-area-inset-bottom))]" />
    <div hidden={inlineVisible} className={cn("fixed inset-x-0 bottom-0 z-40 border-t border-border bg-background px-3 pb-[env(safe-area-inset-bottom)]", variant === "studio" && "bg-card", variant === "library" && "border-t-2 border-t-foreground")}>
      <div className="mx-auto flex min-h-14 max-w-lg items-center justify-around gap-2 text-muted-foreground [&>a]:px-2 [&>button]:px-2">
        <ArticleCommentLink />
        <button type="button" className={controlClassName} onClick={() => void share(title)}><Share2 className="size-4" aria-hidden="true" />{t.share}</button>
        {items.length > 0 ? <Sheet open={contentsOpen} onOpenChange={setContentsOpen}>
          <SheetTrigger className={controlClassName}><List className="size-4" aria-hidden="true" />{t.toc}</SheetTrigger>
          <SheetOverlay className="lg:hidden" />
          <SheetContent side="bottom" aria-describedby={undefined} onCloseAutoFocus={event => {
            const id = pendingHeading.current
            if(!id) return
            event.preventDefault()
            pendingHeading.current = null
            requestAnimationFrame(() => navigateTo(id))
          }} className="max-h-[75dvh] overflow-y-auto overscroll-contain border-border px-5 pt-4 pb-[calc(1rem+env(safe-area-inset-bottom))] lg:hidden">
            <div className="mb-3 flex items-center justify-between border-b border-border pb-3">
              <SheetTitle className="font-heading text-lg">{t.toc}</SheetTitle>
              <SheetClose aria-label={t.close} className={controlClassName}><X className="size-5" aria-hidden="true" /></SheetClose>
            </div>
            <nav aria-label={t.toc}><ol className="space-y-1">{items.map(item => <li key={item.id}>
              <a href={`#${encodeURIComponent(item.id)}`} aria-current={activeId === item.id ? "location" : undefined} className={cn("block border-l-2 border-transparent py-3 text-sm text-muted-foreground hover:bg-accent aria-[current=location]:border-primary aria-[current=location]:text-primary", tocIndentClasses[Math.min(tocIndentClasses.length - 1, Math.max(0, item.depth - baseline))])} onClick={event => {
                if(event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
                event.preventDefault()
                pendingHeading.current = item.id
                setContentsOpen(false)
              }}>{item.text || t.untitledSection}</a>
            </li>)}</ol></nav>
          </SheetContent>
        </Sheet> : null}
      </div>
    </div>
  </div>
}
