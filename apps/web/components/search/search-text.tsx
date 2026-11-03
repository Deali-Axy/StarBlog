// API snippets are text. Only the search engine's literal mark delimiters become elements.
export function HighlightedText({ text }: { text: string }) {
  return text.split(/(<mark>[\s\S]*?<\/mark>)/g).map((part, index) => (
    part.startsWith("<mark>") && part.endsWith("</mark>")
      ? <mark key={index} className="bg-transparent font-semibold text-inherit">{part.slice(6, -7)}</mark>
      : part
  ))
}

export function snippetText(text: string) {
  return text
    .replace(/!\[([^\]]*)\]\([^)]*\)/g, "$1")
    .replace(/\[([^\]]+)\]\([^)]*\)/g, "$1")
    .replace(/^\s{0,3}(?:#{1,6}|>|[-*+])\s+/gm, "")
    .replace(/`{1,3}/g, "")
    .replace(/\s+/g, " ")
    .trim()
}
