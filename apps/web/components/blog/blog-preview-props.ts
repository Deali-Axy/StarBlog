import type { BlogVariant } from "@/lib/blog-variant"
export interface BlogPreviewProps { value: BlogVariant; onValueChange: (value: BlogVariant) => void; onCloseAutoFocus: (event: Event) => void }
