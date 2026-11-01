"use client"

import { cn } from "@/lib/utils"
import { DropdownMenu as Primitive } from "radix-ui"
import * as React from "react"
import { useHydrated } from "./use-hydrated"

const DropdownMenu = Primitive.Root
function DropdownMenuTrigger({ className, ...props }: React.ComponentProps<typeof Primitive.Trigger>) {
  const hydrated = useHydrated()
  return <Primitive.Trigger data-slot="dropdown-menu-trigger" className={cn("", className)} {...props} disabled={!hydrated || props.disabled} />
}
const DropdownMenuPortal = Primitive.Portal
function DropdownMenuContent({ className, ...props }: React.ComponentProps<typeof Primitive.Content>) {
  return <Primitive.Content data-slot="dropdown-menu-content" className={cn("outline-none", className)} {...props} />
}
function DropdownMenuLabel({ className, ...props }: React.ComponentProps<typeof Primitive.Label>) {
  return <Primitive.Label data-slot="dropdown-menu-label" className={cn("", className)} {...props} />
}
function DropdownMenuRadioGroup({ className, ...props }: React.ComponentProps<typeof Primitive.RadioGroup>) {
  return <Primitive.RadioGroup data-slot="dropdown-menu-radiogroup" className={cn("", className)} {...props} />
}
function DropdownMenuRadioItem({ className, ...props }: React.ComponentProps<typeof Primitive.RadioItem>) {
  return <Primitive.RadioItem data-slot="dropdown-menu-radioitem" className={cn("", className)} {...props} />
}
function DropdownMenuItemIndicator({ className, ...props }: React.ComponentProps<typeof Primitive.ItemIndicator>) {
  return <Primitive.ItemIndicator data-slot="dropdown-menu-itemindicator" className={cn("", className)} {...props} />
}
function DropdownMenuItem({ className, ...props }: React.ComponentProps<typeof Primitive.Item>) {
  return <Primitive.Item data-slot="dropdown-menu-item" className={cn("", className)} {...props} />
}
function DropdownMenuSeparator({ className, ...props }: React.ComponentProps<typeof Primitive.Separator>) {
  return <Primitive.Separator data-slot="dropdown-menu-separator" className={cn("", className)} {...props} />
}
export { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuItemIndicator, DropdownMenuLabel, DropdownMenuPortal, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuTrigger }
