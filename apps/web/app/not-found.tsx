import Link from "next/link"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

export default function NotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4 px-4 text-center">
      <h1 className="text-3xl font-bold">页面不存在</h1>
      <p className="text-muted-foreground">这篇文章可能尚未发布，或链接已经失效。</p>
      <Button asChild>
        <Link href={routes.blog}>返回文章列表</Link>
      </Button>
    </div>
  )
}
