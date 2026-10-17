export interface DecodedPhotoImage {
  image: HTMLImageElement
  status: "loading" | "ready" | "failed"
  promise: Promise<HTMLImageElement>
  attempt: number
  cancel: () => void
}

/** A viewing session retains only the current original and its neighbours. */
export class LightboxImageCache {
  private readonly entries = new Map<string, DecodedPhotoImage>()

  constructor(private readonly createImage: () => HTMLImageElement = () => new Image()) {}

  peek(src: string) {
    return this.entries.get(src)
  }

  retain(sources: readonly string[]) {
    const retained = new Set(sources)
    for (const [src, entry] of this.entries) {
      if (retained.has(src)) continue
      entry.cancel()
      this.entries.delete(src)
    }
  }

  clear() {
    this.retain([])
  }

  load(src: string, priority: "high" | "low" = "high"): DecodedPhotoImage {
    const existing = this.entries.get(src)
    if (existing && existing.status !== "failed") {
      if (priority === "high") existing.image.fetchPriority = "high"
      return existing
    }

    const image = this.createImage()
    image.decoding = "async"
    image.fetchPriority = priority
    const attempt = existing ? existing.attempt + 1 : 0
    const entry: DecodedPhotoImage = {
      image,
      status: "loading",
      promise: Promise.resolve(image),
      attempt,
      cancel: () => {},
    }

    entry.promise = new Promise<HTMLImageElement>((resolve, reject) => {
      let settled = false
      const finish = (error?: Error) => {
        if (settled) return
        settled = true
        image.onload = null
        image.onerror = null
        entry.status = error ? "failed" : "ready"
        if (error) reject(error)
        else resolve(image)
      }
      image.onload = () => {
        // A completed download can still be waiting for image decoding.
        void Promise.resolve().then(() => image.decode()).then(() => finish(), finish)
      }
      image.onerror = () => finish(new Error("Photo could not be loaded"))
      entry.cancel = () => {
        if (settled) return
        finish(new DOMException("Photo is outside the viewing window", "AbortError"))
        image.removeAttribute("src")
      }
    })

    this.entries.set(src, entry)
    image.src = attempt ? `${src}${src.includes("?") ? "&" : "?"}lightboxRetry=${attempt}` : src
    return entry
  }
}
