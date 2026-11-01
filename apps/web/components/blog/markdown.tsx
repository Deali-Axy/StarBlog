import "server-only"

import renderReactMarkdown, { type Components } from "react-markdown"
import remarkGfm from "remark-gfm"
import rehypeHighlight from "rehype-highlight"
import rehypeSlug from "rehype-slug"

import "highlight.js/styles/github-dark.min.css"

import { rehypeArticleHeadings, type ArticleHeading } from "@/lib/markdown"
import styles from "./markdown.module.css"

const markdownComponents: Components = {
  h1: ({ id, children }) => <h1 id={id} tabIndex={-1}>{children}</h1>,
  h2: ({ id, children }) => <h2 id={id} tabIndex={-1}>{children}</h2>,
  h3: ({ id, children }) => <h3 id={id} tabIndex={-1}>{children}</h3>,
  h4: ({ id, children }) => <h4 id={id} tabIndex={-1}>{children}</h4>,
  h5: ({ id, children }) => <h5 id={id} tabIndex={-1}>{children}</h5>,
  h6: ({ id, children }) => <h6 id={id} tabIndex={-1}>{children}</h6>,
  table: ({ children }) => <div className={styles.tableScroll} tabIndex={0}><table>{children}</table></div>,
  img: ({ src, alt, title }) => {
    if (!src || typeof src !== "string") return null
    return (
      // Markdown images may use API media URLs (served through Next rewrites).
      // eslint-disable-next-line @next/next/no-img-element
      <img src={src} alt={alt ?? ""} title={title} loading="lazy" decoding="async" />
    )
  },
}

/** Server entry point: parse once, then pass rendered React nodes to the client layout. */
export function renderArticleMarkdown(content: string) {
  const headings: ArticleHeading[] = []
  // The library exposes a synchronous, hook-free renderer. Run the pipeline here
  // so the TOC is ready before it crosses the server/client boundary.
  const rendered = renderReactMarkdown({
    children: content,
    remarkPlugins: [remarkGfm],
    rehypePlugins: [rehypeSlug, [rehypeArticleHeadings, { headings }], [rehypeHighlight, { detect: false }]],
    components: markdownComponents,
  })
  return { headings, content: <div data-markdown className={styles.markdown}>{rendered}</div> }
}
