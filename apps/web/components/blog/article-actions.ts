"use client"
import { useI18n } from "@/components/app-providers"
import { toast } from "sonner"
export function useArticleActions() {
  const { t } = useI18n()
  return {
    share: async (title: string) => {
      try {
        const url = new URL(window.location.href)
        url.hash = ""
        url.searchParams.delete("bv")
        if(navigator.share) await navigator.share({ title, url: url.href })
        else { await navigator.clipboard.writeText(url.href); toast.success(t.linkCopied) }
      } catch(error) { if(!(error instanceof DOMException && error.name === "AbortError")) toast.error(t.shareFailed) }
    }
  }
}
