import type { BlogVariant } from "@/lib/blog-variant"

type ArticleSlot =
  | "gutter" | "page" | "grid" | "trail" | "hero" | "runningHead"
  | "category" | "title" | "summary" | "byline" | "authorMark"
  | "metaDivider" | "share" | "languages" | "tocPane" | "readingColumn"
  | "body" | "endMark" | "recordPane" | "tocHeading" | "tocList"
  | "tocLink" | "mobileToc"

// Complete utility strings keep variant composition readable and statically scannable.
// Layouts use the standard 12-column grid; widths shared with the blog are named tokens.
export const articleDesigns: Record<BlogVariant, Partial<Record<ArticleSlot, string>>> = {
  library: {
    gutter: "lg:px-8",
    page: "max-w-7xl",
    grid: "lg:gap-x-12 xl:gap-x-16",
    hero: "col-span-full border-b-2 border-foreground pt-3 pb-7 sm:pt-6 sm:pb-10",
    runningHead: "border-b border-border pb-5",
    category: "mt-7 bg-highlight px-2 py-1 text-lime-950 sm:mt-9",
    title: "mt-5 text-4xl sm:mt-6 sm:text-5xl xl:text-6xl",
    summary: "max-w-3xl text-sm sm:text-lg",
    byline: "sm:mt-9",
    tocPane: "pt-12 lg:col-span-3 lg:col-start-1 lg:row-start-3",
    readingColumn: "lg:col-span-8 lg:col-start-5 lg:pt-12",
    body: "[&_h2]:border-b [&_h2]:border-border [&_h2]:pb-2",
  },
  editorial: {
    gutter: "lg:px-12",
    page: "max-w-blog-folio",
    grid: "lg:gap-x-12",
    trail: "min-h-14 border-b border-border sm:min-h-16",
    hero: "col-span-full py-6 sm:py-8 lg:col-span-9",
    runningHead: "hidden",
    category: "mt-0 gap-3 before:h-px before:w-6 before:bg-current before:content-['']",
    title: "mt-4 font-heading text-3xl font-semibold leading-snug sm:text-4xl",
    summary: "mt-4 max-w-none text-sm sm:mt-4 sm:text-base",
    byline: "mt-5 border-t border-border pt-4 sm:mt-6",
    authorMark: "hidden",
    metaDivider: "sm:hidden",
    share: "py-1 text-muted-foreground",
    languages: "mt-3",
    tocPane: "border-l border-border pt-8 pl-6 lg:col-span-3 lg:col-start-10 lg:row-start-2 lg:row-span-2",
    readingColumn: "border-t border-border lg:col-span-9 lg:col-start-1",
    body: "text-base [&_:is(h1,h2,h3)]:font-heading [&_h2]:text-2xl [&_blockquote]:border-l [&_blockquote]:ps-5",
    endMark: "text-2xl",
    tocHeading: "border-0",
    tocList: "mt-0 lg:max-h-[calc(100dvh-var(--blog-header-height)-20rem)]",
    tocLink: "border-l aria-[current=location]:bg-transparent aria-[current=location]:text-primary",
    mobileToc: "mt-0",
  },
  studio: {
    page: "max-w-blog-studio sm:pb-12",
    grid: "lg:gap-x-6",
    trail: "min-h-14 sm:min-h-16",
    hero: "col-span-full border border-border bg-card px-5 py-6 sm:px-8 sm:py-8 lg:col-span-9 lg:col-start-4 lg:border-b-0 xl:col-span-8 xl:col-start-3 xl:px-10",
    runningHead: "tracking-wide [&>span]:before:mr-2 [&>span]:before:inline-block [&>span]:before:size-1.5 [&>span]:before:bg-primary [&>span]:before:align-middle [&>span]:before:content-['']",
    category: "mt-7 font-mono sm:mt-8",
    title: "mt-4 text-3xl font-semibold leading-snug sm:text-4xl",
    summary: "border-l-2 border-border ps-4 text-sm",
    authorMark: "hidden",
    tocPane: "lg:col-span-3 lg:col-start-1 lg:row-start-2 lg:row-span-2 xl:col-span-2",
    readingColumn: "border border-t-0 border-border bg-card px-5 pt-7 pb-7 sm:px-8 sm:pb-8 xl:col-span-8 xl:col-start-3 xl:px-10 xl:pt-3 xl:pb-10 lg:col-span-9 lg:col-start-4",
    body: "text-sm sm:text-sm lg:border-t lg:border-border lg:pt-8 [&_:is(h1,h2,h3)]:font-sans",
    endMark: "text-center font-mono text-xs tracking-widest text-muted-foreground",
    recordPane: "xl:col-span-2 xl:col-start-11 xl:row-start-2 xl:row-span-2 xl:block",
    tocHeading: "border border-border bg-muted px-3 py-4 font-mono",
    tocList: "lg:mt-2",
    tocLink: "rounded-sm",
    mobileToc: "mt-0 border border-t-0 border-border bg-muted px-5 sm:px-8",
  },
}

export const articleStickyClassName = "sticky top-[calc(var(--blog-header-height)+1.5rem)]"

// Heading levels range from 1 to 6. Each step follows Tailwind's standard spacing scale.
export const tocIndentClasses = ["ps-2", "ps-5", "ps-8", "ps-11", "ps-14", "ps-17"] as const
