"use client"
import type { PublishedPostSummary } from "@/lib/api/types"
import type { ComponentType } from "react"
import { useHomeDesign } from "../home-design-context"
import { EditorialFeaturedPosts } from "../views/editorial-featured-posts"
import { NotesFeaturedPosts } from "../views/notes-featured-posts"
type Props = {
  posts: PublishedPostSummary[]
}
const components = { editorial: EditorialFeaturedPosts, notes: NotesFeaturedPosts } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["sections"]["featuredPosts"], ComponentType<Props>>
export function FeaturedPosts(props: Props) { const { config } = useHomeDesign(); const View = components[config.sections.featuredPosts]; return <View {...props} /> }
