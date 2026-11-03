/** Heading IDs and the TOC are collected from the same rendered Markdown tree. */
export interface ArticleHeading {
  id: string
  text: string
  depth: number
}

/** Legacy posts and translations store image names relative to their post media directory. */
export function articleImageUrl(src: string, postId?: string) {
  if (!postId || !src || /^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(src)) return src
  const normalized = src.replaceAll("\\", "/").replace(/^(?:\.\/)+/, "")
  if (normalized.split("/").some(segment => segment === "..")) return ""
  return `/media/blog/${encodeURIComponent(postId)}/${normalized}`
}

// The subset of HAST used by this rehype plugin.
interface MarkdownNode {
  type: string
  tagName?: string
  value?: string
  properties?: Record<string, unknown>
  children?: MarkdownNode[]
}

function headingText(node: MarkdownNode): string {
  if (node.type === "text") return node.value ?? ""
  if (node.tagName === "img") return String(node.properties?.alt ?? "")
  return (node.children ?? []).map(headingText).join("")
}

export function rehypeArticleHeadings({ headings }: { headings: ArticleHeading[] }) {
  return (tree: MarkdownNode) => {
    const used = new Set<string>()
    function reserve(node: MarkdownNode) {
      if (node.properties?.id) used.add(String(node.properties.id))
      node.children?.forEach(reserve)
    }
    reserve(tree)
    function visit(node: MarkdownNode) {
      // Footnotes have their own navigation and a visually hidden heading.
      if (node.properties?.dataFootnotes) return
      if (node.type === "element" && /^h[1-6]$/.test(node.tagName ?? "")) {
        let id = String(node.properties?.id ?? "")
        if (!id) {
          let suffix = headings.length + 1
          do { id = `section-${suffix++}` } while (used.has(id))
          node.properties = { ...node.properties, id }
          used.add(id)
        }
        headings.push({ id, text: headingText(node), depth: Number(node.tagName![1]) })
      }
      node.children?.forEach(visit)
    }
    visit(tree)
  }
}

export function headingIdFromHash(hash: string) {
  const id = hash.replace(/^#/, "")
  try { return decodeURIComponent(id) } catch { return id }
}

/** Choose a section even in the gap between headings and in the last section. */
export function currentHeadingIndex(tops: number[], offset: number, atEnd = false) {
  if (!tops.length) return -1
  if (atEnd) return tops.length - 1
  let current = 0
  for (let index = 0; index < tops.length; index++) {
    if (tops[index] > offset) break
    current = index
  }
  return current
}
