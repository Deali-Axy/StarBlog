import type { ReactNode } from "react"
import type { messages } from "@/lib/i18n"

export type RssViewProps = {
  t: (typeof messages)["zh"]
  feedAddress: ReactNode
}
