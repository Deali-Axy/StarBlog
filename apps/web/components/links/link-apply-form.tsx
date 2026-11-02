"use client"

import { useI18n } from "@/components/app-providers"

import { type FormEvent, useState } from "react"
import { ArrowUpRight, LoaderCircle } from "lucide-react"
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
    <form className="grid gap-5 sm:grid-cols-2" onSubmit={onSubmit} aria-busy={apply.isPending} aria-describedby="link-form-hint">
      <div className="space-y-2">
        <Label htmlFor="link-name">{t.websiteName}</Label>
        <Input
          id="link-name"
          autoComplete="organization"
          className="min-h-11 rounded-none bg-background"
          required
          value={form.name}
          onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-url">{t.url}</Label>
        <Input
          id="link-url"
          type="url"
          autoComplete="url"
          className="min-h-11 rounded-none bg-background"
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
          autoComplete="name"
          className="min-h-11 rounded-none bg-background"
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
          autoComplete="email"
          className="min-h-11 rounded-none bg-background"
          required
          value={form.email}
          onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
        />
      </div>
      <div className="space-y-2 sm:col-span-2">
        <Label htmlFor="link-description">{t.description}{" "}<span className="ml-2 text-xs font-normal text-muted-foreground">{t.linksPage.optional}</span></Label>
        <Textarea
          id="link-description"
          className="min-h-28 rounded-none bg-background"
          value={form.description}
          onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
        />
      </div>
      <p id="link-form-hint" className="text-xs leading-6 text-muted-foreground sm:col-span-2">{t.linksPage.formHint}</p>
      <div className="flex flex-wrap gap-3 border-t border-border pt-5 sm:col-span-2">
        <Button type="submit" disabled={apply.isPending} className="min-h-11 rounded-none px-5">
          {apply.isPending ? <LoaderCircle aria-hidden="true" className="animate-spin motion-reduce:animate-none" /> : <ArrowUpRight aria-hidden="true" />}
          {apply.isPending ? t.linksPage.submitting : t.linksPage.submit}
        </Button>
        <Button type="reset" variant="ghost" disabled={apply.isPending} className="min-h-11 rounded-none" onClick={() => setForm(emptyForm)}>
          {t.reset}
        </Button>
      </div>
    </form>
  )
}
