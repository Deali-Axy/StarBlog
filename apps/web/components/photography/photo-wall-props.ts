import type { PhotoResponse } from "@/lib/api/types"
import type { PhotoDensity } from "@/lib/photography/layout"
export interface PhotoWallProps { photos: PhotoResponse[]; aspects: number[]; frame: { width: number; viewport: number }; density: PhotoDensity; featuredIds: ReadonlySet<string>; authorName: string; onOpen: (id?: string) => void; onLayout: () => void; onDimensions: (id: string, width: number, height: number) => void; empty: string }
