import Link from "next/link"
import type { Metadata } from "next"
import { FolderOpen, Rss } from "lucide-react"

import { CategoryCard } from "@/components/blog/category-card"
import { EnhancedPostCard } from "@/components/blog/enhanced-post-card"
import { FeaturedPosts, LatestMarquee } from "@/components/home/featured-sections"
import { Button } from "@/components/ui/button"
import { getHome, getOverview, listPosts } from "@/lib/api/queries"
import { postHref, routes } from "@/lib/routes"
import { site } from "@/lib/site"

export const revalidate = 300

export const metadata: Metadata = {
  title: "首页",
}

export default async function HomePage() {
  const [home, overview, latest] = await Promise.all([
    getHome(),
    getOverview().catch(() => null),
    listPosts({ page: 1, pageSize: 12 }),
  ])

  const photo = home.randomPhotoVisible ? home.randomPhoto : null

  return (
    <div className="bg-gradient-to-br from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <section className="mx-auto grid w-full max-w-6xl gap-10 px-4 py-16 lg:grid-cols-2 lg:items-center">
        <div>
          <p className="text-sm font-medium tracking-wide text-blue-600 dark:text-blue-400">开源博客</p>
          <h1 className="mt-3 bg-gradient-to-r from-slate-900 to-slate-600 bg-clip-text text-4xl font-bold text-transparent md:text-5xl dark:from-white dark:to-slate-400">
            {site.name}
          </h1>
          <p className="mt-4 max-w-xl text-lg text-muted-foreground">{site.description}</p>
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
          <div className="mt-8 flex flex-wrap gap-3">
            <Button asChild>
              <Link href={routes.blog}>阅读文章</Link>
            </Button>
            <Button variant="outline" asChild>
              <a href={routes.feed}>
                <Rss />
                RSS
              </a>
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
          <Link href={routes.photo(photo.id)} className="hidden overflow-hidden rounded-2xl border border-border/70 shadow-lg md:block">
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

        {home.links.length > 0 ? (
          <section className="mt-16">
            <h2 className="mb-4 text-2xl font-bold">友情链接</h2>
            <ul className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-muted-foreground">
              {home.links.map((link) => (
                <li key={link.id}>
                  <a href={link.url} target="_blank" rel="noreferrer" className="hover:text-foreground">
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </section>
        ) : null}

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
