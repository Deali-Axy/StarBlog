import Link from "next/link"

import type { LinkResponse } from "@/lib/api/types"
import { routes } from "@/lib/routes"
import { site } from "@/lib/site"

export function SiteFooter({ links = [] }: { links?: LinkResponse[] }) {
  return (
    <footer className="border-t border-border/60 bg-background/80">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 px-4 py-10 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="font-heading font-semibold">{site.name}</p>
          <p className="mt-2 text-sm text-muted-foreground">用 Markdown 写作，用自己的网站发布。</p>
        </div>
        <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm">
          <Link href={routes.blog} className="text-muted-foreground hover:text-foreground">
            文章
          </Link>
          <Link href={routes.photography} className="text-muted-foreground hover:text-foreground">
            摄影
          </Link>
          <Link href={routes.links} className="text-muted-foreground hover:text-foreground">
            友链
          </Link>
          <Link href={routes.about} className="text-muted-foreground hover:text-foreground">
            关于
          </Link>
          <Link href={routes.rss} className="text-muted-foreground hover:text-foreground">
            RSS
          </Link>
        </div>
        {links.length > 0 ? (
          <div className="max-w-sm">
            <p className="text-sm font-medium">友情链接</p>
            <ul className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
              {links.map((link) => (
                <li key={link.id}>
                  <a href={link.url} target="_blank" rel="noreferrer" className="hover:text-foreground">
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </div>
    </footer>
  )
}
