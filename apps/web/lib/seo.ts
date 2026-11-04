import type { Metadata } from "next"

import type { CategoryNodeResponse, PostResponse } from "@/lib/api/types"
import { categoryPath } from "@/lib/blog-categories"
import type { BlogQuery } from "@/lib/blog-query"
import type { Locale } from "@/lib/i18n"
import { postHref, routes } from "@/lib/routes"
import { absoluteUrl, getSite, socialImage } from "@/lib/site"

export const seoText = {
  zh: {
    home: "技术、摄影与生活手记",
    blog: "文章与手记",
    blogDescription: "浏览技术实践、开发经验与生活记录，按分类查找文章，发现值得阅读的内容。",
    photography: "摄影作品",
    photographyDescription: "浏览旅行、城市与日常生活中的摄影作品，记录文字之外的风景。",
    search: "搜索文章",
    searchDescription: "搜索文章标题和内容，查找感兴趣的技术主题与生活记录。",
    results: (keyword: string) => `搜索「${keyword}」`,
    category: (name: string) => `${name} · 分类文章`,
    categoryDescription: (name: string) => `浏览「${name}」分类及其子分类中的文章、实践经验与学习记录。`,
    page: (page: number) => `第 ${page} 页`,
  },
  en: {
    home: "Notes on code, photography and life",
    blog: "Articles and notes",
    blogDescription: "Explore articles on software development, practical experience and everyday life. Browse by topic and find your next read.",
    photography: "Photography",
    photographyDescription: "Photographs from travel, city streets and everyday life. Discover the stories beyond the words.",
    search: "Search articles",
    searchDescription: "Search article titles and content to find topics, development notes and stories that interest you.",
    results: (keyword: string) => `Search for “${keyword}”`,
    category: (name: string) => `${name} · Articles`,
    categoryDescription: (name: string) => `Explore articles, practical experience and learning notes in ${name} and its subcategories.`,
    page: (page: number) => `Page ${page}`,
  },
}

/** Keep descriptions readable when summaries are empty or contain Markdown. */
export function plainDescription(value: string | null | undefined, fallback: string, limit = 160) {
  const plain = (value ?? "")
    .replace(/```[\s\S]*?```/g, " ")
    .replace(/!\[[^\]]*\]\([^)]*\)/g, " ")
    .replace(/\[([^\]]+)\]\([^)]*\)/g, "$1")
    .replace(/<[^>]*>/g, " ")
    .replace(/[#*_`~>|]/g, " ")
    .replace(/\s+/g, " ").trim()
  const text = plain || fallback
  return Array.from(text).length > limit ? `${Array.from(text).slice(0, limit - 1).join("").trimEnd()}…` : text
}

interface PageMetadataOptions {
  locale: Locale
  title: string
  description: string
  path: string
  image?: string
  noindex?: boolean
  type?: "website" | "article"
}

/** Explicit titles avoid the root page's same-segment title-template exception. */
export function pageMetadata({ locale, title, description, path, image = socialImage, noindex = false, type = "website" }: PageMetadataOptions): Metadata {
  const site = getSite(locale)
  const fullTitle = `${title} · ${site.name}`
  const summary = plainDescription(description, site.description)
  return {
    title: { absolute: fullTitle },
    description: summary,
    alternates: {
      canonical: absoluteUrl(path),
      types: { "application/atom+xml": absoluteUrl(routes.feed) },
    },
    robots: { index: !noindex, follow: true },
    openGraph: {
      type, title: fullTitle, description: summary, url: absoluteUrl(path),
      siteName: site.name, locale: locale === "en" ? "en_US" : "zh_CN",
      images: [{ url: absoluteUrl(image), alt: fullTitle }],
    },
    twitter: { card: "summary_large_image", title: fullTitle, description: summary, images: [absoluteUrl(image)] },
  }
}

export function blogMetadata(locale: Locale, query: BlogQuery, categories: CategoryNodeResponse[]): Metadata {
  const text = seoText[locale]
  const category = categoryPath(categories, query.categoryId).at(-1)
  let title = query.search ? text.results(query.search) : category ? text.category(category.text) : text.blog
  if (query.page > 1) title += ` · ${text.page(query.page)}`
  // Sorting changes page membership; retain it instead of canonicalizing to an unrelated page.
  const path = routes.blogList({ ...query, bv: undefined, view: undefined })
  return pageMetadata({
    locale, title, path,
    description: category ? text.categoryDescription(category.text) : text.blogDescription,
    noindex: Boolean(query.search || (query.categoryId && !category) || query.sortBy !== "lastUpdateTime" || query.sortType !== "desc"),
  })
}

export function articlePath(post: Pick<PostResponse, "id" | "slug" | "language">) {
  const path = postHref(post)
  return post.language ? `${path}?lang=${encodeURIComponent(post.language)}` : path
}

export function articleLanguages(post: Pick<PostResponse, "id" | "slug">, translations: { language: string }[]) {
  const original = absoluteUrl(postHref(post))
  const languages: Record<string, string> = { "zh-CN": original, "x-default": original }
  for (const { language } of translations) {
    // Only advertise real language codes supplied by the translation catalog.
    if (/^[a-z]{2,3}(?:-[a-zA-Z0-9]{2,8})*$/.test(language) && language.toLowerCase() !== "zh-cn") {
      languages[language] = absoluteUrl(`${postHref(post)}?lang=${encodeURIComponent(language)}`)
    }
  }
  return languages
}

export function serializeJsonLd(data: Record<string, unknown>) {
  return JSON.stringify(data).replace(/</g, "\\u003c")
}
