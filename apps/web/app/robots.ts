import type { MetadataRoute } from "next"
import { absoluteUrl } from "@/lib/site"

export default function robots(): MetadataRoute.Robots {
  return {
    rules: { userAgent: "*", allow: "/", disallow: ["/api/v1/admin/", "/admin/", "/api-docs/"] },
    sitemap: [absoluteUrl("/sitemap.xml"), absoluteUrl("/sitemap-images.xml")],
  }
}
