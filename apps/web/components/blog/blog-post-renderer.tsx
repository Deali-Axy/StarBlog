"use client"
import type { BlogVariant } from "@/lib/blog-variant"
import type { ComponentType } from "react"
import type { ArticleLayoutProps } from "./article-props"
import { ArticleReadingProvider } from "./article-reading"
import { useBlogLayout } from "./blog-layout-context"
import { CommentSessionProvider } from "./comment-section"
import { FolioArticle } from "./views/editorial-article"
import { IndexArticle } from "./views/library-article"
import { ArchiveArticle } from "./views/studio-article"
const articles = { library: IndexArticle, editorial: FolioArticle, studio: ArchiveArticle } satisfies Record<BlogVariant, ComponentType<ArticleLayoutProps>>
export function BlogPostRenderer(props: ArticleLayoutProps) {
  const { variant } = useBlogLayout()
  const Article = articles[variant]
  return <CommentSessionProvider key={props.post.id} postId={props.post.id}><ArticleReadingProvider items={props.headings} layoutKey={variant}><Article {...props} /></ArticleReadingProvider></CommentSessionProvider>
}
