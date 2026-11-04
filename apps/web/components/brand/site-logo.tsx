import Image from "next/image"

import { site } from "@/lib/site"
import { cn } from "@/lib/utils"

// Assets are copied unchanged from docs/brand/mote/exports.
const brandPath = "/brand/mote"

function BrandAsset({ mark = false, reversed = false, className }: {
  mark?: boolean
  reversed?: boolean
  className?: string
}) {
  const name = mark ? "mark" : "logo"
  const dimensions = mark ? { width: 256, height: 256 } : { width: 726, height: 240 }
  const alt = mark ? "" : site.name

  // Home's theme preview can override the document's dark class.
  return (
    <span className={cn("inline-block shrink-0 align-middle", mark ? "size-7" : "w-[140px] min-w-[140px] sm:w-40", className)} aria-hidden={mark || undefined}>
      {!reversed && <Image src={`${brandPath}/${name}.svg`} {...dimensions} alt={alt} className="block h-auto w-full dark:hidden [[data-home-mode=light]_&]:block! [[data-home-mode=dark]_&]:hidden!" />}
      <Image src={`${brandPath}/${name}-reversed.svg`} {...dimensions} alt={alt} className={cn("h-auto w-full", reversed ? "block" : "hidden dark:block [[data-home-mode=light]_&]:hidden! [[data-home-mode=dark]_&]:block!")} />
    </span>
  )
}

export function SiteLogo(props: { reversed?: boolean; className?: string }) {
  return <BrandAsset {...props} />
}

export function MoteMark(props: { className?: string }) {
  return <BrandAsset mark {...props} />
}
