"use client"
import type { LinkResponse } from "@/lib/api/types"
import type { ComponentType } from "react"
import { useHomeDesign } from "../home-design-context"
import { CardLinks } from "../views/card-links"
import { DirectoryLinks } from "../views/directory-links"
type Props = {
  links: LinkResponse[]
}
const components = { directory: DirectoryLinks, cards: CardLinks } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["sections"]["links"], ComponentType<Props>>
export function FriendLinks(props: Props) { const { config } = useHomeDesign(); const View = components[config.sections.links]; return <View {...props} /> }
