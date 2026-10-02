import Link from "next/link"
import type { Metadata } from "next"
import { Camera, FolderOpen, Rss, Shuffle } from "lucide-react"

import { CategoryCard } from "@/components/blog/category-card"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { FeaturedPosts, LatestMarquee } from "@/components/home/featured-sections"
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
    <div className="bg-gradient-to-br from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
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
      <section className="mx-auto grid w-full max-w-6xl gap-10 px-4 py-16 lg:grid-cols-2 lg:items-center">
        <div>
          <p className="text-sm font-medium tracking-wide text-blue-600 dark:text-blue-400">开源博客</p>
          <h1 className="mt-3 bg-gradient-to-r from-slate-900 to-slate-600 bg-clip-text text-4xl font-bold text-transparent md:text-5xl dark:from-white dark:to-slate-400">
            {site.name}
          </h1>
          <p className="mt-2 text-lg text-muted-foreground">{site.authorName}</p>
          <p className="mt-2 max-w-xl text-muted-foreground">{site.authorBio}</p>
          {overview ? (
            <dl className="mt-6 flex flex-wrap gap-6 text-sm">
              <div>
                <dt className="text-muted-foreground">文章</dt>
                <dd className="text-2xl font-semibold">{overview.postsCount}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">分类</dt>
                <dd className="text-2xl font-semibold">{overview.categoriesCount}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">摄影</dt>
                <dd className="text-2xl font-semibold">{overview.photosCount}</dd>
              </div>
            </dl>
          ) : null}
          <div className="mt-6 flex flex-wrap gap-2">
            {site.skills.map((skill) => (
              <Link
                key={skill.label}
                href={routes.searchResults(skill.keyword)}
                className="rounded-full border border-border/70 bg-card px-3 py-1 text-sm text-muted-foreground transition hover:border-blue-400 hover:text-blue-600"
              >
                {skill.label}
              </Link>
            ))}
          </div>
          <div className="mt-4 flex flex-wrap gap-4 text-sm">
            <a href={site.githubUrl} target="_blank" rel="noreferrer" className="hover:text-blue-600">
              GitHub
            </a>
            <a href={site.zhihuUrl} target="_blank" rel="noreferrer" className="hover:text-blue-600">
              知乎
            </a>
            <a href={site.cnblogsUrl} target="_blank" rel="noreferrer" className="hover:text-blue-600">
              博客园
            </a>
          </div>
          <div className="mt-8 flex flex-wrap gap-3">
            <Button asChild>
              <Link href={routes.blogRandom}>
                <Shuffle />
                随机文章
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href={routes.photographyRandom}>
                <Camera />
                随机摄影
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href={routes.rss}>
                <Rss />
                RSS
              </Link>
            </Button>
          </div>
        </div>
        {home.topPost ? (
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
        ) : photo ? (
          <Link
            href={routes.photo(photo.id)}
            className="hidden overflow-hidden rounded-2xl border border-border/70 shadow-lg md:block"
          >
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={`/api/v1/photos/${photo.id}/thumbnail?width=900`}
              alt={photo.title}
              className="h-80 w-full object-cover"
            />
            <div className="bg-card p-4">
              <p className="font-medium">{photo.title}</p>
              <p className="text-sm text-muted-foreground">{photo.location}</p>
            </div>
          </Link>
        ) : null}
      </section>

      <div className="mx-auto w-full max-w-6xl px-4 pb-16">
        <FeaturedPosts posts={home.featuredPosts} />
        <LatestMarquee posts={latest.items} />

        {home.chartVisible ? <CategoryWordCloud items={wordCloud} /> : null}

        {home.featuredCategories.length > 0 ? (
          <section className="mt-16">
            <div className="mb-6 flex items-center gap-3">
              <div className="flex size-8 items-center justify-center rounded-lg bg-gradient-to-r from-emerald-500 to-teal-500">
                <FolderOpen className="size-4 text-white" />
              </div>
              <h2 className="text-2xl font-bold">精选分类</h2>
            </div>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
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
          <section className="mt-16">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-2xl font-bold">精选摄影</h2>
              <Button variant="ghost" size="sm" asChild>
                <Link href={routes.photography}>全部摄影 →</Link>
              </Button>
            </div>
            <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
              {home.featuredPhotos.map((item) => (
                <Link
                  key={item.id}
                  href={routes.photo(item.id)}
                  className="overflow-hidden rounded-xl border border-border/70 transition hover:shadow-md"
                >
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

        <section className="mt-16">
          <div className="mb-4 flex items-center justify-between gap-4">
            <h2 className="text-2xl font-bold">友情链接</h2>
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
