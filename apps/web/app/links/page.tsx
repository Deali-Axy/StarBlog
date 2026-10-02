import type { Metadata } from "next"

import { LinkApplyForm } from "@/components/links/link-apply-form"
import { listVisibleLinks } from "@/lib/api/queries"

export const revalidate = 300

export const metadata: Metadata = {
  title: "友情链接",
  description: "申请友链并查看已展示的站点。",
}

const requirements = [
  { title: "相互性", detail: "请先在您的网站添加本站链接，再进行友链申请。" },
  { title: "内容类别", detail: "本站优先招同类原创、内容相近的博客或网站。" },
  { title: "SEO", detail: "Baidu 和 Google 有正常收录，有近期快照的网站优先。" },
  { title: "合法性", detail: "不含有违反相关国家法律内容的合法网站，不接受垃圾站链接。" },
  { title: "更新及时性", detail: "不接受原创内容很少，且长期不更新的网站。" },
  { title: "可访问性", detail: "如您的网站无法访问，将会暂时撤销友情链接。" },
]

export default async function LinksPage() {
  const links = await listVisibleLinks()

  return (
    <div className="page-wrap space-y-14 py-16">
      <div>
        <p className="kicker">往来</p>
        <h1 className="mt-2 text-3xl">友情链接</h1>
      </div>

      {links.length > 0 ? (
        <ul className="grid gap-x-8 gap-y-6 sm:grid-cols-2 lg:grid-cols-3">
          {links.map((link) => (
            <li key={link.id} className="border-t border-border/70 pt-4">
              <a href={link.url} target="_blank" rel="noreferrer" className="block hover:text-primary">
                <p className="font-heading text-lg">{link.name}</p>
                {link.description ? (
                  <p className="mt-2 text-sm text-muted-foreground">{link.description}</p>
                ) : null}
                <p className="mt-2 truncate text-xs text-muted-foreground">{link.url}</p>
              </a>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-muted-foreground">暂时还没有公开友链，欢迎申请交换。</p>
      )}

      <div className="grid gap-12 lg:grid-cols-2">
        <section>
          <h2 className="text-xl">申请友链</h2>
          <div className="mt-6">
            <LinkApplyForm />
          </div>
        </section>
        <section>
          <h2 className="text-xl">注意事项</h2>
          <ol className="mt-6 space-y-4 text-sm">
            {requirements.map((item, index) => (
              <li key={item.title}>
                <p className="font-medium">
                  {index + 1}. {item.title}
                </p>
                <p className="mt-1 text-muted-foreground">{item.detail}</p>
              </li>
            ))}
          </ol>
        </section>
      </div>
    </div>
  )
}
