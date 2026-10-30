import ReactMarkdown, { type Components } from "react-markdown"
import remarkGfm from "remark-gfm"
import rehypeHighlight from "rehype-highlight"
import rehypeSlug from "rehype-slug"

import "highlight.js/styles/github-dark.min.css"

import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const markdownHeadingClassName = cn(headingClassName, "mt-9 mb-4 scroll-mt-20 text-foreground/95")

const markdownComponents: Components = {
  h1: ({ id, children }) => <h1 id={id} className={cn(markdownHeadingClassName, "text-3xl leading-tight")}>{children}</h1>,
  h2: ({ id, children }) => <h2 id={id} className={cn(markdownHeadingClassName, "text-2xl leading-tight")}>{children}</h2>,
  h3: ({ id, children }) => <h3 id={id} className={cn(markdownHeadingClassName, "text-xl leading-snug")}>{children}</h3>,
  h4: ({ id, children }) => <h4 id={id} className={markdownHeadingClassName}>{children}</h4>,
  h5: ({ id, children }) => <h5 id={id} className={markdownHeadingClassName}>{children}</h5>,
  h6: ({ id, children }) => <h6 id={id} className={markdownHeadingClassName}>{children}</h6>,
  p: ({ children }) => <p className="my-4 leading-loose">{children}</p>,
  ul: ({ children, className }) => <ul className={cn("my-4 list-disc pl-5", className)}>{children}</ul>,
  ol: ({ start, children }) => <ol start={start} className="my-4 list-decimal pl-5">{children}</ol>,
  li: ({ children, className }) => <li className={cn("my-1.5 leading-relaxed", className)}>{children}</li>,
  a: ({ href, title, children }) => <a href={href} title={title} className="text-primary underline underline-offset-4">{children}</a>,
  strong: ({ children }) => <strong className="font-semibold">{children}</strong>,
  code: ({ children, className }) => <code className={cn("rounded-sm border border-border/85 bg-muted/30 px-2 py-0.5 text-sm", className)}>{children}</code>,
  pre: ({ children }) => <pre className="my-4 overflow-x-auto rounded-md border border-border/90 bg-[oklch(0.16_0.02_264)] p-4">{children}</pre>,
  blockquote: ({ children }) => <blockquote className="my-4 border-l-2 border-primary py-1 pl-4 text-muted-foreground/90">{children}</blockquote>,
  hr: () => <hr className="my-7 border-0 border-t border-border/90" />,
  table: ({ children }) => <table className="my-4 w-full border-collapse overflow-hidden rounded-md border border-border/90">{children}</table>,
  th: ({ children, style }) => <th style={style} className="border-b border-border/90 px-4 py-3.5 text-left">{children}</th>,
  td: ({ children, style }) => <td style={style} className="border-b border-border/90 px-4 py-3.5 text-left">{children}</td>,
  img: ({ src, alt }) => {
    if (!src || typeof src !== "string") return null
    return (
      // Markdown images may use API media URLs (served through Next rewrites).
      // eslint-disable-next-line @next/next/no-img-element
      <img src={src} alt={alt ?? ""} className="my-6 h-auto max-w-full rounded-none" />
    )
  },
}

export function MarkdownContent({ content }: { content: string }) {
  return (
    <div
      data-markdown
      className="text-foreground/90 [&>:first-child]:mt-0 [&>:last-child]:mb-0 [&_pre_code]:rounded-none [&_pre_code]:border-0 [&_pre_code]:bg-transparent! [&_pre_code]:p-0!"
    >
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[rehypeSlug, rehypeHighlight]}
        components={markdownComponents}
      >
        {content}
      </ReactMarkdown>
    </div>
  )
}
