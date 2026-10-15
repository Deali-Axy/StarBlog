import { routes } from "@/lib/routes"

export function isPhotographyPath(pathname: string) {
  return pathname === routes.photography || pathname.startsWith(`${routes.photography}/`)
}

const hiddenLocations = new Set(["unknown", "null", "n/a", "na", "未知", "未知地点"])

export function photoLocation(location?: string | null) {
  const value = location?.trim() ?? ""
  if (!value || hiddenLocations.has(value.toLowerCase())) return ""
  return value
}
