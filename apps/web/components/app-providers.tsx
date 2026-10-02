"use client"

import * as React from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"

import { ThemeProvider } from "@/components/theme-provider"
import { TooltipProvider } from "@/components/ui/tooltip"

export function AppProviders({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  const [queryClient] = React.useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
            staleTime: 30_000,
            refetchOnWindowFocus: false,
          },
        },
      }),
  )

  return (
    <ThemeProvider>
      <TooltipProvider>
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      </TooltipProvider>
    </ThemeProvider>
  )
}
