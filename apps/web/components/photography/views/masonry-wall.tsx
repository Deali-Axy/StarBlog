"use client"
import { useI18n } from "@/components/app-providers"
import { layoutMasonry, photoColumns } from "@/lib/photography/layout"
import { routes } from "@/lib/routes"
import { Camera } from "lucide-react"
import Link from "next/link"
import { useLayoutEffect, useMemo } from "react"
import { PhotoTile } from "../photo-tile"
import type { PhotoWallProps } from "../photo-wall-props"
export function MasonryWall({ photos, aspects, frame, density, featuredIds, authorName, onOpen, onLayout, onDimensions, empty }: PhotoWallProps) {
  const { t } = useI18n()
  const layout = useMemo(() => {
    if(!frame.width)
      return null; const gap = frame.viewport < 640 ? 12 : 24; return layoutMasonry(aspects, frame.width, photoColumns(frame.viewport, density), gap)
  }, [aspects, frame, density])
  useLayoutEffect(() => {
    if(layout)
      onLayout()
  }, [layout, onLayout])
  return <>        {photos.length === 0 ? <div className="py-28 text-center text-sm text-muted-foreground"><Camera className="mx-auto mb-4 size-8 opacity-50" /><p>{empty}</p><Link href={routes.home} className="mt-5 inline-block underline underline-offset-4">{t.backHome}</Link></div> : layout ? (<div className="relative" style={{ height: layout.height }}>
    {layout.boxes.map((box) => <PhotoTile key={photos[box.index].id} photo={photos[box.index]} featured={featuredIds.has(photos[box.index].id)} authorName={authorName} priority={box.index < photoColumns(frame.viewport, density)} onOpen={onOpen} onDimensions={onDimensions} className="absolute" style={{ left: box.x, top: box.y, width: box.width, height: box.height }} sizes={`${Math.ceil(box.width)}px`} />)}
  </div>) : <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-6 lg:grid-cols-4 2xl:grid-cols-5">
    {photos.map((photo, index) => <PhotoTile key={photo.id} photo={photo} featured={featuredIds.has(photo.id)} authorName={authorName} priority={index < 5} onOpen={onOpen} onDimensions={onDimensions} style={{ aspectRatio: String(aspects[index]) }} sizes="(min-width: 1536px) 20vw, (min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw" />)}
  </div>}</>
}
