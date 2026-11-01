import type { PostResponse, PostTranslationResponse } from "@/lib/api/types"
import type { ArticleHeading } from "@/lib/markdown"
import type { ReactNode } from "react"
export type ArticlePost = Omit<PostResponse, "content" | "toc">

export type ArticleLayoutProps = {
  post: ArticlePost
  headings: ArticleHeading[]
  children: ReactNode
  minutes: number
  related?: PostResponse[]
  permalink: string
  translations?: Pick<PostTranslationResponse, "language">[]
  lang?: string
  translationMissing?: boolean
}
