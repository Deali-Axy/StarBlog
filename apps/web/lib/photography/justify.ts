export interface JustifiedBox {
  index: number
  x: number
  y: number
  width: number
  height: number
}

interface RowItem {
  index: number
  aspect: number
}

/** Keep extreme frames from blowing a row into a strip or a tower. */
function clampAspect(value: number) {
  if (!Number.isFinite(value) || value <= 0) return 1.5
  return Math.min(3.2, Math.max(0.45, value))
}

export function photoAspect(width: number, height: number) {
  if (width > 0 && height > 0) return clampAspect(width / height)
  return 1.5
}

export function targetRowHeight(containerWidth: number) {
  if (containerWidth < 700) return 188
  if (containerWidth < 1100) return 240
  return 292
}

/**
 * Flickr-style justified rows: each full row shares one height and fills the
 * container. A photo is kept for the next row when adding it would make the
 * current row much shorter than the target. The last row stays at the target
 * height instead of stretching a single frame across the page.
 */
export function layoutJustified(
  aspects: number[],
  containerWidth: number,
  targetHeight: number,
  gap: number,
): { boxes: JustifiedBox[]; height: number } {
  if (containerWidth <= gap || targetHeight <= 0 || aspects.length === 0) {
    return { boxes: [], height: 0 }
  }

  const boxes: JustifiedBox[] = []
  let row: RowItem[] = []
  let y = 0

  function fittedHeight(items: RowItem[]) {
    const sum = items.reduce((total, item) => total + item.aspect, 0)
    const available = containerWidth - gap * (items.length - 1)
    if (available <= 0 || sum <= 0) return targetHeight
    return available / sum
  }

  function commit(items: RowItem[], fit: boolean) {
    if (items.length === 0) return
    const height = fit ? fittedHeight(items) : targetHeight
    let x = 0
    items.forEach((item, index) => {
      const natural = item.aspect * height
      const last = index === items.length - 1
      const width = fit && last ? Math.max(1, containerWidth - x) : natural
      boxes.push({ index: item.index, x, y, width, height })
      x += width + gap
    })
    y += height + gap
  }

  for (let index = 0; index < aspects.length; index += 1) {
    const candidate: RowItem[] = [...row, { index, aspect: clampAspect(aspects[index] ?? 0) }]
    const height = fittedHeight(candidate)
    if (candidate.length > 1 && height < targetHeight * 0.9) {
      const previousHeight = fittedHeight(row)
      commit(row, previousHeight <= targetHeight * 1.4)
      row = [{ index, aspect: clampAspect(aspects[index] ?? 0) }]
      if (fittedHeight(row) <= targetHeight) {
        commit(row, true)
        row = []
      }
      continue
    }
    row = candidate
    if (height <= targetHeight) {
      commit(row, true)
      row = []
    }
  }

  if (row.length > 0) commit(row, false)
  return { boxes, height: Math.max(0, y - gap) }
}
