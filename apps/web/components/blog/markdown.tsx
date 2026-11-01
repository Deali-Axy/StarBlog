import "server-only"

import renderReactMarkdown, { type Components } from "react-markdown"
import remarkGfm from "remark-gfm"
import rehypeHighlight from "rehype-highlight"
import rehypeSlug from "rehype-slug"

import "highlight.js/styles/github-dark.min.css"

import { rehypeArticleHeadings, type ArticleHeading } from "@/lib/markdown"
import { cn } from "@/lib/utils"

const headingClassName = "mt-10 mb-4 scroll-mt-[calc(var(--blog-header-height)+5rem)] font-semibold leading-snug tracking-tight focus:outline-none target:underline target:decoration-primary target:decoration-1 target:underline-offset-4"

const markdownComponents: Components = {
  h1: ({ id, children, className }) => <h1 id={id} tabIndex={-1} className={cn(headingClassName, "text-3xl", className)}>{children}</h1>,
  h2: ({ id, children, className }) => <h2 id={id} tabIndex={-1} className={cn(headingClassName, "text-2xl", className)}>{children}</h2>,
  h3: ({ id, children, className }) => <h3 id={id} tabIndex={-1} className={cn(headingClassName, "text-xl", className)}>{children}</h3>,
  h4: ({ id, children, className }) => <h4 id={id} tabIndex={-1} className={cn(headingClassName, "text-base", className)}>{children}</h4>,
  h5: ({ id, children, className }) => <h5 id={id} tabIndex={-1} className={cn(headingClassName, "text-base", className)}>{children}</h5>,
  h6: ({ id, children, className }) => <h6 id={id} tabIndex={-1} className={cn(headingClassName, "text-base", className)}>{children}</h6>,
  p: ({ children }) => <p className="my-5">{children}</p>,
  ul: ({ children, className }) => <ul className={cn("my-5", className, className?.includes("contains-task-list") ? "list-none ps-1" : "list-disc ps-6")}>{children}</ul>,
  ol: ({ start, children, className }) => <ol start={start} className={cn("my-5 list-decimal ps-6", className)}>{children}</ol>,
  li: ({ children, className }) => <li className={cn("my-2 ps-0.5 [&>ul]:my-2 [&>ol]:my-2", className)}>{children}</li>,
  a: ({ id, href, title, children, "aria-label": label, "aria-describedby": describedBy }) => <a id={id} href={href} title={title} aria-label={label} aria-describedby={describedBy} className="text-primary underline decoration-1 underline-offset-4 hover:decoration-2">{children}</a>,
  strong: ({ children }) => <strong className="font-bold">{children}</strong>,
  code: ({ children, className }) => <code className={cn("rounded-sm border border-border bg-muted px-1.5 py-0.5 font-mono text-sm", className)}>{children}</code>,
  pre: ({ children }) => <pre className="my-7 max-w-full overflow-x-auto rounded-sm border border-slate-800 bg-slate-950 p-4 font-mono text-sm leading-relaxed text-slate-100 sm:p-6 [&_code]:rounded-none [&_code]:border-0 [&_code]:bg-transparent! [&_code]:p-0! [&_code]:text-xs sm:[&_code]:text-sm">{children}</pre>,
  blockquote: ({ children }) => <blockquote className="my-7 border-l-2 border-primary py-1 ps-5 text-muted-foreground [&>:first-child]:mt-1 [&>:last-child]:mb-1">{children}</blockquote>,
  hr: () => <hr className="my-10 border-0 border-t border-border" />,
  table: ({ children }) => <div className="my-7 max-w-full overflow-x-auto" tabIndex={0}><table className="w-full border-collapse text-sm">{children}</table></div>,
  th: ({ children, style }) => <th style={style} className="min-w-28 border border-border bg-muted px-4 py-3 text-left font-semibold">{children}</th>,
  td: ({ children, style }) => <td style={style} className="min-w-28 border border-border px-4 py-3 text-left">{children}</td>,
  input: ({ type, checked, disabled }) => <input type={type} checked={checked} disabled={disabled} className="me-2 accent-primary" />,
  img: ({ src, alt, title }) => {
    if (!src || typeof src !== "string") return null
    return (
      // Markdown images may use API media URLs (served through Next rewrites).
      // eslint-disable-next-line @next/next/no-img-element
      <img src={src} alt={alt ?? ""} title={title} loading="lazy" decoding="async" className="mx-auto my-8 block h-auto max-w-full" />
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
  return { headings, content: <div data-markdown className={cn(
    "wrap-anywhere leading-loose text-foreground [&>:first-child]:mt-0 [&>:last-child]:mb-0",
    "[&_.footnotes]:mt-16 [&_.footnotes]:border-t [&_.footnotes]:border-border [&_.footnotes]:text-sm [&_.footnotes]:text-muted-foreground",
  )}>{rendered}</div> }
}
