import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"
import type { Metadata } from "next"

import { FeaturedCategories } from "@/components/home/featured-categories"
import { FeaturedPosts, LatestPosts } from "@/components/home/featured-sections"
import { HomeHero } from "@/components/home/home-hero"
import { CategoryWordCloud } from "@/components/home/category-word-cloud"
import { JsonLd } from "@/components/seo/json-ld"
import { Button } from "@/components/ui/button"
import { getCategoryWordCloud, getHome, getOverview, listPosts } from "@/lib/api/queries"
import { parseFeaturedCategoriesVariant } from "@/lib/featured-categories-variant"
import { postHref, routes } from "@/lib/routes"
import { absoluteUrl, getSite } from "@/lib/site"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  return {
    title: t.home,
    alternates: { canonical: "/" },
    openGraph: { title: site.name, description: site.description, url: absoluteUrl("/"), type: "website" },
  }
}

export default async function HomePage({
  searchParams,
}: {
  searchParams: Promise<{ fc?: string }>
}) {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const params = await searchParams
  const featuredCategoriesVariant = parseFeaturedCategoriesVariant(params.fc) ?? undefined
  const [home, overview, latest, wordCloud] = await Promise.all([
    getHome(locale),
    getOverview().catch(() => null),
    listPosts({ page: 1, pageSize: 12 }, locale),
    getCategoryWordCloud().catch(() => []),
  ])

  const photo = home.randomPhotoVisible ? home.randomPhoto : null

  return (
    <div className="relative overflow-hidden">
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
      <HomeHero locale={locale} site={site} overview={overview} photo={photo} topPost={home.topPost} />

      <div className="page-wrap relative pb-20">
        <FeaturedPosts posts={home.featuredPosts} />
        <LatestPosts posts={latest.items} />

        {home.chartVisible ? <CategoryWordCloud items={wordCloud} /> : null}

        <FeaturedCategories
          categories={home.featuredCategories}
          initialVariant={featuredCategoriesVariant}
        />

        {home.featuredPhotos.length > 0 ? (
          <section className="mt-20">
            <div className="mb-6 flex items-end justify-between">
              <div>
                <p className="kicker">{t.photoPaper}</p>
                <h2 className="mt-2 text-2xl">{t.featuredPhotos}</h2>
              </div>
              <Button variant="ghost" size="sm" asChild>
                <Link href={routes.photography}>{t.allPhotos}</Link>
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
              <p className="kicker">{t.exchange}</p>
              <h2 className="mt-2 text-2xl">{t.friendLinks}</h2>
            </div>
            <Button variant="ghost" size="sm" asChild>
              <Link href={routes.links}>{t.applyLink}</Link>
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
            <p className="text-sm text-muted-foreground">{t.noLinks}</p>
          )}
        </section>

        {home.topPost ? (
          <p className="sr-only">
            {t.pinnedPost}
            <Link href={postHref(home.topPost)}>{home.topPost.title}</Link>
          </p>
        ) : null}
      </div>
    </div>
  )
}
