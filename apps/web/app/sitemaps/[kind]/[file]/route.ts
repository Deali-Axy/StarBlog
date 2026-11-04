import { connection } from "next/server"
import { sitemapChunk, xmlResponse } from "@/lib/sitemap"

export async function GET(_request: Request, { params }: { params: Promise<{ kind: string; file: string }> }) {
  await connection()
  const { kind, file } = await params
  const match = /^([1-9]\d*)\.xml$/.exec(file)
  const xml = match ? await sitemapChunk(kind, Number(match[1])) : null
  return xml ? xmlResponse(xml) : new Response("Sitemap not found", { status: 404, headers: { "X-Robots-Tag": "noindex" } })
}
