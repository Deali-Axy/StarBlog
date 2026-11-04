"use client"

import Link from "next/link"
import { useI18n } from "@/components/app-providers"
import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

export default function ErrorPage({ reset }: { reset: () => void }) {
  const { locale, t } = useI18n()
  return (
    <section className="mx-auto flex min-h-[60vh] max-w-xl flex-col justify-center px-4 py-16">
      <h1 className="text-2xl font-medium">{locale === "en" ? "This page could not be loaded" : "暂时无法加载此页面"}</h1>
      <p role="alert" className="mt-4 text-muted-foreground">{locale === "en" ? "Please try again in a moment, or return to the home page." : "请稍后重试，或先返回首页继续浏览。"}</p>
      <div className="mt-8 flex gap-3">
        <Button onClick={reset}>{t.retry}</Button>
        <Button variant="outline" asChild><Link href={routes.home}>{t.backHome}</Link></Button>
      </div>
    </section>
  )
}
