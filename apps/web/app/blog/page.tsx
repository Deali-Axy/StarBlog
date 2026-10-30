import type { Metadata } from "next"

import { BlogExplorer } from "@/components/blog/blog-explorer"
import { getCategoryTree, listPosts } from "@/lib/api/queries"
import { parseBlogQuery, type BlogSearchParams } from "@/lib/blog-query"
import { getI18n } from "@/lib/i18n-server"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.posts }
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
