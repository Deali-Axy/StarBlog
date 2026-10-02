import type { Metadata } from "next"
import { notFound } from "next/navigation"

import { BlogPostRenderer } from "@/components/blog/blog-post-renderer"
import { ApiError } from "@/lib/api/types"
import { getPublishedPost } from "@/lib/api/queries"

export const revalidate = 300

interface PostPageProps {
  params: Promise<{ id: string }>
}

export async function generateMetadata({ params }: PostPageProps): Promise<Metadata> {
  const { id } = await params
  try {
    const post = await getPublishedPost(id)
    if (!post) return { title: "文章不存在" }
    return {
      title: post.title,
      description: post.summary ?? undefined,
    }
  } catch {
    return { title: "文章不存在" }
  }
}

export default async function PostPage({ params }: PostPageProps) {
  const { id } = await params
  let post
  try {
    post = await getPublishedPost(decodeURIComponent(id))
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
  if (!post) notFound()
  return <BlogPostRenderer post={post} />
}
