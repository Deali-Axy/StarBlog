export type PhotoView = "masonry" | "justified"
export type PhotoDensity = "comfortable" | "standard" | "compact"

export interface PhotoBox {
  index: number
  x: number
  y: number
  width: number
  height: number
}

export function photoColumns(viewportWidth: number, density: PhotoDensity) {
  const standard = viewportWidth < 640 ? 2 : viewportWidth < 1024 ? 3 : viewportWidth < 1536 ? 4 : 5
  const offset = density === "comfortable" ? -1 : density === "compact" ? 1 : 0
  return Math.max(1, Math.min(viewportWidth < 640 ? 2 : 6, standard + offset))
}

export function densityScale(density: PhotoDensity) {
  return density === "comfortable" ? 1.3 : density === "compact" ? 0.75 : 1
}

export function layoutMasonry(aspects: number[], width: number, columns: number, gap: number) {
  if (width <= 0 || columns < 1 || aspects.length === 0) return { boxes: [], height: 0 }
  const count = Math.max(1, Math.min(Math.floor(columns), Math.floor((width + gap) / (gap + 1))))
  const columnWidth = (width - gap * (count - 1)) / count
  const heights = Array<number>(count).fill(0)
  const boxes: PhotoBox[] = aspects.map((aspect, index) => {
    const column = heights.indexOf(Math.min(...heights))
    const height = columnWidth / (Number.isFinite(aspect) && aspect > 0 ? aspect : 1.5)
    const box = { index, x: column * (columnWidth + gap), y: heights[column], width: columnWidth, height }
    heights[column] += height + gap
    return box
  })
  return { boxes, height: Math.max(0, ...heights) - gap }
}
