export const site = {
  name: process.env.NEXT_PUBLIC_SITE_NAME ?? "StarBlog",
  description: process.env.NEXT_PUBLIC_SITE_DESCRIPTION ?? "基于 .NET 的开源博客",
  url: (process.env.NEXT_PUBLIC_SITE_URL ?? "https://blog.deali.cn").replace(/\/$/, ""),
  authorName: process.env.NEXT_PUBLIC_AUTHOR_NAME ?? "曦远 (DealiAxy)",
  authorTitle: process.env.NEXT_PUBLIC_AUTHOR_TITLE ?? "独立开发者",
  authorBio:
    process.env.NEXT_PUBLIC_AUTHOR_BIO ?? "全干开发者、Linux中度依赖者、业余摄影玩家、文学爱好者",
  githubUrl: process.env.NEXT_PUBLIC_GITHUB_URL ?? "https://github.com/Deali-Axy",
  zhihuUrl: process.env.NEXT_PUBLIC_ZHIHU_URL ?? "https://www.zhihu.com/people/dealiaxy",
  cnblogsUrl: process.env.NEXT_PUBLIC_CNBLOGS_URL ?? "https://www.cnblogs.com/deali/",
  labName: process.env.NEXT_PUBLIC_LAB_NAME ?? "程序设计实验室",
  labDescription:
    process.env.NEXT_PUBLIC_LAB_DESCRIPTION ??
    "专注于互联网热门新技术探索与团队敏捷开发实践，包括Web前后端、移动端开发、机器学习、数据分析、算法、Linux等技术，欢迎探讨、分享学习实践经验。",
  wechatQr: "/qrcode_sm.jpg",
  labImage: "/codelab.jpg",
  skills: [
    { label: "C#", keyword: "C#" },
    { label: "Python", keyword: "Python" },
    { label: "React", keyword: "React" },
    { label: "Flutter", keyword: "Flutter" },
    { label: "Kotlin", keyword: "Kotlin" },
    { label: "Go", keyword: "Go语言" },
    { label: "LLM", keyword: "LLM" },
  ],
}

export function getAuthorInitials(name = site.authorName) {
  const parts = name.split(/\s+/).filter(Boolean)
  if (parts.length >= 2) return `${parts[0]![0]}${parts[1]![0]}`.toUpperCase()
  return name.slice(0, 2).toUpperCase()
}

export function absoluteUrl(path = "/") {
  if (path.startsWith("http://") || path.startsWith("https://")) return path
  return `${site.url}${path.startsWith("/") ? path : `/${path}`}`
}
