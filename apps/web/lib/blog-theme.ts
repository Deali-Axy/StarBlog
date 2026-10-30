import type { BlogVariant } from "@/lib/blog-variant"

// The list and its portals share tokens; the global site theme stays independent.
const themes: Record<BlogVariant, string> = {
  library: [
    "[--background:#f7f8f2] dark:[--background:#171914]",
    "[--foreground:#1b2018] dark:[--foreground:#eff1e7]",
    "[--card:#f7f8f2] dark:[--card:#20251c]",
    "[--primary:#4b5b12] dark:[--primary:#d5eb65]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#202719]",
    "[--muted:#eceee4] dark:[--muted:#2a3025]",
    "[--muted-foreground:#646b5d] dark:[--muted-foreground:#acb4a1]",
    "[--border:#c9cebf] dark:[--border:#424a39]",
    "[--accent:#e1ef83] dark:[--accent:#3b4824]",
    "[--highlight:#deef70] dark:[--highlight:#deef70]",
    "[--ring:#637828] dark:[--ring:#d5eb65] [--radius:0rem]",
  ].join(" "),
  editorial: [
    "[--background:#f3eee4] dark:[--background:#23231e]",
    "[--foreground:#293c32] dark:[--foreground:#e5e4d6]",
    "[--card:#f8f4eb] dark:[--card:#2d3028]",
    "[--primary:#8d4536] dark:[--primary:#e2aa88]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#2f261c]",
    "[--muted:#e8e2d5] dark:[--muted:#35382e]",
    "[--muted-foreground:#716f60] dark:[--muted-foreground:#b2b3a2]",
    "[--border:#cbc7b7] dark:[--border:#4d5142]",
    "[--accent:#e5d9c5] dark:[--accent:#454535]",
    "[--highlight:#314f3b] dark:[--highlight:#b9cfb0]",
    "[--ring:#8d4536] dark:[--ring:#e2aa88] [--radius:0rem]",
  ].join(" "),
  studio: [
    "[--background:#e7eae5] dark:[--background:#161c18]",
    "[--foreground:#28332b] dark:[--foreground:#e0e7df]",
    "[--card:#f3f5ef] dark:[--card:#222b24]",
    "[--primary:#3e6247] dark:[--primary:#b7d3a9]",
    "[--primary-foreground:#ffffff] dark:[--primary-foreground:#1c2a1d]",
    "[--muted:#dce2d8] dark:[--muted:#2d382e]",
    "[--muted-foreground:#65715f] dark:[--muted-foreground:#a5b4a1]",
    "[--border:#c4cebf] dark:[--border:#3c4b3b]",
    "[--accent:#d8e4cc] dark:[--accent:#384c31]",
    "[--highlight:#3e6247] dark:[--highlight:#b7d3a9]",
    "[--ring:#3e6247] dark:[--ring:#b7d3a9] [--radius:0.2rem]",
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
