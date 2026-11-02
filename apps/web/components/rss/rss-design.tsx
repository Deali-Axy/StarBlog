"use client"

import type { ComponentType } from "react"
import { useI18n } from "@/components/app-providers"
import { UtilityDesignSwitcher, useUtilityDesign } from "@/components/layout/utility-design-switcher"
import { FeedAddress } from "@/components/rss/feed-address"
import type { RssViewProps } from "@/components/rss/rss-view-props"
import { EditorialRss } from "@/components/rss/views/editorial-rss"
import { FlatRss } from "@/components/rss/views/flat-rss"
import type { UtilityVariant } from "@/lib/utility-variant"

const views = { flat: FlatRss, editorial: EditorialRss } satisfies Record<UtilityVariant, ComponentType<RssViewProps>>

export function RssDesign({ feedUrl }: { feedUrl: string }) {
  const { t } = useI18n()
  const { variant, switchVariant } = useUtilityDesign()
  const View = views[variant]

  return (
    <>
      <UtilityDesignSwitcher value={variant} onChange={switchVariant} />
      <View t={t} feedAddress={<FeedAddress url={feedUrl} />} />
    </>
  )
}
