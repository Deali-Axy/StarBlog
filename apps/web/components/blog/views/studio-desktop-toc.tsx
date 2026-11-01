"use client"
import { useI18n } from "@/components/app-providers"
import { cn } from "@/lib/utils"
import { ArrowUp } from "lucide-react"
import { useId } from "react"
import nativeStyles from "../article-native.module.css"
import { tocIndentClasses, useArticleReading } from "../article-reading"
export function ArchiveDesktopTOC() {
  const { t } = useI18n()
  const { items, baseline, activeId, progress, navigate } = useArticleReading()
  const listId = useId()
  const nav = <nav aria-label={t.toc} data-article-toc>
    {<div className={"flex justify-between gap-3 text-xs border border-border bg-muted px-3 py-4 font-mono"}>
      <span>{t.toc}</span><small className="font-mono text-xs text-muted-foreground">{String(items.length).padStart(2, "0")}</small>
    </div>}
    <ol id={listId} className={"list-none overflow-y-auto overscroll-contain [scrollbar-width:thin] mt-4 max-h-[calc(100dvh-var(--blog-header-height)-15rem)] lg:mt-2"}>
      {items.map((item, index) => <li key={item.id}>
        <a href={`#${encodeURIComponent(item.id)}`} onClick={event => navigate(event, item.id)} aria-current={activeId === item.id ? "location" : undefined} className={cn("flex items-baseline gap-2 border-l-2 border-transparent px-2 py-2 text-xs leading-relaxed text-muted-foreground wrap-anywhere transition-colors hover:bg-muted hover:text-foreground aria-[current=location]:border-primary aria-[current=location]:bg-accent aria-[current=location]:text-foreground motion-reduce:transition-none", "rounded-sm", tocIndentClasses[Math.min(tocIndentClasses.length - 1, Math.max(0, item.depth - baseline))])}>
          <small className="shrink-0 font-mono text-xs opacity-60" aria-hidden="true">{String(index + 1).padStart(2, "0")}</small>
          <span>{item.text || t.untitledSection}</span>
        </a>
      </li>)}
    </ol>
    {<>
      <div className="mt-7 border-t border-border pt-4">
        <div className="flex justify-between gap-4 text-xs text-muted-foreground"><span>{t.readingProgress}</span><span>{progress}%</span></div>
        <progress className={cn("mt-3 block h-0.5 w-full appearance-none border-0 bg-border text-primary", nativeStyles.progress)} aria-label={t.readingProgress} max={100} value={progress} />
      </div>
      <button type="button" className="mt-4 inline-flex cursor-pointer items-center gap-2 py-2 text-xs text-muted-foreground hover:text-foreground" onClick={() => window.scrollTo({ top: 0, behavior: "instant" })}><ArrowUp size={14} aria-hidden="true" />{t.backToTop}</button>
    </>}
  </nav>
  return nav
}
