import { BookOpen, Braces, Compass, Cpu, HardDrive, Monitor, Server, Smartphone, Terminal, type LucideIcon, } from "lucide-react"
export function resolveCategoryIcon(iconCssClass: string, name: string): LucideIcon {
  const key = `${iconCssClass} ${name}`.toLowerCase()
  if(/(python|django|flask)/.test(key))
    return Terminal
  if(/(microsoft|asp|dotnet|\.net|csharp|c#)/.test(key))
    return Monitor
  if(/(android|flutter|mobile|kotlin|ios)/.test(key))
    return Smartphone
  if(/(react|vue|js|javascript|typescript|node|前端)/.test(key))
    return Braces
  if(/(hard-drive|backup|保全|zfs|数据)/.test(key))
    return HardDrive
  if(/(starblog|blog|笔记|book)/.test(key))
    return BookOpen
  if(/(server|devops|docker|linux|shell)/.test(key))
    return Server
  if(/(ml|ai|llm|cpu|算法)/.test(key))
    return Cpu
  return Compass
}
export function CategoryIcon({ iconCssClass, name, ...props }: {
  iconCssClass: string
  name: string
} & import("lucide-react").LucideProps) {
  const key = resolveCategoryIcon(iconCssClass, name)
  if(key === Terminal)
    return <Terminal {...props} />
  if(key === Monitor)
    return <Monitor {...props} />
  if(key === Smartphone)
    return <Smartphone {...props} />
  if(key === Braces)
    return <Braces {...props} />
  if(key === HardDrive)
    return <HardDrive {...props} />
  if(key === BookOpen)
    return <BookOpen {...props} />
  if(key === Server)
    return <Server {...props} />
  if(key === Cpu)
    return <Cpu {...props} />
  return <Compass {...props} />
}
