import dayjs from "dayjs"

export function formatDate(value?: string | null, pattern = "YYYY-MM-DD") {
  if (!value) return ""
  return dayjs(value).format(pattern)
}

export function readingMinutes(content?: string | null) {
  const length = content?.length ?? 0
  return Math.max(1, Math.ceil(length / 400))
}
