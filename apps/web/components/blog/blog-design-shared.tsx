import type { CategoryNodeResponse, PageResult, PostResponse } from "@/lib/api/types"
import type { CategoryTreeState } from "./category-tree"
export interface BlogDesignProps {
  categories: CategoryNodeResponse[]
  posts: PageResult<PostResponse>
  path: CategoryNodeResponse[]
  title: string
  treeState: CategoryTreeState
}
