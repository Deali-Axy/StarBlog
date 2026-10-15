import { getI18n } from "@/lib/i18n-server"
import Link from "next/link"
import type { Metadata } from "next"
import { Search } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { searchPosts } from "@/lib/api/queries"
import { formatDate } from "@/lib/format"
import { postHref, routes } from "@/lib/routes"

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.search }
}

interface SearchPageProps {
  searchParams: Promise<{ keyword?: string; page?: string }>
}

export default async function SearchPage({ searchParams }: SearchPageProps) {
  const { locale, t } = await getI18n()
  const params = await searchParams
  const keyword = params.keyword?.trim() ?? ""
  const page = Math.max(1, Number(params.page) || 1)
  const results = keyword ? await searchPosts({ keyword, page, pageSize: 20 }, locale) : []

  return (
    <div className="mx-auto w-full max-w-3xl px-4 py-16">
      <p className="kicker">{t.retrieval}</p>
      <h1 className="mt-2 text-3xl">{t.search}</h1>
      <form className="mt-8 flex gap-2" action={routes.search} method="get">
        <Input name="keyword" defaultValue={keyword} placeholder={t.keyword} />
        <Button type="submit">
          <Search />
          {t.search}
        </Button>
      </form>

      {!keyword ? (
        <p className="mt-10 text-muted-foreground">{t.searchHint}</p>
      ) : results.length === 0 ? (
        <p className="mt-10 text-muted-foreground">{t.noResults(keyword)}</p>
      ) : (
        <ul className="mt-10 divide-y divide-border/70 border-y border-border/70">
          {results.map((item) => (
            <li key={item.id} className="py-5">
              <Link href={postHref(item)} className="text-lg hover:text-primary">
                <span
                  className="search-hit"
                  dangerouslySetInnerHTML={{ __html: item.highlightedTitle }}
                />
              </Link>
              <p className="mt-1 text-xs text-muted-foreground">{formatDate(item.lastUpdateTime)}</p>
              <p
                className="search-hit mt-3 text-sm leading-relaxed text-muted-foreground"
                dangerouslySetInnerHTML={{ __html: item.highlightedSnippet }}
              />
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
