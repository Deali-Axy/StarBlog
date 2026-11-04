import type { Metadata } from "next"
import { notFound } from "next/navigation"

import { BlogPostRenderer } from "@/components/blog/blog-post-renderer"
import { renderArticleMarkdown } from "@/components/blog/markdown"
import { JsonLd } from "@/components/seo/json-ld"
import { ApiError, type PostResponse } from "@/lib/api/types"
import { getPostTranslations, getPublishedPost, getRelatedPosts } from "@/lib/api/queries"
import { articleLanguages, articlePath, pageMetadata, plainDescription } from "@/lib/seo"
import { routes } from "@/lib/routes"
import { absoluteUrl, getSite, socialImage } from "@/lib/site"
import { articleLanguage } from "@/lib/i18n"
import { getI18n } from "@/lib/i18n-server"
import { readingMinutes } from "@/lib/format"

export const revalidate = 300

interface PostPageProps {
  params: Promise<{ id: string }>
  searchParams: Promise<{ lang?: string }>
}

function postMetadata(post: PostResponse): Omit<PostResponse, "content" | "toc"> {
  const metadata = { ...post }
  delete metadata.content
  delete metadata.toc
  return metadata
}

export async function generateMetadata({ params, searchParams }: PostPageProps): Promise<Metadata> {
  const { locale } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  const { lang } = await searchParams
  const post = await getPublishedPost(id, articleLanguage(locale, lang))
  if (!post) notFound()
  const translations = await getPostTranslations(post.id).catch(() => [])
  const description = plainDescription(post.summary?.trim() || post.content, site.description)
  const metadata = pageMetadata({ locale, title: post.title, description, path: articlePath(post), type: "article" })
  return {
    ...metadata,
    alternates: { ...metadata.alternates, languages: articleLanguages(post, translations) },
    openGraph: {
      ...metadata.openGraph, type: "article",
      locale: post.language === "en" ? "en_US" : post.language?.replaceAll("-", "_") || "zh_CN",
      publishedTime: post.creationTime, modifiedTime: post.lastUpdateTime,
      authors: [site.authorName],
    },
  }
}

export default async function PostPage({ params, searchParams }: PostPageProps) {
  const { locale, t } = await getI18n()
  const site = getSite(locale)
  const { id } = await params
  const { lang } = await searchParams
  const requestedLanguage = articleLanguage(locale, lang)
  const idOrSlug = id
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
  const permalink = absoluteUrl(articlePath(post))
  const markdown = renderArticleMarkdown(post.content ?? "", post.id)
  // Only metadata and rendered nodes cross the client boundary, not Markdown or
  // unused backend TOC/translation bodies and their parsing dependencies.
  const article = postMetadata(post)

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
              description: plainDescription(post.summary?.trim() || post.content, site.description),
              image: absoluteUrl(socialImage),
              datePublished: post.creationTime,
              dateModified: post.lastUpdateTime,
              author: { "@type": "Person", name: site.authorName, url: absoluteUrl(routes.about) },
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
        post={article}
        headings={markdown.headings}
        minutes={readingMinutes(post.content)}
        related={related.map(postMetadata)}
        permalink={permalink}
        translations={translations.map(({ language }) => ({ language }))}
        lang={activeLanguage}
        translationMissing={Boolean(requestedLanguage && !activeLanguage)}
      >
        {markdown.content}
      </BlogPostRenderer>
    </>
  )
}
