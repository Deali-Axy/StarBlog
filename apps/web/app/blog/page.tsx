import Link from "next/link"
import type { Metadata } from "next"
import { Search } from "lucide-react"

import { CategoryTree } from "@/components/blog/category-tree"
import { ControlPanel } from "@/components/blog/control-panel"
import { PaginationNav } from "@/components/blog/pagination-nav"
import { PostCard } from "@/components/blog/post-card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { getCategoryTree, listPosts } from "@/lib/api/queries"
import { routes } from "@/lib/routes"

export const revalidate = 300

export const metadata: Metadata = {
  title: "文章",
}

interface BlogPageProps {
  searchParams: Promise<{
    page?: string
    pageSize?: string
    search?: string
    categoryId?: string
    sortBy?: string
    sortType?: string
    view?: string
  }>
}

export default async function BlogPage({ searchParams }: BlogPageProps) {
  const params = await searchParams
  const page = Math.max(1, Number(params.page) || 1)
  const pageSize = Math.min(100, Math.max(1, Number(params.pageSize) || 12))
  const search = params.search?.trim() || undefined
  const categoryId = params.categoryId ? Number(params.categoryId) : undefined
  const sortBy =
    params.sortBy === "title" || params.sortBy === "creationTime" ? params.sortBy : "lastUpdateTime"
  const sortType = params.sortType === "asc" ? "asc" : "desc"
  const view = params.view === "list" ? "list" : "grid"

  const [tree, posts] = await Promise.all([
    getCategoryTree(),
    listPosts({
      page,
      pageSize,
      search,
      categoryId,
      sortBy,
      sortType,
      includeDescendants: Boolean(categoryId),
    }),
  ])

  const items = posts.items

  return (
    <div className="page-wrap grid gap-12 py-12 lg:grid-cols-[14rem_minmax(0,1fr)]">
      <aside className="lg:sticky lg:top-24 lg:self-start">
        <CategoryTree
          categoryNodes={tree}
          currentCategoryId={Number.isFinite(categoryId) ? categoryId : undefined}
          searchQuery={search}
        />
      </aside>
      <div className="space-y-6">
        <div>
          <p className="kicker">观测记录</p>
          <h1 className="mt-2 text-3xl">文章</h1>
        </div>
        <form className="flex gap-2" action={routes.blog} method="get">
          {categoryId ? <input type="hidden" name="categoryId" value={categoryId} /> : null}
          <Input name="search" defaultValue={search} placeholder="在文章中搜索标题" />
          <Button type="submit">
            <Search />
            搜索
          </Button>
        </form>
        <ControlPanel
          currentSortBy={sortBy}
          currentSortType={sortType}
          currentView={view}
          searchQuery={search}
          categoryId={Number.isFinite(categoryId) ? categoryId : undefined}
        />
        {items.length === 0 ? (
          <div className="border-y border-dashed border-border py-16 text-center text-muted-foreground">
            这片星空里暂时没有匹配的文章。
            <div className="mt-4">
              <Button variant="outline" asChild>
                <Link href={routes.blog}>查看全部</Link>
              </Button>
            </div>
          </div>
        ) : (
          <div className={view === "list" ? "divide-y divide-border/70" : "grid grid-cols-1 gap-8 md:grid-cols-2"}>
            {items.map((post) => (
              <PostCard
                key={post.id}
                viewMode={view}
                post={{
                  id: post.id,
                  title: post.title,
                  slug: post.slug,
                  summary: post.summary,
                  categoryName: post.categoryName,
                  lastUpdateTime: post.lastUpdateTime,
                  content: post.content,
                }}
              />
            ))}
          </div>
        )}
        <PaginationNav
          currentPage={posts.page}
          totalPages={posts.totalPages}
          searchQuery={search}
          categoryId={Number.isFinite(categoryId) ? categoryId : undefined}
          sortBy={sortBy}
          sortType={sortType}
          pageSize={pageSize}
          viewMode={view}
        />
      </div>
    </div>
  )
}
