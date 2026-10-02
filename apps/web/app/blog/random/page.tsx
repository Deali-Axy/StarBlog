import { notFound, redirect } from "next/navigation"

import { ApiError } from "@/lib/api/types"
import { getRandomPost } from "@/lib/api/queries"
import { postHref } from "@/lib/routes"

export const dynamic = "force-dynamic"

export default async function RandomPostPage() {
  try {
    const post = await getRandomPost()
    redirect(postHref(post))
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
}
