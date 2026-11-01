"use client"
import { useI18n } from "@/components/app-providers"
import { cn } from "@/lib/utils"
import { ChevronDown, List } from "lucide-react"
import { useId } from "react"
import nativeStyles from "../article-native.module.css"
import { tocIndentClasses, useArticleReading } from "../article-reading"
export function ArchiveMobileTOC() {
  const { t } = useI18n()
  const { items, baseline, activeId, navigate } = useArticleReading()
  const listId = useId()
  const nav = <nav aria-label={t.toc} data-article-toc className={"pb-4"}>

    <ol id={listId} className={"list-none overflow-y-auto overscroll-contain [scrollbar-width:thin] max-h-80 lg:mt-2"}>
      {items.map((item, index) => <li key={item.id}>
        <a href={`#${encodeURIComponent(item.id)}`} onClick={event => navigate(event, item.id)} aria-current={activeId === item.id ? "location" : undefined} className={cn("flex items-baseline gap-2 border-l-2 border-transparent px-2 py-2 text-xs leading-relaxed text-muted-foreground wrap-anywhere transition-colors hover:bg-muted hover:text-foreground aria-[current=location]:border-primary aria-[current=location]:bg-accent aria-[current=location]:text-foreground motion-reduce:transition-none", "rounded-sm", tocIndentClasses[Math.min(tocIndentClasses.length - 1, Math.max(0, item.depth - baseline))])}>
          <small className="shrink-0 font-mono text-xs opacity-60" aria-hidden="true">{String(index + 1).padStart(2, "0")}</small>
          <span>{item.text || t.untitledSection}</span>
        </a>
      </li>)}
    </ol>

  </nav>
  return <details className={cn("group/toc col-span-full row-start-3 mt-6 border-y border-border lg:hidden", nativeStyles.mobileContents, "mt-0 border border-t-0 border-border bg-muted px-5 sm:px-8")}>
    <summary className="flex min-h-12 cursor-pointer list-none items-center gap-2 text-xs">
      <List size={16} aria-hidden="true" /><span>{t.toc}</span>
      <small className="ml-auto font-mono text-xs text-muted-foreground">{t.sectionCount(items.length)}</small>
      <ChevronDown className="size-4 transition-transform group-open/toc:rotate-180 motion-reduce:transition-none" aria-hidden="true" />
    </summary>
    {nav}
  </details>
}
