import Link from "next/link"

import { Button } from "@/components/ui/button"
import { routes } from "@/lib/routes"

export default function NotFound() {
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-xl flex-col justify-center px-4 py-16">
      <p className="kicker">404</p>
      <h1 className="mt-3 text-3xl">这片天空暂时没有这颗星</h1>
      <p className="mt-4 text-muted-foreground">这篇文章可能尚未发布，或链接已经失效。</p>
      <div className="mt-8">
        <Button asChild>
          <Link href={routes.blog}>返回文章列表</Link>
        </Button>
      </div>
    </div>
  )
}
