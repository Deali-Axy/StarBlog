"use client"
import type { PhotoResponse } from "@/lib/api/types"
import type { ComponentType } from "react"
import { useHomeDesign } from "../home-design-context"
import { ContactSheetPhotos } from "../views/contact-sheet-photos"
import { PrintsPhotos } from "../views/prints-photos"
type Props = {
  photos: PhotoResponse[]
}
const components = { contactSheet: ContactSheetPhotos, prints: PrintsPhotos } satisfies Record<ReturnType<typeof useHomeDesign>["config"]["sections"]["photos"], ComponentType<Props>>
export function FeaturedPhotos(props: Props) { const { config } = useHomeDesign(); const View = components[config.sections.photos]; return <View {...props} /> }
