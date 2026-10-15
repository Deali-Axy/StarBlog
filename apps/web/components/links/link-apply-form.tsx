"use client"

import { useI18n } from "@/components/app-providers"

import { FormEvent, useState } from "react"
import { toast } from "sonner"

import { useApplyLinkExchange } from "@/features/links/api"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { ApiError } from "@/lib/api/types"

const emptyForm = {
  name: "",
  description: "",
  url: "",
  webMaster: "",
  email: "",
}

export function LinkApplyForm() {
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

  return (
    <form className="space-y-4" onSubmit={onSubmit}>
      <div className="space-y-2">
        <Label htmlFor="link-name">{t.websiteName}</Label>
        <Input
          id="link-name"
          required
          value={form.name}
          onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-description">{t.description}</Label>
        <Textarea
          id="link-description"
          value={form.description}
          onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-url">{t.url}</Label>
        <Input
          id="link-url"
          type="url"
          required
          placeholder="https://"
          value={form.url}
          onChange={(event) => setForm((current) => ({ ...current, url: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-webmaster">{t.webmaster}</Label>
        <Input
          id="link-webmaster"
          required
          value={form.webMaster}
          onChange={(event) => setForm((current) => ({ ...current, webMaster: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-email">{t.email}</Label>
        <Input
          id="link-email"
          type="email"
          required
          value={form.email}
          onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
        />
      </div>
      <div className="flex gap-2">
        <Button type="submit" disabled={apply.isPending}>
          {t.submit}
        </Button>
        <Button type="reset" variant="outline" onClick={() => setForm(emptyForm)}>
          {t.reset}
        </Button>
      </div>
    </form>
  )
}
