import dayjs from "dayjs"

export function formatDate(value?: string | null, pattern = "YYYY-MM-DD") {
  if (!value) return ""
  return dayjs(value).format(pattern)
}

export function readingMinutes(content?: string | null) {
  const length = content?.length ?? 0
  return Math.max(1, Math.ceil(length / 400))
}

const relativeUnits: [number, Intl.RelativeTimeFormatUnit, number][] = [
  [60, "second", 1],
  [3600, "minute", 60],
  [86400, "hour", 3600],
  [86400 * 30, "day", 86400],
  [86400 * 365, "month", 86400 * 30],
  [Number.POSITIVE_INFINITY, "year", 86400 * 365],
]

export function formatRelativeTime(value: string | null | undefined, locale: "zh" | "en") {
  if (!value) return ""
  const then = new Date(value).getTime()
  if (Number.isNaN(then)) return ""
  const deltaSeconds = Math.round((then - Date.now()) / 1000)
  const abs = Math.abs(deltaSeconds)
  const rtf = new Intl.RelativeTimeFormat(locale === "zh" ? "zh-CN" : "en", { numeric: "auto" })
  for (const [limit, unit, scale] of relativeUnits) {
    if (abs < limit) return rtf.format(Math.round(deltaSeconds / scale), unit)
  }
  return rtf.format(Math.round(deltaSeconds / (86400 * 365)), "year")
}
