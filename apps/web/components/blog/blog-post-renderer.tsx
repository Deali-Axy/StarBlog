"use client"

import { BookOpen, CalendarDays, Clock, Share2, Tag, User } from "lucide-react"
import { toast } from "sonner"

import { flattenToc, SidebarTOC } from "@/components/blog/sidebar-toc"
import { MarkdownContent } from "@/components/blog/markdown"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip"
import type { PostResponse } from "@/lib/api/types"
import { getAuthorInitials, site } from "@/lib/site"
import { formatDate, readingMinutes } from "@/lib/format"

export function BlogPostRenderer({ post }: { post: PostResponse }) {
  const toc = flattenToc(post.toc)
  const author = site.authorName
  const initials = getAuthorInitials()

  function share() {
    const url = window.location.href
    if (navigator.share) {
      void navigator.share({ title: post.title, url }).catch(() => {})
      return
    }
    void navigator.clipboard.writeText(url)
    toast.success("链接已复制")
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-800">
      <div className="mx-auto max-w-6xl px-4 py-8">
        <Card className="mb-8 border-0 bg-white/80 py-6 shadow-lg backdrop-blur-sm dark:bg-slate-900/80">
          <CardHeader>
            <div className="mb-4 flex items-center gap-3">
              <Avatar className="size-12">
                <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 font-bold text-white">
                  {initials}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 text-sm text-muted-foreground">
                <div className="flex items-center gap-2">
                  <User className="size-4" />
                  <span>{author}</span>
                  <Separator orientation="vertical" className="h-4" />
                  <CalendarDays className="size-4" />
                  <span>{formatDate(post.creationTime, "YYYY年MM月DD日")}</span>
                </div>
              </div>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button variant="outline" size="icon" onClick={share}>
                    <Share2 />
                  </Button>
                </TooltipTrigger>
                <TooltipContent>分享文章</TooltipContent>
              </Tooltip>
            </div>
            <h1 className="bg-gradient-to-r from-slate-900 to-slate-600 bg-clip-text text-4xl leading-tight font-bold text-transparent dark:from-slate-100 dark:to-slate-400">
              {post.title}
            </h1>
            {post.summary ? (
              <p className="mt-3 text-lg leading-relaxed text-muted-foreground">{post.summary}</p>
            ) : null}
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              <span className="flex items-center gap-2">
                <Tag className="size-4" />
                <Badge variant="secondary">{post.categoryName || "未分类"}</Badge>
              </span>
              <Separator orientation="vertical" className="h-4" />
              <span className="flex items-center gap-2">
                <BookOpen className="size-4" />
                {post.content?.length ?? 0} 字
              </span>
              <Separator orientation="vertical" className="h-4" />
              <span className="flex items-center gap-2">
                <Clock className="size-4" />
                约 {readingMinutes(post.content)} 分钟阅读
              </span>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 gap-8 lg:grid-cols-4">
          <article className="lg:col-span-3">
            <Card className="border-0 bg-white/80 py-6 shadow-lg backdrop-blur-sm dark:bg-slate-900/80">
              <CardContent className="p-8">
                <MarkdownContent content={post.content ?? ""} />
              </CardContent>
            </Card>
          </article>
          <aside className="lg:col-span-1">
            <div className="sticky top-24 space-y-6">
              {toc.length > 0 ? (
                <Card className="border-0 bg-white/80 py-4 shadow-lg backdrop-blur-sm dark:bg-slate-900/80">
                  <CardContent className="p-4">
                    <SidebarTOC items={toc} className="max-h-96 overflow-y-auto" />
                  </CardContent>
                </Card>
              ) : null}
              <Card className="border-0 bg-white/80 py-4 shadow-lg backdrop-blur-sm dark:bg-slate-900/80">
                <CardHeader className="pb-3">
                  <h3 className="flex items-center gap-2 text-sm font-semibold">
                    <User className="size-4" />
                    作者信息
                  </h3>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-3">
                    <Avatar>
                      <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 text-xs font-bold text-white">
                        {initials}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="text-sm font-medium">{author}</p>
                      <p className="text-xs text-muted-foreground">{site.authorTitle}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </aside>
        </div>
      </div>
    </div>
  )
}
