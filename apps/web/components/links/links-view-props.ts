import type { ReactNode } from "react"
import type { LinkResponse } from "@/lib/api/types"
import type { messages } from "@/lib/i18n"
import type { getSite } from "@/lib/site"

export type LinksViewProps = {
  links: LinkResponse[]
  site: ReturnType<typeof getSite>
  t: (typeof messages)["zh"]
  form: ReactNode
}
