"use client"
import type { CategoryWordCloudItem } from "@/lib/api/types"
import type { ComponentType } from "react"
import { useHomeDesign } from "../home-design-context"
import { CloudInterests } from "../views/cloud-interests"
import { TagInterests } from "../views/tag-interests"
type Props = {
  items: CategoryWordCloudItem[]
}
const components = { cloud: CloudInterests, tags: TagInterests } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["sections"]["interests"], ComponentType<Props>>
export function CategoryWordCloud(props: Props) { const { config } = useHomeDesign(); const View = components[config.sections.interests]; return <View {...props} /> }
