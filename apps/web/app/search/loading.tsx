export default function SearchLoading() {
  return (
    <div aria-busy="true" className="mx-auto w-full max-w-6xl space-y-8 px-5 py-16 sm:px-8">
      <div className="h-3 w-40 rounded bg-muted motion-safe:animate-pulse" />
      <div className="h-12 w-3/4 max-w-lg rounded bg-muted motion-safe:animate-pulse" />
      <div className="h-5 w-full max-w-xl rounded bg-muted motion-safe:animate-pulse" />
      <div className="h-18 w-full max-w-3xl rounded-lg border border-border bg-card motion-safe:animate-pulse" />
      <div className="space-y-6 border-t border-border pt-8">
        {[0, 1, 2].map((item) => <div key={item} className="h-24 rounded bg-muted/60 motion-safe:animate-pulse" />)}
      </div>
    </div>
  )
}
