"use client"
import type { PostResponse } from "@/lib/api/types"
import type { ComponentType } from "react"
import { useHomeDesign } from "../home-design-context"
import { LedgerLatestPosts } from "../views/ledger-latest-posts"
import { TimelineLatestPosts } from "../views/timeline-latest-posts"
type Props = {
  posts: PostResponse[]
}
const components = { timeline: TimelineLatestPosts, ledger: LedgerLatestPosts } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["sections"]["latestPosts"], ComponentType<Props>>
export function LatestPosts(props: Props) { const { config } = useHomeDesign(); const View = components[config.sections.latestPosts]; return <View {...props} /> }
