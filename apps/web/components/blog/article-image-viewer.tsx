"use client"

import { useI18n } from "@/components/app-providers"
import { Dialog, DialogClose, DialogContent, DialogPortal, DialogTitle } from "@/components/ui/dialog"
import { ChevronLeft, ChevronRight, ExternalLink, Maximize, Minimize, RotateCw, X, ZoomIn, ZoomOut } from "lucide-react"
import { useEffect, useRef, useState, type ComponentProps, type MouseEvent, type ReactNode } from "react"

type ArticleImage = { src: string; alt: string }
type Gallery = { images: ArticleImage[]; index: number }
const actionClass = "inline-flex size-10 shrink-0 cursor-pointer items-center justify-center rounded-lg text-white/85 hover:bg-white/15 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white disabled:cursor-default disabled:opacity-30"

function ImageAction({ label, ...props }: ComponentProps<"button"> & { label: string }) {
  return <button type="button" className={actionClass} aria-label={label} title={label} {...props} />
}

/** Enhance server-rendered image links while retaining native navigation before hydration. */
export function ArticleImageViewer({ children }: { children: ReactNode }) {
  const { t } = useI18n()
  const root = useRef<HTMLDivElement>(null)
  const trigger = useRef<HTMLAnchorElement | null>(null)
  const content = useRef<HTMLDivElement>(null)
  const [gallery, setGallery] = useState<Gallery | null>(null)
  const [fullscreen, setFullscreen] = useState(false)
  const [notice, setNotice] = useState("")

  useEffect(() => {
    const update = () => setFullscreen(document.fullscreenElement === content.current && Boolean(content.current))
    document.addEventListener("fullscreenchange", update)
    return () => document.removeEventListener("fullscreenchange", update)
  }, [])

  function open(event: MouseEvent<HTMLDivElement>) {
    if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    const link = event.target instanceof Element ? event.target.closest<HTMLAnchorElement>("a[data-article-image]") : null
    if (!link || !root.current?.contains(link) || !link.getAttribute("href")) return
    const links = Array.from(root.current.querySelectorAll<HTMLAnchorElement>("a[data-article-image][href]"))
      .filter(anchor => anchor.getAttribute("href") && anchor.querySelector("img")?.getAttribute("src"))
    const index = links.indexOf(link)
    if (index < 0) return
    const images = links.map(anchor => ({ src: anchor.href, alt: anchor.querySelector("img")?.alt || anchor.querySelector("img")?.title || "" }))
    event.preventDefault()
    trigger.current = link
    setNotice("")
    setGallery({ images, index })
  }

  function move(direction: number) {
    setGallery(current => current ? { ...current, index: Math.max(0, Math.min(current.images.length - 1, current.index + direction)) } : null)
  }

  function close() {
    if (document.fullscreenElement === content.current) void document.exitFullscreen().catch(() => {})
    setGallery(null)
  }

  async function toggleFullscreen() {
    setNotice("")
    try {
      if (document.fullscreenElement === content.current) await document.exitFullscreen()
      else if (content.current?.requestFullscreen) await content.current.requestFullscreen()
      else setNotice(t.imageFullscreenUnavailable)
    } catch { setNotice(t.imageFullscreenUnavailable) }
  }

  const image = gallery?.images[gallery.index]
  return <div ref={root} onClick={open}>
    {children}
    <Dialog open={Boolean(gallery)} onOpenChange={value => { if (!value) close() }}>
      <DialogPortal>
        {gallery && image ? <DialogContent ref={content} aria-describedby={undefined} data-article-image-viewer
          onCloseAutoFocus={event => { event.preventDefault(); trigger.current?.focus({ preventScroll: true }) }}
          onKeyDown={event => {
            if (event.altKey || event.ctrlKey || event.metaKey) return
            if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
              event.preventDefault()
              move(event.key === "ArrowLeft" ? -1 : 1)
            }
          }}
          className="fixed inset-0 z-[70] flex h-dvh w-full flex-col bg-[#111310] text-white outline-none">
          <DialogTitle className="sr-only">{t.articleImagePreview}</DialogTitle>
          <div className="flex shrink-0 items-center gap-2 px-3 pb-2 pt-[max(12px,env(safe-area-inset-top))] sm:px-5">
            <span className="mr-auto pl-2 font-mono text-xs tabular-nums text-white/65" aria-live="polite">{gallery.index + 1} / {gallery.images.length}</span>
            <a href={image.src} target="_blank" rel="noopener noreferrer" className={actionClass} aria-label={t.imageOpenOriginal} title={t.imageOpenOriginal}><ExternalLink className="size-5" /></a>
            <ImageAction label={fullscreen ? t.imageExitFullscreen : t.imageFullscreen} onClick={() => void toggleFullscreen()}>{fullscreen ? <Minimize className="size-5" /> : <Maximize className="size-5" />}</ImageAction>
            <DialogClose asChild><ImageAction label={t.close}><X className="size-5" /></ImageAction></DialogClose>
          </div>
          <ImageStage key={`${gallery.index}:${image.src}`} image={image} />
          <div className="flex shrink-0 items-center gap-3 px-3 pb-[max(12px,env(safe-area-inset-bottom))] pt-2 sm:px-5">
            <ImageAction label={t.imagePrevious} disabled={gallery.index === 0} onClick={() => move(-1)}><ChevronLeft className="size-5" /></ImageAction>
            <p className="min-w-0 flex-1 truncate text-center text-xs text-white/65" title={image.alt}>{image.alt || t.articleImagePreview}</p>
            <ImageAction label={t.imageNext} disabled={gallery.index === gallery.images.length - 1} onClick={() => move(1)}><ChevronRight className="size-5" /></ImageAction>
          </div>
          {notice ? <p role="status" className="px-4 pb-3 text-center text-xs text-white/70">{notice}</p> : null}
        </DialogContent> : null}
      </DialogPortal>
    </Dialog>
  </div>
}

function ImageStage({ image }: { image: ArticleImage }) {
  const { t } = useI18n()
  const stage = useRef<HTMLDivElement>(null)
  const [bounds, setBounds] = useState({ width: 800, height: 600 })
  const [natural, setNatural] = useState({ width: 0, height: 0 })
  const [zoom, setZoom] = useState(1)
  const [rotation, setRotation] = useState(0)
  const [failed, setFailed] = useState(false)
  const [attempt, setAttempt] = useState(0)
  useEffect(() => {
    const element = stage.current
    if (element) element.scrollTo((element.scrollWidth - element.clientWidth) / 2, (element.scrollHeight - element.clientHeight) / 2)
  }, [zoom, rotation])
  useEffect(() => {
    const element = stage.current
    if (!element) return
    const observer = new ResizeObserver(() => setBounds({ width: element.clientWidth, height: element.clientHeight }))
    observer.observe(element)
    return () => observer.disconnect()
  }, [])
  const sideways = rotation % 180 !== 0
  const width = sideways ? natural.height : natural.width
  const height = sideways ? natural.width : natural.height
  const fit = width && height ? Math.min(1, Math.max(1, bounds.width - 48) / width, Math.max(1, bounds.height - 48) / height) : 1
  const scale = fit * zoom
  return <>
    <div ref={stage} className="min-h-0 flex-1 overflow-auto overscroll-contain" aria-busy={!natural.width && !failed}>
      <div className="grid min-h-full min-w-full w-max place-items-center p-6">
        {failed ? <div role="status" className="space-y-4 text-center text-sm text-white/75"><p>{t.imageLoadFailed}</p><button type="button" className="rounded-lg bg-white/15 px-4 py-2 text-white" onClick={() => { setFailed(false); setAttempt(value => value + 1) }}>{t.retry}</button></div>
          : <div className="relative" style={{ width: width * scale || 48, height: height * scale || 48 }}>
            {!natural.width ? <span role="status" className="absolute inset-0 grid place-items-center text-xs text-white/65">{t.imageLoading}</span> : null}
            {/* API media and external Markdown URLs are intentionally kept intact. */}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img key={attempt} src={image.src} alt={image.alt} draggable={false} decoding="async"
              onLoad={event => setNatural({ width: event.currentTarget.naturalWidth, height: event.currentTarget.naturalHeight })}
              onError={() => setFailed(true)} className="absolute left-1/2 top-1/2 max-w-none"
              style={{ width: natural.width * scale, height: natural.height * scale, transform: `translate(-50%, -50%) rotate(${rotation}deg)`, opacity: natural.width ? 1 : 0 }} />
          </div>}
      </div>
    </div>
    <div className="flex shrink-0 items-center justify-center gap-2 py-2" role="group" aria-label={t.imageTools}>
      <ImageAction label={t.imageZoomOut} disabled={zoom <= 0.5 || failed} onClick={() => setZoom(value => Math.max(0.5, value - 0.5))}><ZoomOut className="size-5" /></ImageAction>
      <button type="button" className="min-h-10 min-w-16 rounded-lg px-2 font-mono text-xs text-white/75 hover:bg-white/15 focus-visible:outline-2 focus-visible:outline-white" aria-label={t.imageReset} title={t.imageReset} onClick={() => { setZoom(1); setRotation(0); stage.current?.scrollTo(0, 0) }}>{Math.round(zoom * 100)}%</button>
      <ImageAction label={t.imageZoomIn} disabled={zoom >= 4 || failed} onClick={() => setZoom(value => Math.min(4, value + 0.5))}><ZoomIn className="size-5" /></ImageAction>
      <ImageAction label={t.imageRotate} disabled={failed} onClick={() => setRotation(value => (value + 90) % 360)}><RotateCw className="size-5" /></ImageAction>
    </div>
  </>
}
