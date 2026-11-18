"use client"

import Image from "next/image"
import { useState, type SyntheticEvent } from "react"
import type { PostResponse } from "@/lib/api/types"
import { cn } from "@/lib/utils"
import type { CoverMode } from "./use-cover-mode"

type Candidate = NonNullable<PostResponse["coverImages"]>[number]
const singleSizes = "(max-width: 639px) calc(100vw - 32px), (max-width: 1023px) 55vw, 700px"

/** One shared frame keeps both presentations stable while bad candidates fall through. */
export function ArticleCover({ post, eager = false, mode = "collage", className }: { post: PostResponse; eager?: boolean; mode?: CoverMode; className?: string }) {
  const [rejected, setRejected] = useState<string[]>([])
  const candidates = post.coverImages?.filter((image, index, items) => !rejected.includes(image.url) && items.findIndex(item => item.url === image.url) === index) ?? []
  const images = candidates.slice(0, mode === "collage" ? 3 : 1)
  if (post.coverHidden || !images.length) return null
  const collage = images.length > 1
  const reject = (url: string) => setRejected(current => current.includes(url) ? current : [...current, url])
  return <span data-article-cover data-cover-mode={collage ? "collage" : "single"} data-cover-count={images.length}
    className={cn("relative mt-6 block aspect-[3/2] w-full overflow-hidden bg-muted/50 group-[&:nth-child(4n+2)]:order-[-1] group-[&:nth-child(4n+2)]:mb-6 group-[&:nth-child(4n+2)]:mt-0 group-[&:nth-child(4n+2)]:aspect-[4/3]", className)}>
    {collage ? <span className={cn("absolute inset-0 grid gap-1", images.length === 2 ? "grid-cols-2" : "grid-cols-[minmax(0,1.65fr)_minmax(0,1fr)] grid-rows-2")}>
      {images.map((image, index) => <span key={image.url} className={cn("relative min-h-0 min-w-0 overflow-hidden", images.length === 3 && index === 0 && "row-span-2")}>
        <CoverImage image={image} post={post} cropped eager={eager && index === 0} onReject={reject} compact
          sizes={images.length === 3 && index === 0 ? "(max-width: 639px) 62vw, (max-width: 1023px) 34vw, 440px" : "(max-width: 639px) 50vw, (max-width: 1023px) 28vw, 350px"} />
      </span>)}
    </span> : <CoverImage image={images[0]} post={post} cropped={post.coverFit === "cover"} eager={eager} onReject={reject} sizes={singleSizes} />}
  </span>
}

function CoverImage({ image, post, cropped, eager, onReject, sizes, compact = false }: {
  image: Candidate; post: PostResponse; cropped: boolean; eager: boolean; onReject: (url: string) => void; sizes: string; compact?: boolean
}) {
  const props = {
    alt: image.alt || post.title,
    loading: eager ? "eager" as const : "lazy" as const,
    onError: () => onReject(image.url),
    onLoad: (event: SyntheticEvent<HTMLImageElement>) => {
      const { naturalWidth: width, naturalHeight: height } = event.currentTarget
      // Responsive mosaic derivatives can be smaller than the original photograph.
      const tooSmall = compact ? width < 96 || height < 64 : width < 240 || height < 100
      if (image.url !== post.coverUrl && (tooSmall || height / width > 3.2)) onReject(image.url)
    },
    className: cn("h-full w-full transition-transform duration-500 motion-reduce:transition-none", cropped ? "object-cover group-hover:scale-[1.025]" : "object-contain"),
  }
  return image.url.startsWith("/media/") ? <Image {...props} src={image.url} fill sizes={sizes} />
    // Remote URLs render in the browser; the server never proxies arbitrary remote image hosts.
    // eslint-disable-next-line @next/next/no-img-element
    : <img {...props} src={image.url} decoding="async" />
}
