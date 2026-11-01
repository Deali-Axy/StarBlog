import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"
import { cookies } from "next/headers"
import Link from "next/link"

import { CategoryWordCloud } from "@/components/home/category-word-cloud"
import { FeaturedCategories } from "@/components/home/featured-categories"
import { FeaturedPhotos } from "@/components/home/featured-photos"
import { FeaturedPosts, LatestPosts } from "@/components/home/featured-sections"
import { FriendLinks } from "@/components/home/friend-links"
import { HomeDesignProvider } from "@/components/home/home-design-context"
import { HomeDesignSwitcher } from "@/components/home/home-design-switcher"
import { HomeHero } from "@/components/home/home-hero"
import { HomeLayout } from "@/components/home/home-layout"
import { JsonLd } from "@/components/seo/json-ld"
import { getCategoryWordCloud, getHome, getOverview, listPosts } from "@/lib/api/queries"
import { homePreferencesCookie } from "@/lib/home-variant"
import { postHref } from "@/lib/routes"
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

export default async function HomePage() {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const savedPreferences = (await cookies()).get(homePreferencesCookie)?.value ?? ""
  const [home, overview, latest, wordCloud] = await Promise.all([
    getHome(locale),
    getOverview().catch(() => null),
    listPosts({ page: 1, pageSize: 12 }, locale),
    getCategoryWordCloud().catch(() => []),
  ])

  const photo = home.randomPhotoVisible ? home.randomPhoto : null

  return (
    <HomeDesignProvider savedPreferences={savedPreferences}>
      <HomeLayout>
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

        <div className={"w-full max-w-[var(--home-width)] mx-auto px-[1rem]"}>
          <FeaturedPosts posts={home.featuredPosts} />
          <LatestPosts posts={latest.items} />

          {home.chartVisible ? <CategoryWordCloud items={wordCloud} /> : null}

          <FeaturedCategories categories={home.featuredCategories} />
          <FeaturedPhotos photos={home.featuredPhotos} />
          <FriendLinks links={home.links} />

          {home.topPost ? (
            <p className="sr-only">
              {t.pinnedPost}
              <Link href={postHref(home.topPost)}>{home.topPost.title}</Link>
            </p>
          ) : null}
        </div>
      </HomeLayout>
      <HomeDesignSwitcher />
    </HomeDesignProvider>
  )
}
