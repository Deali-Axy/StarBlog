export function linkDomain(href: string) {
  try { return new URL(href).hostname.replace(/^www\./, "") } catch { return href }
}
