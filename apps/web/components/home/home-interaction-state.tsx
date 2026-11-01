"use client"
import { createContext, useContext, useState, type Dispatch, type ReactNode, type SetStateAction } from "react"
const Context = createContext<{
  values: Record<string, unknown>
  set: Dispatch<SetStateAction<Record<string, unknown>>>
} | null>(null)
export function HomeInteractionProvider({ children }: {
  children: ReactNode
}) {
  const [values, set] = useState<Record<string, unknown>>({})
  return <Context.Provider value={{ values, set }}>{children}</Context.Provider>
}
export function useHomeValue<T>(key: string, initial: T): [
  T,
  Dispatch<SetStateAction<T>>
] {
  const context = useContext(Context)
  // Standalone renders also work; the real home always installs the stable provider.
  const [local, setLocal] = useState(initial)
  if(!context)
    return [local, setLocal]
  const value = Object.hasOwn(context.values, key) ? context.values[key] as T : initial
  const set: Dispatch<SetStateAction<T>> = next => context.set(previous => {
    const current = Object.hasOwn(previous, key) ? previous[key] as T : initial
    return { ...previous, [key]: typeof next === "function" ? (next as (value: T) => T)(current) : next }
  })
  return [value, set]
}
