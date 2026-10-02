export function photoThumbnail(id: string, width = 600) {
  return `/api/v1/photos/${encodeURIComponent(id)}/thumbnail?width=${width}`
}

export function photoOriginal(filePath: string) {
  return `/media/photography/${filePath.replaceAll("\\", "/").replace(/^\/+/, "")}`
}
