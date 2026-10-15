"use client"

import * as React from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"

import { ThemeProvider } from "@/components/theme-provider"
import { TooltipProvider } from "@/components/ui/tooltip"
import { messages, type Locale } from "@/lib/i18n"

const LocaleContext = React.createContext<Locale>("zh")

export function useI18n() {
  const locale = React.useContext(LocaleContext)
  return { locale, t: messages[locale] }
}

export function AppProviders({
  children,
  locale,
}: Readonly<{
  children: React.ReactNode
  locale: Locale
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
    <LocaleContext.Provider value={locale}>
      <ThemeProvider>
        <TooltipProvider>
          <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
        </TooltipProvider>
      </ThemeProvider>
    </LocaleContext.Provider>
  )
}
