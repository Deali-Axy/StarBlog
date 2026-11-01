export function pageNumbers(current: number, total: number) {
  if(total <= 5) return Array.from({ length: total }, (_, index) => index + 1)
  const numbers = [...new Set([1, Math.max(2, current - 1), current, Math.min(total - 1, current + 1), total])]
    .filter((page) => page >= 1 && page <= total).sort((a, b) => a - b)
  const pages: Array<number | string> = []
  numbers.forEach((page, index) => {
    if(index && page - numbers[index - 1] > 1) pages.push(`gap-${page}`)
    pages.push(page)
  })
  return pages
}
