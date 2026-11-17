"use client"

import { useSyncExternalStore } from "react"

export type CoverMode = "single" | "collage"
const key = "starblog-cover-mode"
const changed = "starblog:cover-mode"
let memory: CoverMode = "collage"

function snapshot(): CoverMode {
  try {
    const value = window.localStorage.getItem(key)
    return value === "collage" ? "collage" : value === "single" ? "single" : memory
  } catch { return memory }
}
function subscribe(listener: () => void) {
  window.addEventListener("storage", listener)
  window.addEventListener(changed, listener)
  return () => {
    window.removeEventListener("storage", listener)
    window.removeEventListener(changed, listener)
  }
}

export function useCoverMode() {
  const coverMode = useSyncExternalStore(subscribe, snapshot, (): CoverMode => "collage")
  function setCoverMode(value: CoverMode) {
    memory = value
    try { window.localStorage.setItem(key, value) } catch { /* The current tab can still switch without storage. */ }
    window.dispatchEvent(new Event(changed))
  }
  return { coverMode, setCoverMode }
}
