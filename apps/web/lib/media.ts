export function photoThumbnail(id: string, width = 600) {
  return `/api/v1/photos/${encodeURIComponent(id)}/thumbnail?width=${width}`
}

export function photoThumbnailSet(id: string) {
  return [320, 480, 640, 800, 1200, 1600, 2000].map((width) => `${photoThumbnail(id, width)} ${width}w`).join(", ")
}

export function photoOriginal(filePath: string) {
  return `/media/photography/${filePath.replaceAll("\\", "/").replace(/^\/+/, "")}`
}
