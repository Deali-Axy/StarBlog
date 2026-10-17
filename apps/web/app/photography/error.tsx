"use client"

import { PhotographyUnavailable } from "@/components/photography/photography-unavailable"

export default function PhotographyError({ reset }: { reset: () => void }) {
  return <PhotographyUnavailable onRetry={reset} />
}
