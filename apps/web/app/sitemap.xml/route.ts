import { connection } from "next/server"
import { sitemapIndex, xmlResponse } from "@/lib/sitemap"

export async function GET() {
  await connection()
  return xmlResponse(await sitemapIndex())
}
