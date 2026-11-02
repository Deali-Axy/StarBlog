"use client"

import { type FormEvent, useState } from "react"
import { toast } from "sonner"
import { useI18n } from "@/components/app-providers"
import { useApplyLinkExchange } from "@/features/links/api"
import { ApiError } from "@/lib/api/types"

const emptyForm = {
  name: "",
  description: "",
  url: "",
  webMaster: "",
  email: "",
}

export function useLinkApplication() {
  const { locale, t } = useI18n()
  const apply = useApplyLinkExchange()
  const [form, setForm] = useState(emptyForm)

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    try {
      await apply.mutateAsync({
        name: form.name.trim(),
        description: form.description.trim() || undefined,
        url: form.url.trim(),
        webMaster: form.webMaster.trim(),
        email: form.email.trim(),
      })
      toast.success(t.linkSubmitted)
      setForm(emptyForm)
    } catch (error) {
      toast.error(locale === "zh" && error instanceof ApiError ? error.message : t.submitFailed)
    }
  }

  return { form, setForm, apply, onSubmit, reset: () => setForm(emptyForm) }
}

export type LinkApplication = ReturnType<typeof useLinkApplication>
