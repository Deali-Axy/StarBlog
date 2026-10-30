import type { CategoryNodeResponse } from "@/lib/api/types"

export function categoryPath(nodes: CategoryNodeResponse[], id?: number): CategoryNodeResponse[] {
  if (id === undefined) return []
  for (const node of nodes) {
    if (node.id === id) return [node]
    const path = categoryPath(node.nodes ?? [], id)
    if (path.length) return [node, ...path]
  }
  return []
}

/** Keep matching names and their ancestors; never change the original tree. */
export function filterCategories(nodes: CategoryNodeResponse[], search: string): CategoryNodeResponse[] {
  const term = search.trim().toLocaleLowerCase()
  if (!term) return nodes
  return nodes.flatMap((node) => {
    const children = filterCategories(node.nodes ?? [], term)
    return node.text.toLocaleLowerCase().includes(term) || children.length
      ? [{ ...node, nodes: children }]
      : []
  })
}
