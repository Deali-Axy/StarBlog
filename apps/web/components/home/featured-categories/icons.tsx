import {
  BookOpen,
  Braces,
  Compass,
  Cpu,
  HardDrive,
  Monitor,
  Server,
  Smartphone,
  Terminal,
  type LucideIcon,
} from "lucide-react"

export function resolveCategoryIcon(iconCssClass: string, name: string): LucideIcon {
  const key = `${iconCssClass} ${name}`.toLowerCase()
  if (/(python|django|flask)/.test(key)) return Terminal
  if (/(microsoft|asp|dotnet|\.net|csharp|c#)/.test(key)) return Monitor
  if (/(android|flutter|mobile|kotlin|ios)/.test(key)) return Smartphone
  if (/(react|vue|js|javascript|typescript|node|前端)/.test(key)) return Braces
  if (/(hard-drive|backup|保全|zfs|数据)/.test(key)) return HardDrive
  if (/(starblog|blog|笔记|book)/.test(key)) return BookOpen
  if (/(server|devops|docker|linux|shell)/.test(key)) return Server
  if (/(ml|ai|llm|cpu|算法)/.test(key)) return Cpu
  return Compass
}
