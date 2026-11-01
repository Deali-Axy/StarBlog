"use client"
import { homeThemeStyle } from "@/lib/home-theme"
import { homeDesignHref, homePreferencesCookie, normalizeHomePreferences, resolveHomeConfig, resolveHomePreferences, serializeHomePreferences, setHomeSetting, type HomeMode, type HomePreferences, type HomeSettingPath } from "@/lib/home-variant"
import { useTheme } from "next-themes"
import { useSearchParams } from "next/navigation"
import { createContext, useContext, useEffect, useState, useSyncExternalStore, type ReactNode } from "react"
import styles from "./home-design.module.css"
import { HomeInteractionProvider } from "./home-interaction-state"
const changeEvent = "starblog:home-design"
function subscribe(callback: () => void) {
  window.addEventListener(changeEvent, callback)
  window.addEventListener("pageshow", callback)
  return () => { window.removeEventListener(changeEvent, callback); window.removeEventListener("pageshow", callback) }
}
function readSavedPreferences() {
  return document.cookie.split("; ").find(cookie => cookie.startsWith(`${homePreferencesCookie}=`))?.slice(homePreferencesCookie.length + 1) ?? ""
}
type Session = {
  base: HomePreferences
  draft: HomePreferences
  mode: HomeMode
  originalMode: HomeMode
}
function useDesignState(savedPreferences: string) {
  const params = useSearchParams()
  const saved = useSyncExternalStore(subscribe, readSavedPreferences, () => savedPreferences)
  const committed = resolveHomePreferences({ hc: params.get("hc"), hv: params.get("hv"), fc: params.get("fc") }, saved)
  const { theme, resolvedTheme, systemTheme, setTheme } = useTheme()
  const globalMode: HomeMode = theme === "system" || theme === "light" ? theme : "dark"
  const [session, setSession] = useState<Session | null>(null)
  useEffect(() => {
    const cancel = () => setSession(null)
    window.addEventListener("popstate", cancel)
    window.addEventListener("pageshow", cancel)
    return () => { window.removeEventListener("popstate", cancel); window.removeEventListener("pageshow", cancel) }
  }, [])
  const preferences = session?.draft ?? committed
  const config = resolveHomeConfig(preferences)
  const mode = session?.mode ?? globalMode
  const previewMode = session ? mode === "system" ? systemTheme ?? resolvedTheme ?? "light" : mode : undefined
  const dirty = Boolean(session && (serializeHomePreferences(session.base) !== serializeHomePreferences(preferences) || session.mode !== session.originalMode))
  const canApply = Boolean(session && (serializeHomePreferences(resolveHomePreferences({}, saved)) !== serializeHomePreferences(preferences) || dirty || !saved.startsWith("v2.")))
  function begin() { setSession({ base: committed, draft: committed, mode: globalMode, originalMode: globalMode }) }
  function setSetting(path: HomeSettingPath, value?: unknown) {
    setSession(current => current ? { ...current, draft: setHomeSetting(current.draft, path, value) } : current)
  }
  function selectPreset(preset: HomePreferences["preset"]) {
    setSession(current => current ? { ...current, draft: normalizeHomePreferences({ preset }) } : current)
  }
  function resetAll() { selectPreset(preferences.preset) }
  function undo() { setSession(current => current ? { ...current, draft: current.base, mode: current.originalMode } : current) }
  function changeMode(value: HomeMode) {
    if(session)
      setSession({ ...session, mode: value })
    else
      setTheme(value)
  }
  function apply() {
    if(!session)
      return
    const next = serializeHomePreferences(preferences)
    if(next !== serializeHomePreferences(committed)) {
      const url = window.location.href
      window.history.replaceState(window.history.state, "", homeDesignHref(url, committed))
      window.history.pushState(window.history.state, "", homeDesignHref(url, preferences))
    }
    document.cookie = `${homePreferencesCookie}=${next}; Path=/; Max-Age=31536000; SameSite=Lax${location.protocol === "https:" ? "; Secure" : ""}`
    if(session.mode !== globalMode)
      setTheme(session.mode)
    setSession(null)
    window.dispatchEvent(new Event(changeEvent))
  }
  return { preferences, config, mode, previewMode, effectiveMode: previewMode ?? resolvedTheme, open: Boolean(session), dirty, canApply, begin, cancel: () => setSession(null), setSetting, selectPreset, resetAll, undo, changeMode, apply }
}
const HomeDesignContext = createContext<ReturnType<typeof useDesignState> | null>(null)
export function useHomeDesign() {
  const context = useContext(HomeDesignContext)
  if(!context)
    throw new Error("Home design provider is required")
  return context
}
export function HomeDesignProvider({ savedPreferences, children }: {
  savedPreferences: string
  children: ReactNode
}) {
  const state = useDesignState(savedPreferences)
  return <HomeDesignContext.Provider value={state}>
    <div className={`${styles.theme} min-h-dvh overflow-x-clip ${styles.page}`} style={homeThemeStyle(state.config)} data-home-design={state.preferences.preset} data-home-mode={state.previewMode} data-home-texture={state.config.theme.texture} data-home-border={state.config.theme.border} data-home-shadow={state.config.theme.shadow}>
      <HomeInteractionProvider>{children}</HomeInteractionProvider>
    </div>
  </HomeDesignContext.Provider>
}
