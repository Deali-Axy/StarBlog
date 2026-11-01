"use client"
import { useI18n } from "@/components/app-providers"
import { toast } from "sonner"
export function useArticleActions() {
  const { t } = useI18n()
  return {
    share: async (title: string) => {
      try {
        if(navigator.share) await navigator.share({ title, url: window.location.href })
        else { await navigator.clipboard.writeText(window.location.href); toast.success(t.linkCopied) }
      } catch(error) { if(!(error instanceof DOMException && error.name === "AbortError")) toast.error(t.shareFailed) }
    }
  }
}
