"use client"
import type { PhotoResponse } from "@/lib/api/types"
import { useHomeValue } from "./home-interaction-state"
export function useHomePhotoState(photo: PhotoResponse) {
  const [failed, setFailed] = useHomeValue(`photo-failed:${photo.id}`, false)
  const location = photo.location?.trim()
  const hasLocation = Boolean(location && !/^(unknown|未知|未知地点)$/i.test(location))
  return { failed, setFailed, location, hasLocation }
}
