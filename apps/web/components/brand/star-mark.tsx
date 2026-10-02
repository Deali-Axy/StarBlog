import { cn } from "@/lib/utils"

export function StarMark({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" className={cn("size-4 fill-current", className)} aria-hidden="true">
      <path d="M12 2.2l1.35 7.1 7.15.7-5.5 4.55 1.7 7.05L12 17.8 7.3 21.6l1.7-7.05L3.5 10l7.15-.7z" />
    </svg>
  )
}
