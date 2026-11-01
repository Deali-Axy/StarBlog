"use client"
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogOverlay, DialogPortal, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { cn } from "@/lib/utils"
import { cva } from "class-variance-authority"
import type { ComponentProps } from "react"
const sheetSides = cva("fixed z-50 flex flex-col bg-card text-card-foreground outline-none", { variants: { side: { left: "inset-y-0 left-0 border-r", right: "inset-y-0 right-0 border-l", top: "inset-x-0 top-0 border-b", bottom: "inset-x-0 bottom-0 border-t" } }, defaultVariants: { side: "right" } })
function SheetContent({ side = "right", className, ...props }: ComponentProps<typeof DialogContent> & { side?: "left" | "right" | "top" | "bottom" }) {
  return <DialogContent data-slot="sheet-content" className={cn(sheetSides({ side }), className)} {...props} />
}
export { Dialog as Sheet, DialogClose as SheetClose, SheetContent, DialogDescription as SheetDescription, DialogOverlay as SheetOverlay, DialogPortal as SheetPortal, DialogTitle as SheetTitle, DialogTrigger as SheetTrigger }
