"use client"
import { createContext, useContext, useState, type ReactNode } from "react"
import { useBlogNavigation } from "./blog-navigation"
function useInteractions() {
  const { query } = useBlogNavigation()
  const [search, setSearch] = useState({ committed: query.search, draft: query.search ?? "" })
  if(search.committed !== query.search) setSearch({ committed: query.search, draft: query.search ?? "" })
  const [selectedId, setSelectedId] = useState<string | null>(null)
  return { searchDraft: search.draft, setSearchDraft: (draft: string) => setSearch(current => ({ ...current, draft })), selectedId, setSelectedId }
}
const Context = createContext<ReturnType<typeof useInteractions> | null>(null)
export function BlogInteractionProvider({ children }: { children: ReactNode }) { const state = useInteractions(); return <Context.Provider value={state}>{children}</Context.Provider> }
export function useBlogInteractions() { const state = useContext(Context); if(!state) throw new Error("Blog interactions are required"); return state }
