export const site = {
  name: process.env.NEXT_PUBLIC_SITE_NAME ?? "StarBlog",
  description: process.env.NEXT_PUBLIC_SITE_DESCRIPTION ?? "基于 .NET 的开源博客",
  authorName: process.env.NEXT_PUBLIC_AUTHOR_NAME ?? process.env.NEXT_PUBLIC_SITE_NAME ?? "StarBlog",
  authorTitle: process.env.NEXT_PUBLIC_AUTHOR_TITLE ?? "独立开发者",
}

export function getAuthorInitials(name = site.authorName) {
  const parts = name.split(/\s+/).filter(Boolean)
  if (parts.length >= 2) return `${parts[0]![0]}${parts[1]![0]}`.toUpperCase()
  return name.slice(0, 2).toUpperCase()
}
