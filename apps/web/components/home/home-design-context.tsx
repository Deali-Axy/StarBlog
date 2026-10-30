"use client"

import { createContext, useContext, useSyncExternalStore, type ReactNode } from "react"
import { useSearchParams } from "next/navigation"

import { homeDesignHref, homePreferencesCookie, resolveHomePreferences, serializeHomePreferences, type HomePreferences } from "@/lib/home-variant"

const changeEvent = "starblog:home-design"

function subscribe(callback: () => void) {
  window.addEventListener(changeEvent, callback)
  window.addEventListener("pageshow", callback)
  return () => {
    window.removeEventListener(changeEvent, callback)
    window.removeEventListener("pageshow", callback)
  }
}

function readSavedPreferences() {
  return document.cookie.split("; ").find(cookie => cookie.startsWith(`${homePreferencesCookie}=`))?.slice(homePreferencesCookie.length + 1) ?? ""
}

const HomeDesignContext = createContext<(HomePreferences & {
  update: (patch: Partial<HomePreferences>) => void
}) | null>(null)

export function useHomeDesign() {
  const context = useContext(HomeDesignContext)
  if (!context) throw new Error("Home design provider is required")
  return context
}

export function HomeDesignProvider({ savedPreferences, children }: { savedPreferences: string; children: ReactNode }) {
  const params = useSearchParams()
  const saved = useSyncExternalStore(subscribe, readSavedPreferences, () => savedPreferences)
  const preferences = resolveHomePreferences({ hv: params.get("hv"), fc: params.get("fc") }, saved)

  function update(patch: Partial<HomePreferences>) {
    const next = { ...preferences, ...patch }
    if (serializeHomePreferences(next) === serializeHomePreferences(preferences)) return
    // Make the first history entry explicit before changing the saved fallback.
    const url = window.location.href
    window.history.replaceState(null, "", homeDesignHref(url, preferences))
    document.cookie = `${homePreferencesCookie}=${serializeHomePreferences(next)}; Path=/; Max-Age=31536000; SameSite=Lax${location.protocol === "https:" ? "; Secure" : ""}`
    window.history.pushState(null, "", homeDesignHref(url, next))
    window.dispatchEvent(new Event(changeEvent))
  }

  return <HomeDesignContext.Provider value={{ ...preferences, update }}>
    <div data-home-design={preferences.variant} className="min-w-0">{children}</div>
  </HomeDesignContext.Provider>
}
