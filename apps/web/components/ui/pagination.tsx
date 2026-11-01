import { cn } from "@/lib/utils"
import type { ComponentProps } from "react"
export function Pagination({ className, ...props }: ComponentProps<"nav">) { return <nav data-slot="pagination" className={cn("flex flex-wrap items-center justify-between gap-3", className)} {...props} /> }
export function PaginationContent({ className, ...props }: ComponentProps<"div">) { return <div data-slot="pagination-content" className={cn("flex items-center gap-1", className)} {...props} /> }
