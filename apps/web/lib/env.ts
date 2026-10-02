function trimSlash(value: string) {
  return value.replace(/\/$/, "")
}

/** 服务端直连 API；浏览器走相对路径，由 Next rewrite 转发。 */
export function getApiBaseUrl() {
  const internal = process.env.INTERNAL_API_BASE_URL?.trim()
  if (internal) return trimSlash(internal)
  if (typeof window === "undefined") {
    return trimSlash(process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://127.0.0.1:5039")
  }
  return ""
}
