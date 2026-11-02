"use client"

import { useId, useRef, useState } from "react"
import { Check, Copy } from "lucide-react"

import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"

export function FeedAddress({ url }: { url: string }) {
  const { t } = useI18n()
  const id = useId()
  const input = useRef<HTMLInputElement>(null)
  const [status, setStatus] = useState<"copied" | "manual" | null>(null)

  async function copyAddress() {
    try {
      await navigator.clipboard.writeText(url)
      setStatus("copied")
    } catch {
      input.current?.focus()
      input.current?.select()
      setStatus("manual")
    }
  }

  return (
    <div>
      <div className="flex flex-col gap-3 sm:flex-row">
        <label htmlFor={id} className="sr-only">{t.rssPage.addressLabel}</label>
        <input ref={input} id={id} type="text" readOnly value={url} onFocus={(event) => event.currentTarget.select()} className="min-h-12 w-full min-w-0 flex-1 rounded-none border border-border bg-background px-4 font-mono text-sm text-foreground selection:bg-primary/20 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary" />
        <Button type="button" className="min-h-12 shrink-0 rounded-none px-6" onClick={copyAddress}>
          {status === "copied" ? <Check aria-hidden="true" /> : <Copy aria-hidden="true" />}{t.rssPage.copy}
        </Button>
      </div>
      <p role="status" className="mt-2 text-xs leading-6 text-muted-foreground">{status === "copied" ? t.rssPage.copied : status === "manual" ? t.rssPage.copyFallback : null}</p>
    </div>
  )
}
