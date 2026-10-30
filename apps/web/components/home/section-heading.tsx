import Link from "next/link"
import { ArrowUpRight } from "lucide-react"
import styles from "./home-sections.module.css"

export function HomeSectionHeading({ id, eyebrow, title, href, action }: {
  id: string; eyebrow: string; title: string; href: string; action: string
}) {
  return <div className={styles.sectionHeading}>
    <div><p className={styles.eyebrow}>{eyebrow}</p><h2 id={id}>{title}</h2></div>
    <Link href={href} className={styles.sectionAction}>{action}<ArrowUpRight size={15} aria-hidden="true" /></Link>
  </div>
}
