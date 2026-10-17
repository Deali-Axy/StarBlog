"use client"

import { ImageOff, LoaderCircle, RotateCw } from "lucide-react"
import { useCallback, useState, type ReactNode } from "react"

import { useI18n } from "@/components/app-providers"
import { cn } from "@/lib/utils"

export function PhotoImage({ src, srcSet, sizes, alt, width, height, priority, className, imageClassName, children, onDimensions }: {
  src: string; srcSet?: string; sizes?: string; alt: string; width: number; height: number; priority?: boolean
  className?: string; imageClassName?: string; children?: ReactNode
  onDimensions?: (width: number, height: number) => void
}) {
  const { t } = useI18n()
  const [status, setStatus] = useState<"loading" | "loaded" | "failed">("loading")
  const [attempt, setAttempt] = useState(0)
  const attachImage = useCallback((node: HTMLImageElement | null) => {
    // SSR images can finish (or fail) before React attaches load/error handlers.
    if (!node?.complete) return
    setStatus(node.naturalWidth > 0 ? "loaded" : "failed")
    if (node.naturalWidth > 0) onDimensions?.(node.naturalWidth, node.naturalHeight)
  }, [onDimensions])
  const retryUrl = (url: string) => attempt ? `${url}${url.includes("?") ? "&" : "?"}retry=${attempt}` : url
  return (
    <div className={cn("relative overflow-hidden bg-muted", className)}>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img ref={attachImage} key={attempt} src={retryUrl(src)} srcSet={srcSet?.split(", ").map((entry) => { const [url, size] = entry.split(" "); return `${retryUrl(url)} ${size}` }).join(", ")} sizes={sizes} alt={alt} width={width} height={height} loading={priority ? "eager" : "lazy"} decoding="async" onLoad={(event) => { setStatus("loaded"); onDimensions?.(event.currentTarget.naturalWidth, event.currentTarget.naturalHeight) }} onError={() => setStatus("failed")} className={cn("block h-full w-full object-cover", status === "failed" && "opacity-0", imageClassName)} />
      {status === "loading" ? <span aria-hidden className="pointer-events-none absolute inset-0 grid place-items-center bg-muted"><LoaderCircle className="size-5 animate-spin text-muted-foreground/50" /></span> : null}
      {children}
      {status === "failed" ? <div className="absolute inset-0 z-10 flex flex-col items-center justify-center gap-2 bg-muted p-2 text-center text-muted-foreground"><ImageOff className="size-5" /><p role="status" className="text-xs">{t.photoLoadFailed}</p><button type="button" aria-label={`${t.retry}: ${alt}`} onClick={() => { setStatus("loading"); setAttempt((value) => value + 1) }} className="inline-flex min-h-11 items-center gap-1.5 px-3 text-xs hover:text-foreground"><RotateCw className="size-3.5" />{t.retry}</button></div> : null}
    </div>
  )
}
