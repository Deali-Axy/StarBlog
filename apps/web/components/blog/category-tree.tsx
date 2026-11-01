import type { CategoryNodeResponse } from "@/lib/api/types"
export interface CategoryTreeState {
  expanded: Set<number>
  search: string
  setSearch: (search: string) => void
  toggle: (id: number) => void
}

export interface CategoryTreeProps {
  categoryNodes: CategoryNodeResponse[]
  state: CategoryTreeState
  onSelect?: () => void
  className?: string
}
