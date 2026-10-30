import Link from "next/link"
import { ArrowUpRight } from "lucide-react"

export function HomeSectionHeading({ id, eyebrow, title, href, action }: {
  id: string; eyebrow: string; title: string; href: string; action: string
}) {
  return <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
    <div><p className="mb-2.5 text-[11px] tracking-[.12em] text-primary">{eyebrow}</p><h2 id={id} className="font-heading text-[clamp(1.5rem,2.8vw,2rem)] leading-normal font-semibold tracking-[-.04em]">{title}</h2></div>
    <Link href={href} className="inline-flex min-h-11 items-center gap-2 text-xs text-muted-foreground hover:text-primary">{action}<ArrowUpRight size={15} aria-hidden="true" /></Link>
  </div>
}
