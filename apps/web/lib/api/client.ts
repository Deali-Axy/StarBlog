import { getApiBaseUrl } from "@/lib/env"
import { ApiError, type ProblemDetails } from "@/lib/api/types"

interface RequestOptions extends Omit<RequestInit, "body"> {
  query?: Record<string, string | number | boolean | undefined | null>
  body?: unknown
}

function buildUrl(path: string, query?: RequestOptions["query"]) {
  const base = getApiBaseUrl()
  const url = base ? new URL(path, `${base}/`) : new URL(path, "http://local.invalid")
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value === undefined || value === null || value === "") continue
      url.searchParams.set(key, String(value))
    }
  }
  return base ? url.toString() : `${url.pathname}${url.search}`
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { query, body, headers, ...init } = options
  const requestHeaders = new Headers(headers)
  if (body !== undefined && !requestHeaders.has("Content-Type")) {
    requestHeaders.set("Content-Type", "application/json")
  }

  const response = await fetch(buildUrl(path, query), {
    ...init,
    headers: requestHeaders,
    body: body === undefined ? undefined : JSON.stringify(body),
    cache: init.cache,
    next: init.next,
  })

  if (response.status === 204) return undefined as T

  const payload = (await response.json().catch(() => ({}))) as T & ProblemDetails
  if (!response.ok) {
    throw new ApiError(
      response.status,
      payload.detail || payload.title || `请求失败 (${response.status})`,
      payload,
    )
  }
  return payload
}

export function apiGet<T>(path: string, query?: RequestOptions["query"], init?: RequestOptions) {
  return apiRequest<T>(path, { ...init, method: "GET", query })
}

export function apiPost<T>(path: string, body?: unknown, query?: RequestOptions["query"], init?: RequestOptions) {
  return apiRequest<T>(path, { ...init, method: "POST", body, query, cache: "no-store" })
}
