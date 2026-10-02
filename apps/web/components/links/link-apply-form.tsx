"use client"

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
      toast.success("友链申请已提交，请留意邮件通知。")
      setForm(emptyForm)
    } catch (error) {
      toast.error(error instanceof ApiError ? error.message : "提交失败")
    }
  }

  return (
    <form className="space-y-4" onSubmit={onSubmit}>
      <div className="space-y-2">
        <Label htmlFor="link-name">网站名称</Label>
        <Input
          id="link-name"
          required
          value={form.name}
          onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-description">简介</Label>
        <Textarea
          id="link-description"
          value={form.description}
          onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-url">网址</Label>
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
        <Label htmlFor="link-webmaster">站长</Label>
        <Input
          id="link-webmaster"
          required
          value={form.webMaster}
          onChange={(event) => setForm((current) => ({ ...current, webMaster: event.target.value }))}
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="link-email">邮箱</Label>
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
          提交
        </Button>
        <Button type="reset" variant="outline" onClick={() => setForm(emptyForm)}>
          重置
        </Button>
      </div>
    </form>
  )
}
