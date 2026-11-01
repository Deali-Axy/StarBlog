"use client"

import { Award } from "lucide-react"
import Link from "next/link"
import { type CSSProperties } from "react"

import { useI18n } from "@/components/app-providers"
import { PhotoImage } from "@/components/photography/photo-image"
import { useHydrated } from "@/components/ui/use-hydrated"
import type { PhotoResponse } from "@/lib/api/types"
import { photoThumbnail, photoThumbnailSet } from "@/lib/media"
import { routes } from "@/lib/routes"
import { cn } from "@/lib/utils"
import { AuthorMark } from "./photo-author-mark"
export function PhotoTile({ photo, featured, authorName, priority, onOpen, onDimensions, className, style, sizes }: {
  photo: PhotoResponse; featured: boolean; authorName: string; priority: boolean; onOpen: (id?: string) => void; className?: string; style: CSSProperties; sizes: string
  onDimensions: (id: string, width: number, height: number) => void
}) {
  const { t } = useI18n()
  const hydrated = useHydrated()
  const title = photo.title || t.photography
  return (
    <div id={`photo-${photo.id}`} data-photo-id={photo.id} className={cn("group relative", className)} style={style}>
      <PhotoImage src={photoThumbnail(photo.id, 800)} srcSet={photoThumbnailSet(photo.id)} sizes={sizes} alt={title} width={photo.width || 1500} height={photo.height || 1000} priority={priority} onDimensions={photo.width > 0 && photo.height > 0 ? undefined : (width, height) => onDimensions(photo.id, width, height)} className="h-full w-full">
        <Link aria-disabled={!hydrated} id={`photo-link-${photo.id}`} href={routes.photo(photo.id)} scroll={false} aria-label={title} onClick={(event) => { if(!event.metaKey && !event.ctrlKey && !event.shiftKey && !event.altKey && event.button === 0) onOpen(photo.id) }} className="absolute inset-0 block focus-visible:outline-offset-[-3px]">
          {featured ? <span className="absolute top-3 left-3 grid size-7 place-items-center bg-black/55 text-white" title={t.featured} aria-label={t.featured}><Award className="size-4" /></span> : null}
          <span data-photo-caption className="pointer-events-none absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/75 to-transparent px-3 pt-10 pb-3 text-white opacity-0 transition-opacity duration-200 group-hover:opacity-100 group-focus-within:opacity-100 max-sm:opacity-100 [@media(hover:none)]:opacity-100 [@media(pointer:coarse)]:opacity-100">
            <span className="block truncate text-sm font-medium">{title}</span>
            <span className="mt-1 hidden items-center gap-1.5 text-xs text-white/80 sm:flex"><AuthorMark name={authorName} className="size-5 text-[10px]" />{authorName}</span>
          </span>
        </Link>
      </PhotoImage>
    </div>
  )
}
