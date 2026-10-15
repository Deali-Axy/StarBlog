import type { Metadata } from "next"
import { notFound } from "next/navigation"

import { BlogPostRenderer } from "@/components/blog/blog-post-renderer"
import { JsonLd } from "@/components/seo/json-ld"
import { ApiError } from "@/lib/api/types"
import { getPostTranslations, getPublishedPost, getRelatedPosts } from "@/lib/api/queries"
import { postHref, routes } from "@/lib/routes"
import { absoluteUrl, getSite } from "@/lib/site"
import { articleLanguage } from "@/lib/i18n"
import { getI18n } from "@/lib/i18n-server"

export const revalidate = 300

interface PostPageProps {
  params: Promise<{ id: string }>
  searchParams: Promise<{ lang?: string }>
}

export async function generateMetadata({ params, searchParams }: PostPageProps): Promise<Metadata> {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  const { lang } = await searchParams
  try {
    const post = await getPublishedPost(decodeURIComponent(id), articleLanguage(locale, lang))
    if (!post) return { title: t.postNotFound }
    const url = absoluteUrl(postHref(post))
    const description = post.summary ?? site.description
    return {
      title: post.title,
      description,
      alternates: { canonical: url },
      openGraph: {
        type: "article",
        title: post.title,
        description,
        url,
        siteName: site.name,
        locale: locale === "en" ? "en_US" : "zh_CN",
        publishedTime: post.creationTime,
        modifiedTime: post.lastUpdateTime,
      },
      twitter: {
        card: "summary",
        title: post.title,
        description,
      },
    }
  } catch {
    return { title: t.postNotFound }
  }
}

export default async function PostPage({ params, searchParams }: PostPageProps) {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  const { lang } = await searchParams
  const requestedLanguage = articleLanguage(locale, lang)
  const idOrSlug = decodeURIComponent(id)
  let post
  try {
    post = await getPublishedPost(idOrSlug, requestedLanguage)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
  if (!post) notFound()

  const [related, translations] = await Promise.all([
    getRelatedPosts(post.id, 4, locale).catch(() => []),
    getPostTranslations(post.id).catch(() => []),
  ])
  const activeLanguage = post.language ?? undefined
  const permalink = absoluteUrl(postHref(post))

  return (
    <>
      <JsonLd
        data={{
          "@context": "https://schema.org",
          "@graph": [
            {
              "@type": "BlogPosting",
              inLanguage: activeLanguage ?? "zh-CN",
              headline: post.title,
              description: post.summary ?? undefined,
              datePublished: post.creationTime,
              dateModified: post.lastUpdateTime,
              author: { "@type": "Person", name: site.authorName },
              publisher: { "@type": "Organization", name: site.name },
              mainEntityOfPage: permalink,
              url: permalink,
            },
            {
              "@type": "BreadcrumbList",
              itemListElement: [
                { "@type": "ListItem", position: 1, name: t.home, item: absoluteUrl(routes.home) },
                { "@type": "ListItem", position: 2, name: t.posts, item: absoluteUrl(routes.blog) },
                { "@type": "ListItem", position: 3, name: post.title, item: permalink },
              ],
            },
          ],
        }}
      />
      <BlogPostRenderer
        post={post}
        related={related}
        permalink={permalink}
        translations={translations}
        lang={activeLanguage}
        translationMissing={Boolean(requestedLanguage && !activeLanguage)}
      />
    </>
  )
}
