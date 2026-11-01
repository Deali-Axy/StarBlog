"use client"

import { cn } from "@/lib/utils"
import { Collapsible as Primitive } from "radix-ui"
import * as React from "react"
import { useHydrated } from "./use-hydrated"

const Collapsible = Primitive.Root
function CollapsibleTrigger({ className, ...props }: React.ComponentProps<typeof Primitive.Trigger>) {
  const hydrated = useHydrated()
  return <Primitive.Trigger data-slot="collapsible-trigger" className={cn("", className)} {...props} disabled={!hydrated || props.disabled} />
}
function CollapsibleContent({ className, ...props }: React.ComponentProps<typeof Primitive.Content>) {
  return <Primitive.Content data-slot="collapsible-content" className={cn("outline-none", className)} {...props} />
}
export { Collapsible, CollapsibleContent, CollapsibleTrigger }
