import Link from "next/link"
import type { Metadata } from "next"
import { Camera, Rss, Shuffle } from "lucide-react"

import { CategoryCard } from "@/components/blog/category-card"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { FeaturedPosts, LatestPosts } from "@/components/home/featured-sections"
import { CategoryWordCloud } from "@/components/home/category-word-cloud"
import { JsonLd } from "@/components/seo/json-ld"
import { Button } from "@/components/ui/button"
import { getCategoryWordCloud, getHome, getOverview, listPosts } from "@/lib/api/queries"
import { postHref, routes } from "@/lib/routes"
import { absoluteUrl, site } from "@/lib/site"

export const revalidate = 300

export const metadata: Metadata = {
  title: "首页",
  alternates: { canonical: "/" },
  openGraph: {
    title: site.name,
    description: site.description,
    url: absoluteUrl("/"),
    type: "website",
  },
}

export default async function HomePage() {
  const [home, overview, latest, wordCloud] = await Promise.all([
    getHome(),
    getOverview().catch(() => null),
    listPosts({ page: 1, pageSize: 12 }),
    getCategoryWordCloud().catch(() => []),
  ])

  const photo = home.randomPhotoVisible ? home.randomPhoto : null

  return (
    <div className="relative overflow-hidden">
      <div className="pointer-events-none absolute inset-x-0 top-0 h-[28rem] bg-[radial-gradient(ellipse_at_80%_0%,oklch(0.82_0.10_78/0.16),transparent_58%)]" />
      <JsonLd
        data={{
          "@context": "https://schema.org",
          "@type": "WebSite",
          name: site.name,
          url: site.url,
          potentialAction: {
            "@type": "SearchAction",
            target: `${site.url}/search?keyword={search_term_string}`,
            "query-input": "required name=search_term_string",
          },
        }}
      />
      <section className="page-wrap relative grid gap-12 py-16 lg:grid-cols-2 lg:items-end lg:py-20">
        <div>
          <p className="kicker">写作 · 摄影 · 观星</p>
          <h1 className="mt-3 text-4xl md:text-5xl">{site.authorName}</h1>
          <p className="mt-3 text-muted-foreground">{site.authorTitle}</p>
          <p className="mt-5 max-w-xl text-[0.95rem] leading-relaxed text-muted-foreground">{site.authorBio}</p>
          {overview ? (
            <dl className="mt-8 flex flex-wrap gap-8 text-sm">
              <div>
                <dt className="text-muted-foreground">文章</dt>
                <dd className="font-heading text-2xl">{overview.postsCount}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">分类</dt>
                <dd className="font-heading text-2xl">{overview.categoriesCount}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">摄影</dt>
                <dd className="font-heading text-2xl">{overview.photosCount}</dd>
              </div>
            </dl>
          ) : null}
          <div className="mt-6 flex flex-wrap gap-2">
            {site.skills.map((skill) => (
              <Link
                key={skill.label}
                href={routes.searchResults(skill.keyword)}
                className="border border-border/80 px-3 py-1 text-sm text-muted-foreground transition-colors hover:border-primary hover:text-primary"
              >
                {skill.label}
              </Link>
            ))}
          </div>
          <div className="mt-4 flex flex-wrap gap-4 text-sm text-muted-foreground">
            <a href={site.githubUrl} target="_blank" rel="noreferrer" className="hover:text-primary">
              GitHub
            </a>
            <a href={site.zhihuUrl} target="_blank" rel="noreferrer" className="hover:text-primary">
              知乎
            </a>
            <a href={site.cnblogsUrl} target="_blank" rel="noreferrer" className="hover:text-primary">
              博客园
            </a>
          </div>
          <div className="mt-8 flex flex-wrap gap-3">
            <Button asChild>
              <Link href={routes.blogRandom}>
                <Shuffle />
                随机一篇
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href={routes.photographyRandom}>
                <Camera />
                随机一张
              </Link>
            </Button>
            <Button variant="ghost" asChild>
              <Link href={routes.rss}>
                <Rss />
                RSS
              </Link>
            </Button>
          </div>
        </div>
        {photo ? (
          <Link href={routes.photo(photo.id)} className="group hidden md:block">
            <figure>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={`/api/v1/photos/${photo.id}/thumbnail?width=900`}
                alt={photo.title}
                className="h-80 w-full object-cover"
              />
              <figcaption className="mt-3 flex items-baseline justify-between gap-3 text-xs text-muted-foreground">
                <span>{photo.title}</span>
                <span>{photo.location}</span>
              </figcaption>
            </figure>
          </Link>
        ) : home.topPost ? (
          <EnhancedPostCard
            variant="featured"
            post={{
              id: home.topPost.id,
              title: home.topPost.title,
              slug: home.topPost.slug,
              summary: home.topPost.summary,
              categoryName: home.topPost.categoryName,
              lastUpdateTime: home.topPost.lastUpdateTime,
              featured: true,
            }}
          />
        ) : null}
      </section>

      <div className="page-wrap relative pb-20">
        <FeaturedPosts posts={home.featuredPosts} />
        <LatestPosts posts={latest.items} />

        {home.chartVisible ? <CategoryWordCloud items={wordCloud} /> : null}

        {home.featuredCategories.length > 0 ? (
          <section className="mt-20">
            <p className="kicker">路径</p>
            <h2 className="mt-2 mb-6 text-2xl">精选分类</h2>
            <div className="grid grid-cols-1 gap-x-8 md:grid-cols-2 lg:grid-cols-3">
              {home.featuredCategories.map((category) => (
                <CategoryCard
                  key={category.id}
                  name={category.name}
                  categoryId={category.categoryId}
                />
              ))}
            </div>
          </section>
        ) : null}

        {home.featuredPhotos.length > 0 ? (
          <section className="mt-20">
            <div className="mb-6 flex items-end justify-between">
              <div>
                <p className="kicker">相纸</p>
                <h2 className="mt-2 text-2xl">精选摄影</h2>
              </div>
              <Button variant="ghost" size="sm" asChild>
                <Link href={routes.photography}>全部摄影</Link>
              </Button>
            </div>
            <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
              {home.featuredPhotos.map((item) => (
                <Link key={item.id} href={routes.photo(item.id)} className="block">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src={`/api/v1/photos/${item.id}/thumbnail?width=480`}
                    alt={item.title}
                    className="h-40 w-full object-cover"
                  />
                </Link>
              ))}
            </div>
          </section>
        ) : null}

        <section className="mt-20">
          <div className="mb-4 flex items-end justify-between gap-4">
            <div>
              <p className="kicker">往来</p>
              <h2 className="mt-2 text-2xl">友情链接</h2>
            </div>
            <Button variant="ghost" size="sm" asChild>
              <Link href={routes.links}>申请友链</Link>
            </Button>
          </div>
          {home.links.length > 0 ? (
            <ul className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-muted-foreground">
              {home.links.map((link) => (
                <li key={link.id}>
                  <a href={link.url} target="_blank" rel="noreferrer" className="hover:text-foreground">
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-muted-foreground">还没有友链，欢迎申请交换。</p>
          )}
        </section>

        {home.topPost ? (
          <p className="sr-only">
            置顶文章：
            <Link href={postHref(home.topPost)}>{home.topPost.title}</Link>
          </p>
        ) : null}
      </div>
    </div>
  )
}
