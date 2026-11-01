"use client"

import { useI18n } from "@/components/app-providers"

import { useEffect, useRef } from "react"
import { useHomeValue } from "./home-interaction-state"

import { getRandomPhoto } from "@/lib/api/queries"
import type { PhotoResponse } from "@/lib/api/types"
import { photoThumbnail } from "@/lib/media"

function preloadThumbnail(id: string) {
  return new Promise<void>((resolve) => {
    const image = new Image()
    image.onload = () => resolve()
    image.onerror = () => resolve()
    image.src = photoThumbnail(id, 1200)
  })
}

export function useHeroPhoto({ photo, onFailure }: { photo: PhotoResponse; onFailure?: () => void }) {
  const { t } = useI18n()
  const imageRef = useRef<HTMLImageElement>(null)
  const [current, setCurrent] = useHomeValue(`hero-current:${photo.id}`, photo)
  const [failed, setFailed] = useHomeValue(`hero-failed:${photo.id}`, false)
  const [pending, setPending] = useHomeValue(`hero-pending:${photo.id}`, false)
  const location = current.location?.trim()
  const hasLocation = location && !/^(unknown|未知|未知地点)$/i.test(location)

  useEffect(() => {
    const image = imageRef.current
    let active = true
    // The server-rendered image may fail before React attaches onError.
    void Promise.resolve().then(() => {
      if(active && !failed && image?.complete && image.naturalWidth === 0) {
        setFailed(true)
        onFailure?.()
      }
    })
    return () => { active = false }
  }, [current.id, failed, onFailure, setFailed])

  async function swap() {
    if(pending) return
    setPending(true)
    try {
      let next = current
      for(let attempt = 0;attempt < 4;attempt += 1) {
        const candidate = await getRandomPhoto()
        if(candidate.id !== current.id) {
          next = candidate
          break
        }
      }
      if(next.id === current.id) return
      await preloadThumbnail(next.id)
      setFailed(false)
      setCurrent(next)
    } catch {
      // Keep the current photo so a failed request can be retried.
    } finally {
      setPending(false)
    }
  }

  return { imageRef, t, current, failed, setFailed, pending, location, hasLocation, swap }
}
