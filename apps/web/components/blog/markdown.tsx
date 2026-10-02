import ReactMarkdown from "react-markdown"
import remarkGfm from "remark-gfm"
import rehypeHighlight from "rehype-highlight"
import rehypeSlug from "rehype-slug"

export function MarkdownContent({ content }: { content: string }) {
  return (
    <div className="content-prose">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[rehypeSlug, rehypeHighlight]}
        components={{
          img: ({ src, alt }) => {
            if (!src || typeof src !== "string") return null
            return (
              // markdown 图可能来自 API 静态目录，保持原 URL（经 Next rewrite）
              // eslint-disable-next-line @next/next/no-img-element
              <img src={src} alt={alt ?? ""} className="h-auto max-w-full" />
            )
          },
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  )
}
