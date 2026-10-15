import { getI18n } from "@/lib/i18n-server"
import type { Metadata } from "next"

import { LinkApplyForm } from "@/components/links/link-apply-form"
import { listVisibleLinks } from "@/lib/api/queries"

export const revalidate = 300

export async function generateMetadata(): Promise<Metadata> {
  const { t } = await getI18n()
  return { title: t.friendLinks, description: t.linksDescription }
}


export default async function LinksPage() {
  const { t } = await getI18n()
  const links = await listVisibleLinks()

  return (
    <div className="page-wrap space-y-14 py-16">
      <div>
        <p className="kicker">{t.exchange}</p>
        <h1 className="mt-2 text-3xl">{t.friendLinks}</h1>
      </div>

      {links.length > 0 ? (
        <ul className="grid gap-x-8 gap-y-6 sm:grid-cols-2 lg:grid-cols-3">
          {links.map((link) => (
            <li key={link.id} className="border-t border-border/70 pt-4">
              <a href={link.url} target="_blank" rel="noreferrer" className="block hover:text-primary">
                <p className="font-heading text-lg">{link.name}</p>
                {link.description ? (
                  <p className="mt-2 text-sm text-muted-foreground">{link.description}</p>
                ) : null}
                <p className="mt-2 truncate text-xs text-muted-foreground">{link.url}</p>
              </a>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-muted-foreground">{t.noLinks}</p>
      )}

      <div className="grid gap-12 lg:grid-cols-2">
        <section>
          <h2 className="text-xl">{t.applyLink}</h2>
          <div className="mt-6">
            <LinkApplyForm />
          </div>
        </section>
        <section>
          <h2 className="text-xl">{t.requirements}</h2>
          <ol className="mt-6 space-y-4 text-sm">
            {t.linkRequirements.map((item, index) => (
              <li key={item.title}>
                <p className="font-medium">
                  {index + 1}. {item.title}
                </p>
                <p className="mt-1 text-muted-foreground">{item.detail}</p>
              </li>
            ))}
          </ol>
        </section>
      </div>
    </div>
  )
}
