import type { BlogVariant } from "@/lib/blog-variant"

// Apply these tokens to the list shell AND Radix portal contents.
const themes: Record<BlogVariant, string> = {
  library: [
    "[--background:#f5f7fa] dark:[--background:#10151f]",
    "[--foreground:#172338] dark:[--foreground:#e6edf7]",
    "[--card:#ffffff] dark:[--card:#181f2c]",
    "[--primary:#3568d4] dark:[--primary:#8eafff]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#101b35]",
    "[--muted:#edf1f7] dark:[--muted:#212b3b]",
    "[--muted-foreground:#65738a] dark:[--muted-foreground:#9daec6]",
    "[--border:#dfe5ee] dark:[--border:#2d384c]",
    "[--accent:#e8efff] dark:[--accent:#263658]",
    "[--ring:#3568d4] dark:[--ring:#8eafff] [--radius:0.75rem]",
  ].join(" "),
  editorial: [
    "[--background:#faf8f3] dark:[--background:#191815]",
    "[--foreground:#292820] dark:[--foreground:#eeece4]",
    "[--card:#fffdf8] dark:[--card:#211f1b]",
    "[--primary:#ac4b2b] dark:[--primary:#eca181]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#301b12]",
    "[--muted:#eeeae0] dark:[--muted:#2c2923]",
    "[--muted-foreground:#777266] dark:[--muted-foreground:#b5afa1]",
    "[--border:#ddd7ca] dark:[--border:#3e3930]",
    "[--accent:#f2e8dc] dark:[--accent:#3b2c23]",
    "[--ring:#ac4b2b] dark:[--ring:#eca181] [--radius:0.2rem]",
  ].join(" "),
  studio: [
    "[--background:#edf2f4] dark:[--background:#0d171d]",
    "[--foreground:#19333e] dark:[--foreground:#dceef3]",
    "[--card:#f8fcfd] dark:[--card:#14242d]",
    "[--primary:#087b88] dark:[--primary:#64d6df]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#08292f]",
    "[--muted:#e0e9ed] dark:[--muted:#1c333e]",
    "[--muted-foreground:#566f7b] dark:[--muted-foreground:#92afb9]",
    "[--border:#cddde3] dark:[--border:#29434e]",
    "[--accent:#d6eef0] dark:[--accent:#1c3d46]",
    "[--ring:#087b88] dark:[--ring:#64d6df] [--radius:0.5rem]",
  ].join(" "),
}

export function blogThemeClassName(variant: BlogVariant) {
  return [
    themes[variant],
    "scheme-light dark:scheme-dark bg-background text-foreground",
    "[--card-foreground:var(--foreground)] [--popover:var(--card)] [--popover-foreground:var(--foreground)]",
    "[--input:var(--border)] [--accent-foreground:var(--foreground)]",
    "[&_:is(a,button,input):focus-visible]:outline-2 [&_:is(a,button,input):focus-visible]:outline-offset-3 [&_:is(a,button,input):focus-visible]:outline-ring",
  ].join(" ")
}
