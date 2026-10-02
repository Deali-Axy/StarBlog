"use client"

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import { apiPost } from "@/lib/api/client"
import { getCommentsByPost } from "@/lib/api/queries"
import type { CommentCreateRequest, CommentCreateResult } from "@/lib/api/types"

export const commentKeys = {
  byPost: (postId: string) => ["comments", "by-post", postId] as const,
}

export function useComments(postId: string) {
  return useQuery({
    queryKey: commentKeys.byPost(postId),
    queryFn: () => getCommentsByPost(postId),
  })
}

export function useSendCommentOtp() {
  return useMutation({
    mutationFn: (email: string) =>
      apiPost<{ message: string }>("/api/v1/comments/email-otp", undefined, { email }),
  })
}

export function useCreateComment(postId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CommentCreateRequest) => apiPost<CommentCreateResult>("/api/v1/comments", body),
    onSuccess: (result) => {
      if (result.comment.visible) {
        void queryClient.invalidateQueries({ queryKey: commentKeys.byPost(postId) })
      }
    },
  })
}
