"use client"

import type { ComponentType } from "react"
import { useI18n } from "@/components/app-providers"
import { UtilityDesignSwitcher, useUtilityDesign } from "@/components/layout/utility-design-switcher"
import { LinkApplyForm } from "@/components/links/link-apply-form"
import type { LinksViewProps } from "@/components/links/links-view-props"
import { useLinkApplication } from "@/components/links/use-link-application"
import { EditorialLinks } from "@/components/links/views/editorial-links"
import { FlatLinks } from "@/components/links/views/flat-links"
import type { LinkResponse } from "@/lib/api/types"
import { getSite } from "@/lib/site"
import type { UtilityVariant } from "@/lib/utility-variant"

const views = { flat: FlatLinks, editorial: EditorialLinks } satisfies Record<UtilityVariant, ComponentType<LinksViewProps>>

export function LinksDesign({ links }: { links: LinkResponse[] }) {
  const { locale, t } = useI18n()
  const { variant, switchVariant } = useUtilityDesign()
  // The controller outlives either presentation, preserving drafts and pending requests.
  const application = useLinkApplication()
  const View = views[variant]

  return (
    <>
      <UtilityDesignSwitcher value={variant} onChange={switchVariant} />
      <View links={links} site={getSite(locale)} t={t} form={<LinkApplyForm application={application} submitLabel={variant === "flat" ? t.submit : t.linksPage.submit} />} />
    </>
  )
}
