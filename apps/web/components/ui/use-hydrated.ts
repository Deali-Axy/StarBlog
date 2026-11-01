"use client"
import { useSyncExternalStore } from "react"
const subscribe = () => () => { }
/** Keep client-only actions unavailable until their event handlers are attached. */
export function useHydrated() { return useSyncExternalStore(subscribe, () => true, () => false) }
