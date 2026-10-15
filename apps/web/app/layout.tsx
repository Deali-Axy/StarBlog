import { Newsreader, Noto_Sans_SC, Noto_Serif_SC } from "next/font/google"
import type { Metadata } from "next"

import "./globals.css"
import { AppProviders } from "@/components/app-providers"
import { SiteFooter } from "@/components/layout/site-footer"
import { SiteHeader } from "@/components/layout/site-header"
import { Toaster } from "@/components/ui/sonner"
import { absoluteUrl, getSite } from "@/lib/site"
import { htmlLang } from "@/lib/i18n"
import { getI18n } from "@/lib/i18n-server"
import { cn } from "@/lib/utils"

const sans = Noto_Sans_SC({
  subsets: ["latin"],
  weight: ["400", "500", "700"],
  variable: "--font-sans",
  display: "swap",
})

const serif = Noto_Serif_SC({
  subsets: ["latin"],
  weight: ["600"],
  variable: "--font-serif-sc",
  display: "swap",
})

const newsreader = Newsreader({
  subsets: ["latin"],
  weight: ["500", "600"],
  variable: "--font-newsreader",
  display: "swap",
})

export async function generateMetadata(): Promise<Metadata> {
  const { locale } = await getI18n()
  const site = getSite(locale)
  return {
    metadataBase: new URL(site.url),
    title: {
      default: site.name,
      template: `%s · ${site.name}`,
    },
    description: site.description,
    alternates: {
      canonical: "/",
      types: {
        "application/atom+xml": "/feed",
      },
    },
    openGraph: {
      type: "website",
      locale: locale === "en" ? "en_US" : "zh_CN",
      siteName: site.name,
      title: site.name,
      description: site.description,
      url: absoluteUrl("/"),
    },
    twitter: {
      card: "summary_large_image",
      title: site.name,
      description: site.description,
    },
  }
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  const { locale } = await getI18n()
  return (
    <html
      lang={htmlLang[locale]}
      suppressHydrationWarning
      className={cn("antialiased font-sans", sans.variable, serif.variable, newsreader.variable)}
    >
      <body>
        <AppProviders locale={locale}>
          <div className="flex min-h-screen flex-col">
            <SiteHeader />
            <main className="flex-1">{children}</main>
            <SiteFooter />
          </div>
        </AppProviders>
        <Toaster richColors position="top-center" />
      </body>
    </html>
  )
}
