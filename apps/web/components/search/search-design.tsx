"use client"

import type { ComponentType } from "react"
import { useI18n } from "@/components/app-providers"
import { UtilityDesignSwitcher, useUtilityDesign } from "@/components/layout/utility-design-switcher"
import { SearchForm } from "@/components/search/search-form"
import type { SearchPageData, SearchViewProps } from "@/components/search/search-view-props"
import { useArticleSearch, type SearchFormProps } from "@/components/search/use-article-search"
import { EditorialSearch } from "@/components/search/views/editorial-search"
import { FlatSearch } from "@/components/search/views/flat-search"
import { FlatSearchForm } from "@/components/search/views/flat-search-form"
import { searchPageHref } from "@/lib/search-query"
import type { UtilityVariant } from "@/lib/utility-variant"

const views = {
  flat: { Page: FlatSearch, Form: FlatSearchForm },
  editorial: { Page: EditorialSearch, Form: SearchForm },
} satisfies Record<UtilityVariant, { Page: ComponentType<SearchViewProps>; Form: ComponentType<SearchFormProps> }>

export function SearchDesign({ data }: { data: SearchPageData }) {
  const { t } = useI18n()
  const { variant, switchVariant } = useUtilityDesign()
  // Keep input and navigation state above either page and form presentation.
  const search = useArticleSearch(data.keyword, variant)
  const { Page, Form } = views[variant]

  return (
    <>
      <UtilityDesignSwitcher value={variant} onChange={switchVariant} />
      <Page {...data} t={t}
        pageHref={(page) => searchPageHref(data.keyword, page, variant)}
        keywordHref={(keyword) => searchPageHref(keyword, 1, variant)}
        form={<Form search={search} hiddenFields={variant === "editorial" ? <input type="hidden" name="uv" value="editorial" /> : null} />}
      />
    </>
  )
}
