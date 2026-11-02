// Profile and project references: deali-home-fumadocs personal site.
// Keep links here; translated presentation copy lives in the shared dictionary.
export const aboutProfile = {
  email: "contact@deali.cn",
  website: "https://deali.cn",
  avatar: "/about/avatar.webp",
  channels: [
    { id: "code", name: "曦远Code", qr: "/about/wechat-code.jpg" },
    { id: "life", name: "曦远Life", qr: "/about/wechat-life.jpg" },
  ],
} as const

export const aboutProjects = [
  {
    id: "starblog",
    name: "StarBlog",
    href: "https://github.com/Deali-Axy/StarBlog",
    stack: "C# / ASP.NET Core / Next.js",
  },
  {
    id: "djangoStarter",
    name: "DjangoStarter",
    href: "https://github.com/Deali-Axy/DjangoStarter",
    stack: "Python / Django / Django Ninja",
  },
  {
    id: "imageGlider",
    name: "ImageGlider",
    href: "https://github.com/iugamlabs/image-glider",
    stack: "C# / .NET / ImageSharp",
  },
] as const

export const aboutToolkit = [
  { id: "web", skills: ["React", "Vue", "Next.js", "TypeScript"] },
  { id: "backend", skills: ["C#", "Python", "Go", "ASP.NET Core", "Django"] },
  { id: "ai", skills: ["LLM", "RAG", "Agent", "MCP", "PyTorch"] },
  { id: "systems", skills: ["Linux", "Docker", "PostgreSQL", "Flutter", "Tauri"] },
] as const
