import { notFound, redirect } from "next/navigation"

import { ApiError } from "@/lib/api/types"
import { getRandomPhoto } from "@/lib/api/queries"
import { routes } from "@/lib/routes"

export const dynamic = "force-dynamic"

export default async function RandomPhotoPage() {
  try {
    const photo = await getRandomPhoto()
    redirect(routes.photo(photo.id))
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound()
    throw error
  }
}
