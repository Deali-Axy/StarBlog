import type { NextConfig } from "next"

function normalizeBackendOrigin(value: string | undefined, fallback: string) {
  const trimmed = value?.trim()
  const normalized = (trimmed ? trimmed : fallback).replace(/\/$/, "")
  return normalized.replace(/\/api$/, "")
}

const BACKEND_URL = normalizeBackendOrigin(
  process.env.INTERNAL_API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL,
  "http://127.0.0.1:5039",
)

const nextConfig: NextConfig = {
  output: "standalone",
  allowedDevOrigins: ["127.0.0.1", "localhost"],
  turbopack: {
    root: process.cwd(),
  },
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${BACKEND_URL}/api/:path*`,
      },
      {
        source: "/media/:path*",
        destination: `${BACKEND_URL}/media/:path*`,
      },
      {
        source: "/feed",
        destination: `${BACKEND_URL}/feed`,
      },
    ]
  },
}

export default nextConfig
