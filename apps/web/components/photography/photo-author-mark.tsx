import { cn } from "@/lib/utils"
export function AuthorMark({ name, className }: { name: string; className?: string }) {
  return <span aria-hidden className={cn("grid size-7 shrink-0 place-items-center rounded-full bg-muted text-xs font-medium text-foreground", className)}>{Array.from(name.trim())[0] || "?"}</span>
}
