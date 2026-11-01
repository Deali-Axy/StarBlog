import type { PageResult, PostResponse } from "@/lib/api/types"
import type { KeyboardEvent, RefObject } from "react"
export interface BlogCollectionProps { posts: PageResult<PostResponse>; selected?: PostResponse; rowRefs?: RefObject<Map<string, HTMLButtonElement>>; select?: (post: PostResponse, trigger: HTMLButtonElement) => void; keyboardSelect?: (event: KeyboardEvent<HTMLButtonElement>, index: number) => void }
