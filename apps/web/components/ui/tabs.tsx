"use client"

import { cn } from "@/lib/utils"
import { Tabs as Primitive } from "radix-ui"
import * as React from "react"
import { useHydrated } from "./use-hydrated"

const Tabs = Primitive.Root
function TabsList({ className, ...props }: React.ComponentProps<typeof Primitive.List>) {
  return <Primitive.List data-slot="tabs-list" className={cn("", className)} {...props} />
}
function TabsTrigger({ className, ...props }: React.ComponentProps<typeof Primitive.Trigger>) {
  const hydrated = useHydrated()
  return <Primitive.Trigger data-slot="tabs-trigger" className={cn("", className)} {...props} disabled={!hydrated || props.disabled} />
}
function TabsContent({ className, ...props }: React.ComponentProps<typeof Primitive.Content>) {
  return <Primitive.Content data-slot="tabs-content" className={cn("outline-none", className)} {...props} />
}
export { Tabs, TabsContent, TabsList, TabsTrigger }
