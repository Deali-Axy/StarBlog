export interface PageResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface TocNode {
  text?: string | null
  href?: string | null
  tags?: string[] | null
  nodes?: TocNode[] | null
}

export interface PostResponse {
  id: string
  title: string
  slug?: string | null
  status?: string | null
  isPublish: boolean
  summary?: string | null
  content?: string | null
  path?: string | null
  creationTime: string
  lastUpdateTime: string
  categoryId: number
  categoryName?: string | null
  categories?: string | null
  toc?: TocNode[] | null
}

export interface PublishedPostSummary {
  id: string
  title: string
  slug?: string | null
  summary?: string | null
  content?: string | null
  creationTime: string
  lastUpdateTime: string
  categoryId: number
  categoryName?: string | null
  isPublish: boolean
}

export interface CategoryNodeResponse {
  id: number
  text: string
  href: string
  tags: string[]
  nodes?: CategoryNodeResponse[] | null
}

export interface FeaturedCategoryResponse {
  id: number
  categoryId: number
  name: string
  description: string
  iconCssClass: string
}

export interface PhotoResponse {
  id: string
  title: string
  location: string
  filePath: string
  width: number
  height: number
  createTime: string
}

export interface LinkResponse {
  id: number
  name: string
  description?: string | null
  url: string
  visible: boolean
}

export interface HomeResponse {
  chartVisible: boolean
  randomPhotoVisible: boolean
  randomPhoto?: PhotoResponse | null
  topPost?: PublishedPostSummary | null
  featuredPosts: PublishedPostSummary[]
  featuredPhotos: PhotoResponse[]
  featuredCategories: FeaturedCategoryResponse[]
  links: LinkResponse[]
}

export interface SiteOverviewResponse {
  postsCount: number
  categoriesCount: number
  photosCount: number
  featuredPostsCount: number
  featuredCategoriesCount: number
  featuredPhotosCount: number
}

export interface SearchResultResponse {
  id: string
  title: string
  highlightedTitle: string
  highlightedSnippet: string
  slug?: string | null
  lastUpdateTime: string
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
}

export class ApiError extends Error {
  status: number
  problem?: ProblemDetails

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message)
    this.name = "ApiError"
    this.status = status
    this.problem = problem
  }
}

export interface ListPostsQuery {
  page?: number
  pageSize?: number
  search?: string
  categoryId?: number
}

export interface SearchQuery {
  keyword: string
  page?: number
  pageSize?: number
}
