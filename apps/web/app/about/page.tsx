import type { Metadata } from "next"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { site } from "@/lib/site"

export const metadata: Metadata = {
  title: "关于本网站",
  description: `${site.authorName} · ${site.authorBio}`,
}

export default function AboutPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-800">
      <div className="mx-auto w-full max-w-6xl px-4 py-12">
        <div className="mb-8 flex items-end justify-between gap-4">
          <h1 className="text-3xl font-bold">关于本网站</h1>
          <p className="text-sm text-muted-foreground">About</p>
        </div>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          <div className="space-y-6">
            <Card className="border-0 bg-white/80 py-6 shadow-sm dark:bg-slate-900/80">
              <CardHeader>
                <CardTitle>关于站长</CardTitle>
                <p className="text-sm text-muted-foreground">{site.authorName}</p>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm leading-relaxed">{site.authorBio}</p>
                <div className="flex flex-wrap gap-3 text-sm">
                  <a href={site.githubUrl} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline">
                    Github
                  </a>
                  <a href={site.zhihuUrl} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline">
                    知乎主页
                  </a>
                  <a href={site.cnblogsUrl} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline">
                    博客园
                  </a>
                </div>
              </CardContent>
            </Card>
            <Card className="border-0 bg-white/80 py-6 shadow-sm dark:bg-slate-900/80">
              <CardHeader>
                <CardTitle>主要技能</CardTitle>
                <p className="text-sm text-muted-foreground">以 CRUD 为主，熟练使用搜索引擎，擅长各种 CV 操作</p>
              </CardHeader>
              <CardContent>
                <ul className="space-y-2 text-sm">
                  <li>后端开发 / C# / Python / Kotlin / Go / PHP</li>
                  <li>前端开发 / React / Vue</li>
                  <li>移动端开发 / Flutter / Android</li>
                  <li>桌面端开发 / Winform / PyQt</li>
                  <li>数据分析</li>
                  <li>爬虫数据采集</li>
                  <li>机器学习 / 深度学习 / NLP / CV</li>
                </ul>
              </CardContent>
            </Card>
          </div>
          <Card className="overflow-hidden border-0 bg-white/80 py-0 shadow-sm dark:bg-slate-900/80">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={site.labImage} alt={site.labName} className="h-48 w-full object-cover" />
            <CardHeader>
              <CardTitle>{site.labName}</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm leading-relaxed text-muted-foreground">{site.labDescription}</p>
            </CardContent>
          </Card>
          <Card className="overflow-hidden border-0 bg-white/80 py-0 shadow-sm dark:bg-slate-900/80">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={site.wechatQr} alt="微信公众号二维码" className="h-48 w-full object-contain bg-white p-6" />
            <CardHeader>
              <CardTitle>微信公众号</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">欢迎扫码交流</p>
              <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{site.labDescription}</p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
