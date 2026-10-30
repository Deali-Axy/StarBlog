import Link from "next/link"
import { ArrowDownRight, ArrowUpRight, Camera, Rss, Shuffle } from "lucide-react"

import { StarMark } from "@/components/brand/star-mark"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { HeroPhoto } from "@/components/home/hero-photo"
import type { PhotoResponse, PublishedPostSummary, SiteOverviewResponse } from "@/lib/api/types"
import { messages, type Locale } from "@/lib/i18n"
import { routes } from "@/lib/routes"
import type { getSite } from "@/lib/site"

type HomeHeroProps = {
  locale: Locale
  site: ReturnType<typeof getSite>
  overview: SiteOverviewResponse | null
  photo?: PhotoResponse | null
  topPost?: PublishedPostSummary | null
}

export function HomeHero({ locale, site, overview, photo, topPost }: HomeHeroProps) {
  const t = messages[locale]
  const identity = site.authorName.match(/^(.+?)\s*[(（]([^()（）]+)[)）]\s*$/u)
  const name = identity?.[1]?.trim() || site.authorName
  const alias = identity?.[2]?.trim()
  const hasVisual = Boolean(photo || topPost)

  return (
    <section className="home-hero page-wrap" aria-labelledby="home-hero-name">
      <div className="home-hero-edition">
        <span className="flex items-center gap-2.5">
          <span className="home-hero-dot" />
          {t.personalJournal}
        </span>
        <span>{t.hero}</span>
      </div>

      <div className={`home-hero-spread${hasVisual ? "" : " home-hero-spread-text"}`}>
        <div className="home-hero-identity">
          <div className="home-hero-nameplate">
            <h1 id="home-hero-name" className="home-hero-name" data-long-name={Array.from(name).length > 3}>
              {name}
            </h1>
            <StarMark className="home-hero-star" />
          </div>
          {alias ? <p className="home-hero-alias">{alias}<span aria-hidden="true"> /</span></p> : null}

          <p className="home-hero-tagline">{t.tagline}</p>

          <div className="home-hero-bio">
            <p className="text-sm font-medium">{site.authorTitle}</p>
            <p className="mt-2 max-w-sm text-sm leading-7 text-muted-foreground">{site.authorBio}</p>
            <Link href={routes.about} className="home-hero-about">
              {t.aboutMe}<ArrowUpRight aria-hidden="true" className="size-3.5" />
            </Link>
          </div>

          <div className="home-hero-actions">
            <Link href={routes.blogRandom} className="home-hero-random">
              <span className="home-hero-random-icon"><Shuffle aria-hidden="true" className="size-5" /></span>
              <span>{t.randomPost}</span>
              <ArrowUpRight aria-hidden="true" className="size-4" />
            </Link>
            <Link href={routes.photographyRandom} className="home-hero-photo-link">
              <Camera aria-hidden="true" className="size-4" />{t.randomPhoto}
            </Link>
          </div>
        </div>

        {photo ? (
          <HeroPhoto photo={photo} />
        ) : topPost ? (
          <div className="home-hero-featured">
            <p className="home-hero-photo-label mb-6">{t.pinned}</p>
            <EnhancedPostCard
              variant="featured"
              post={{ ...topPost, featured: true }}
            />
          </div>
        ) : null}
      </div>

      <div className="home-hero-colophon">
        {overview ? (
          <dl className="home-hero-stats">
            {[
              { label: t.posts, count: overview.postsCount, href: routes.blog },
              { label: t.categories, count: overview.categoriesCount, href: routes.blog },
              { label: t.photography, count: overview.photosCount, href: routes.photography },
            ].map(({ label, count, href }) => (
              <div key={label}>
                <dt>{label}</dt>
                <dd><Link href={href}>{count.toLocaleString(locale === "zh" ? "zh-CN" : "en-US")}</Link></dd>
              </div>
            ))}
          </dl>
        ) : null}

        <div className="home-hero-index">
          <div className="home-hero-skills" aria-label={t.skills}>
            {site.skills.map((skill) => (
              <Link key={skill.label} href={routes.searchResults(skill.keyword)}>{skill.label}</Link>
            ))}
          </div>
          <div className="home-hero-socials">
            <a href={site.githubUrl} target="_blank" rel="noreferrer">GitHub<ArrowUpRight aria-hidden="true" /></a>
            <a href={site.zhihuUrl} target="_blank" rel="noreferrer">{t.zhihu}<ArrowUpRight aria-hidden="true" /></a>
            <a href={site.cnblogsUrl} target="_blank" rel="noreferrer">{t.cnblogs}<ArrowUpRight aria-hidden="true" /></a>
            <Link href={routes.rss}><Rss aria-hidden="true" />RSS</Link>
          </div>
        </div>
        <ArrowDownRight aria-hidden="true" className="home-hero-scroll size-9 stroke-1" />
      </div>
    </section>
  )
}
