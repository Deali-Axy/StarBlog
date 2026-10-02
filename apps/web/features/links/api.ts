"use client"

import { useMutation } from "@tanstack/react-query"

import { apiPost } from "@/lib/api/client"
import type { LinkExchangeApplicationRequest, LinkResponse } from "@/lib/api/types"

export function useApplyLinkExchange() {
  return useMutation({
    mutationFn: (body: LinkExchangeApplicationRequest) => apiPost<LinkResponse>("/api/v1/site/link-exchanges", body),
  })
}
