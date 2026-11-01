"use client"

import { cn } from "@/lib/utils"
import { Dialog as Primitive } from "radix-ui"
import * as React from "react"
import { useHydrated } from "./use-hydrated"

const Dialog = Primitive.Root
function DialogTrigger({ className, ...props }: React.ComponentProps<typeof Primitive.Trigger>) {
  const hydrated = useHydrated()
  return <Primitive.Trigger data-slot="dialog-trigger" className={cn("", className)} {...props} disabled={!hydrated || props.disabled} />
}
const DialogPortal = Primitive.Portal
function DialogClose({ className, ...props }: React.ComponentProps<typeof Primitive.Close>) {
  return <Primitive.Close data-slot="dialog-close" className={cn("", className)} {...props} />
}
function DialogOverlay({ className, ...props }: React.ComponentProps<typeof Primitive.Overlay>) {
  return <Primitive.Overlay data-slot="dialog-overlay" className={cn("fixed inset-0 z-50 bg-black/45 backdrop-blur-[5px]", className)} {...props} />
}
function DialogContent({ className, ...props }: React.ComponentProps<typeof Primitive.Content>) {
  return <Primitive.Content data-slot="dialog-content" className={cn("outline-none", className)} {...props} />
}
function DialogTitle({ className, ...props }: React.ComponentProps<typeof Primitive.Title>) {
  return <Primitive.Title data-slot="dialog-title" className={cn("", className)} {...props} />
}
function DialogDescription({ className, ...props }: React.ComponentProps<typeof Primitive.Description>) {
  return <Primitive.Description data-slot="dialog-description" className={cn("", className)} {...props} />
}
function DialogHeader({ className, ...props }: React.ComponentProps<"div">) { return <div className={cn("flex flex-col gap-2", className)} {...props} /> }
function DialogFooter({ className, ...props }: React.ComponentProps<"div">) { return <div className={cn("flex items-center justify-end gap-2", className)} {...props} /> }
export { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogOverlay, DialogPortal, DialogTitle, DialogTrigger }
