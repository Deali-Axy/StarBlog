import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"

import { getSite } from "@/lib/site"
import { eyebrowClassName, headingClassName, pageContainerClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

export async function generateMetadata(): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  return { title: t.aboutTitle, description: `${site.authorName} · ${site.authorBio}` }
}

export default async function AboutPage() {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  return (
    <div className={cn(pageContainerClassName, "py-16")}>
      <p className={eyebrowClassName}>Colophon</p>
      <h1 className={cn(headingClassName, "mt-2 text-3xl")}>{t.about}</h1>
      <div className="mt-12 grid gap-16 lg:grid-cols-[minmax(0,1fr)_18rem]">
        <div className="max-w-2xl space-y-12">
          <section>
            <h2 className={cn(headingClassName, "text-xl")}>{site.authorName}</h2>
            <p className="mt-4 leading-relaxed text-muted-foreground">{site.authorBio}</p>
            <div className="mt-4 flex flex-wrap gap-4 text-sm">
              <a href={site.githubUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                GitHub
              </a>
              <a href={site.zhihuUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                {t.zhihu}
              </a>
              <a href={site.cnblogsUrl} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                {t.cnblogs}
              </a>
            </div>
          </section>
          <section>
            <h2 className={cn(headingClassName, "text-xl")}>{t.skills}</h2>
            <p className="mt-2 text-sm text-muted-foreground">{t.skillsIntro}</p>
            <ul className="mt-4 space-y-2 text-sm leading-relaxed">
              {t.skillList.map((skill) => <li key={skill}>{skill}</li>)}
            </ul>
          </section>
          <section>
            <h2 className={cn(headingClassName, "text-xl")}>{site.labName}</h2>
            <p className="mt-4 leading-relaxed text-muted-foreground">{site.labDescription}</p>
          </section>
        </div>
        <aside className="space-y-8">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img src={site.labImage} alt={site.labName} className="w-full object-cover" />
          <div>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={site.wechatQr} alt={t.wechatQr} className="w-full bg-white p-4 object-contain" />
            <p className="mt-3 text-sm text-muted-foreground">{t.wechatHint}</p>
          </div>
        </aside>
      </div>
    </div>
  )
}
