"use client"

import { Camera } from "lucide-react"
import { useRouter } from "next/navigation"
import { useI18n } from "@/components/app-providers"
import { photoInsetClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export function PhotographyUnavailable({ onRetry }: { onRetry?: () => void }) {
  const { t } = useI18n()
  const router = useRouter()
  return (
    <div className={cn(photoInsetClassName, "py-28 text-center")}>
      <Camera className="mx-auto mb-4 size-8 text-muted-foreground" />
      <p role="alert" className="text-sm text-muted-foreground">{t.photosLoadFailed}</p>
      <button type="button" onClick={onRetry ?? (() => router.refresh())} className="mt-6 min-h-11 rounded-xl border border-border px-6 text-sm hover:bg-muted">{t.retry}</button>
    </div>
  )
}
