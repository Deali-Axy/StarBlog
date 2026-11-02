"use client"

import { ArrowUpRight, LoaderCircle } from "lucide-react"
import { useI18n } from "@/components/app-providers"
import type { LinkApplication } from "@/components/links/use-link-application"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

export function LinkApplyForm({ application, submitLabel }: { application: LinkApplication; submitLabel?: string }) {
  const { t } = useI18n()
  const { form, setForm, apply, onSubmit, reset } = application

  return (
    <form className="grid gap-5 sm:grid-cols-2" onSubmit={onSubmit} aria-busy={apply.isPending} aria-describedby="link-form-hint">
      <div className="space-y-2">
        <Label htmlFor="link-name">{t.websiteName}</Label>
        <Input
          id="link-name"
          autoComplete="organization"
          className="min-h-11 rounded-none bg-background shadow-none"
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
          className="min-h-11 rounded-none bg-background shadow-none"
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
          className="min-h-11 rounded-none bg-background shadow-none"
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
          className="min-h-11 rounded-none bg-background shadow-none"
          required
          value={form.email}
          onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
        />
      </div>
      <div className="space-y-2 sm:col-span-2">
        <Label htmlFor="link-description">{t.description}{" "}<span className="ml-2 text-xs font-normal text-muted-foreground">{t.linksPage.optional}</span></Label>
        <Textarea
          id="link-description"
          className="min-h-28 rounded-none bg-background shadow-none"
          value={form.description}
          onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
        />
      </div>
      <p id="link-form-hint" className="text-xs leading-6 text-muted-foreground sm:col-span-2">{t.linksPage.formHint}</p>
      <div className="flex flex-wrap gap-3 border-t border-border pt-5 sm:col-span-2">
        <Button type="submit" disabled={apply.isPending} className="min-h-11 rounded-none px-5">
          {apply.isPending ? <LoaderCircle aria-hidden="true" className="animate-spin motion-reduce:animate-none" /> : <ArrowUpRight aria-hidden="true" />}
          {apply.isPending ? t.linksPage.submitting : submitLabel ?? t.linksPage.submit}
        </Button>
        <Button type="reset" variant="ghost" disabled={apply.isPending} className="min-h-11 rounded-none" onClick={reset}>
          {t.reset}
        </Button>
      </div>
    </form>
  )
}
