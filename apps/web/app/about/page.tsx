import type { Metadata } from "next"

import { site } from "@/lib/site"

export const metadata: Metadata = {
  title: "关于本网站",
  description: `${site.authorName} · ${site.authorBio}`,
}

export default function AboutPage() {
  return (
    <div className="page-wrap py-16">
      <p className="kicker">Colophon</p>
      <h1 className="mt-2 text-3xl">关于</h1>
      <div className="mt-12 grid gap-16 lg:grid-cols-[minmax(0,1fr)_18rem]">
        <div className="max-w-2xl space-y-12">
          <section>
            <h2 className="text-xl">{site.authorName}</h2>
            <p className="mt-4 leading-relaxed text-muted-foreground">{site.authorBio}</p>
            <div className="mt-4 flex flex-wrap gap-4 text-sm">
              <a href={site.githubUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                GitHub
              </a>
              <a href={site.zhihuUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                知乎
              </a>
              <a href={site.cnblogsUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                博客园
              </a>
            </div>
          </section>
          <section>
            <h2 className="text-xl">技能</h2>
            <p className="mt-2 text-sm text-muted-foreground">以 CRUD 为主，熟练使用搜索引擎，擅长各种 CV 操作</p>
            <ul className="mt-4 space-y-2 text-sm leading-relaxed">
              <li>后端开发 / C# / Python / Kotlin / Go / PHP</li>
              <li>前端开发 / React / Vue</li>
              <li>移动端开发 / Flutter / Android</li>
              <li>桌面端开发 / Winform / PyQt</li>
              <li>数据分析 · 爬虫 · 机器学习 / NLP / CV</li>
            </ul>
          </section>
          <section>
            <h2 className="text-xl">{site.labName}</h2>
            <p className="mt-4 leading-relaxed text-muted-foreground">{site.labDescription}</p>
          </section>
        </div>
        <aside className="space-y-8">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img src={site.labImage} alt={site.labName} className="w-full object-cover" />
          <div>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={site.wechatQr} alt="微信公众号二维码" className="w-full bg-white p-4 object-contain" />
            <p className="mt-3 text-sm text-muted-foreground">微信公众号，欢迎扫码交流。</p>
          </div>
        </aside>
      </div>
    </div>
  )
}
