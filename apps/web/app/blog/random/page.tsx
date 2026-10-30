import { notFound, redirect } from "next/navigation"

import { ApiError } from "@/lib/api/types"
import { getRandomPost } from "@/lib/api/queries"
import { blogStyleHref, resolveBlogVariant } from "@/lib/blog-variant"
import { postHref } from "@/lib/routes"

export const dynamic = "force-dynamic"

export default async function RandomPostPage({ searchParams }: { searchParams: Promise<{ bv?: string }> }) {
  const { bv } = await searchParams
  try {
    const post = await getRandomPost()
    redirect(bv ? blogStyleHref(postHref(post), resolveBlogVariant(bv)) : postHref(post))
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
}
