import type { Metadata } from "next"

import { BlogExplorer } from "@/components/blog/blog-explorer"
import { getCategoryTree, listPosts } from "@/lib/api/queries"
import { parseBlogQuery, type BlogSearchParams } from "@/lib/blog-query"
import { blogMetadata } from "@/lib/seo"
import { getI18n } from "@/lib/i18n-server"

export const revalidate = 300

export async function generateMetadata({ searchParams }: { searchParams: Promise<BlogSearchParams> }): Promise<Metadata> {
  const { locale } = await getI18n()
  const query = parseBlogQuery(await searchParams)
  const [categories, posts] = await Promise.all([
    getCategoryTree(), listPosts({ ...query, includeDescendants: Boolean(query.categoryId) }, locale),
  ])
  const metadata = blogMetadata(locale, query, categories)
  if (query.page > Math.max(1, posts.totalPages) || !posts.totalCount) metadata.robots = { index: false, follow: true }
  return metadata
}

export default async function BlogPage({ searchParams }: { searchParams: Promise<BlogSearchParams> }) {
  const { locale } = await getI18n()
  const query = parseBlogQuery(await searchParams)
  const [categories, posts] = await Promise.all([
    getCategoryTree(),
    listPosts({ ...query, includeDescendants: Boolean(query.categoryId) }, locale),
  ])
  return <BlogExplorer categories={categories} posts={posts} query={query} />
}
