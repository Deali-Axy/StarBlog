// Shared utility compositions stay as complete class names for Tailwind's scanner.
export const pageContainerClassName = "mx-auto w-full max-w-6xl px-4"
export const eyebrowClassName = "text-xs font-medium tracking-widest text-primary"
export const headingClassName = "font-heading font-semibold tracking-tight"
export const searchHighlightClassName = "[&_mark]:rounded-sm [&_mark]:bg-primary/40 [&_mark]:px-0.5 [&_mark]:text-inherit"

export const photoInsetClassName = "px-4 sm:px-8"
export const photoIconClassName = "inline-flex size-11 shrink-0 items-center justify-center rounded-xl transition-colors duration-150 hover:bg-muted"

// Portals do not inherit the photography shell, so they also need these tokens.
export const photographyThemeClassName = [
  "scheme-light dark:scheme-dark",
  "[--background:#ffffff] dark:[--background:#080808]",
  "[--foreground:#171717] dark:[--foreground:#f5f5f5]",
  "[--card:#ffffff] dark:[--card:#141414]",
  "[--card-foreground:#171717] dark:[--card-foreground:#f5f5f5]",
  "[--popover:#ffffff] dark:[--popover:#141414]",
  "[--popover-foreground:#171717] dark:[--popover-foreground:#f5f5f5]",
  "[--muted:#f3f3f3] dark:[--muted:#202020]",
  "[--muted-foreground:#707070] dark:[--muted-foreground:#a0a0a0]",
  "[--border:#e7e7e7] dark:[--border:#292929]",
  "[--input:#e7e7e7] dark:[--input:#292929]",
  "[--primary:#171717] dark:[--primary:#f5f5f5]",
  "[--primary-foreground:#ffffff] dark:[--primary-foreground:#080808]",
  "[--accent:#eeeeee] dark:[--accent:#262626]",
  "[--accent-foreground:#171717] dark:[--accent-foreground:#f5f5f5]",
  "[--ring:#737373] dark:[--ring:#a3a3a3]",
  "[&_:is(a,button,select):focus-visible]:outline-2 [&_:is(a,button,select):focus-visible]:outline-solid [&_:is(a,button,select):focus-visible]:outline-offset-3 [&_:is(a,button,select):focus-visible]:outline-ring",
].join(" ")

// The homepage menu is portalled, so it shares the cover's theme tokens explicitly.
export const homeThemeClassName = [
  "[--background:#f6f3ec] dark:[--background:#191b1a]",
  "[--foreground:#292720] dark:[--foreground:#eee9df]",
  "[--card:#fbf9f4] dark:[--card:#222522] [--card-foreground:var(--foreground)]",
  "[--muted:#eae6dc] dark:[--muted:#292c28]",
  "[--muted-foreground:#726b60] dark:[--muted-foreground:#b0ab9f]",
  "[--border:#dcd5c8] dark:[--border:#3b3e36]",
  "[--primary:#a34e36] dark:[--primary:#e7a17e]",
  "[--primary-foreground:#fffaf3] dark:[--primary-foreground:#241b15]",
  "[--accent:#eae3d7] dark:[--accent:#32342c] [--accent-foreground:var(--foreground)]",
  "[--ring:var(--primary)]",
].join(" ")

export const homeSectionClassName = "mt-16 first:mt-10 sm:mt-22"
