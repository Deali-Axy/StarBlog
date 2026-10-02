"use client"

import { FormEvent, useEffect, useMemo, useState, useSyncExternalStore } from "react"
import { RefreshCw, Reply } from "lucide-react"
import { toast } from "sonner"

import { useCreateComment, useComments, useSendCommentOtp } from "@/features/comments/api"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { ApiError, type CommentResponse } from "@/lib/api/types"
import { formatDate } from "@/lib/format"
import { getAuthorInitials } from "@/lib/site"

const PROFILE_KEY = "starblog.comment.profile"

interface StoredProfile {
  userName: string
  email: string
  url: string
}

const emptyProfile: StoredProfile = { userName: "", email: "", url: "" }

function readProfile(): StoredProfile {
  if (typeof window === "undefined") return emptyProfile
  try {
    const raw = localStorage.getItem(PROFILE_KEY)
    if (!raw) return emptyProfile
    const parsed = JSON.parse(raw) as Partial<StoredProfile>
    return {
      userName: parsed.userName ?? "",
      email: parsed.email ?? "",
      url: parsed.url ?? "",
    }
  } catch {
    return emptyProfile
  }
}

function subscribeProfile(onChange: () => void) {
  window.addEventListener("storage", onChange)
  window.addEventListener("starblog-comment-profile", onChange)
  return () => {
    window.removeEventListener("storage", onChange)
    window.removeEventListener("starblog-comment-profile", onChange)
  }
}

function saveProfile(profile: StoredProfile) {
  localStorage.setItem(PROFILE_KEY, JSON.stringify(profile))
  window.dispatchEvent(new Event("starblog-comment-profile"))
}

function countComments(nodes: CommentResponse[]): number {
  return nodes.reduce((sum, node) => sum + 1 + countComments(node.comments ?? []), 0)
}

function CommentItem({
  comment,
  onReply,
}: {
  comment: CommentResponse
  onReply: (comment: CommentResponse) => void
}) {
  const name = comment.anonymousUserName || "匿名"
  const pending = !comment.visible || comment.isNeedAudit
  return (
    <div className="space-y-4">
      <div className="flex gap-3">
        <Avatar className="size-10">
          <AvatarFallback>{getAuthorInitials(name)}</AvatarFallback>
        </Avatar>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <p className="font-medium">{name}</p>
            {pending ? <Badge variant="secondary">待审核</Badge> : null}
            <span className="text-xs text-muted-foreground">{formatDate(comment.createdTime, "YYYY-MM-DD HH:mm")}</span>
          </div>
          <p className="mt-2 whitespace-pre-wrap text-sm leading-relaxed">{comment.content}</p>
          {!pending ? (
            <Button variant="ghost" size="sm" className="mt-1 h-7 px-2 text-xs" onClick={() => onReply(comment)}>
              <Reply />
              回复
            </Button>
          ) : null}
        </div>
      </div>
      {comment.comments && comment.comments.length > 0 ? (
        <div className="ml-6 space-y-4 border-l border-border/70 pl-4">
          {comment.comments.map((child) => (
            <CommentItem key={child.id} comment={child} onReply={onReply} />
          ))}
        </div>
      ) : null}
    </div>
  )
}

export function CommentSection({ postId }: { postId: string }) {
  const commentsQuery = useComments(postId)
  const sendOtp = useSendCommentOtp()
  const createComment = useCreateComment(postId)
  const storedProfile = useSyncExternalStore(subscribeProfile, readProfile, () => emptyProfile)
  const [draft, setDraft] = useState<StoredProfile | null>(null)
  const profile = draft ?? storedProfile
  const setProfile = (updater: (current: StoredProfile) => StoredProfile) => {
    setDraft((current) => updater(current ?? storedProfile))
  }
  const [otp, setOtp] = useState("")
  const [content, setContent] = useState("")
  const [replyTo, setReplyTo] = useState<CommentResponse | null>(null)
  const [pending, setPending] = useState<CommentResponse[]>([])
  const [cooldown, setCooldown] = useState(0)

  useEffect(() => {
    if (cooldown <= 0) return
    const timer = window.setTimeout(() => setCooldown((value) => value - 1), 1000)
    return () => window.clearTimeout(timer)
  }, [cooldown])

  const tree = useMemo(() => [...(commentsQuery.data ?? []), ...pending], [commentsQuery.data, pending])
  const total = countComments(tree)

  async function handleOtp() {
    const email = profile.email.trim()
    if (!email) {
      toast.error("请先填写邮箱")
      return
    }
    try {
      const result = await sendOtp.mutateAsync(email)
      toast.success(result.message)
      setCooldown(60)
    } catch (error) {
      toast.error(error instanceof ApiError ? error.message : "验证码发送失败")
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (!profile.email.trim() || !profile.userName.trim() || !content.trim() || !otp.trim()) {
      toast.error("请填写邮箱、验证码、昵称和评论内容")
      return
    }
    try {
      const result = await createComment.mutateAsync({
        postId,
        parentId: replyTo?.id,
        content: content.trim(),
        userName: profile.userName.trim(),
        email: profile.email.trim(),
        url: profile.url.trim() || null,
        emailOtp: otp.trim(),
      })
      saveProfile(profile)
      setDraft(null)
      setOtp("")
      setContent("")
      setReplyTo(null)
      if (!result.comment.visible || result.comment.isNeedAudit) {
        setPending((items) => [result.comment, ...items])
        toast.message(result.message, { description: "该评论待审核，通过后会公开显示。" })
      } else {
        toast.success(result.message)
      }
    } catch (error) {
      toast.error(error instanceof ApiError ? error.message : "发表失败")
    }
  }

  return (
    <section className="mt-16 border-t border-border/70 pt-10">
      <div className="mb-8 flex items-end justify-between">
        <h2 className="text-xl">讨论</h2>
        <span className="text-sm text-muted-foreground">共 {total} 条</span>
      </div>
      <div className="space-y-8">
        <form className="space-y-3" onSubmit={handleSubmit}>
          <div className="grid gap-3 md:grid-cols-4">
            <Input
              type="email"
              placeholder="邮箱地址"
              value={profile.email}
              onChange={(event) => setProfile((current) => ({ ...current, email: event.target.value }))}
              required
            />
            <div className="flex gap-2">
              <Input
                placeholder="验证码"
                value={otp}
                onChange={(event) => setOtp(event.target.value)}
                maxLength={4}
                required
              />
              <Button type="button" variant="outline" onClick={() => void handleOtp()} disabled={sendOtp.isPending || cooldown > 0}>
                {cooldown > 0 ? `${cooldown}s` : "发送"}
              </Button>
            </div>
            <Input
              placeholder="昵称"
              value={profile.userName}
              onChange={(event) => setProfile((current) => ({ ...current, userName: event.target.value }))}
              required
            />
            <Input
              placeholder="网址（可选）"
              value={profile.url}
              onChange={(event) => setProfile((current) => ({ ...current, url: event.target.value }))}
            />
          </div>
          <Textarea
            placeholder="友善的评论是交流的起点"
            maxLength={300}
            value={content}
            onChange={(event) => setContent(event.target.value)}
            required
          />
          <p className="text-xs text-muted-foreground">良言一句三冬暖，恶语伤人六月寒。</p>
          {replyTo ? (
            <div className="flex items-center gap-2 text-sm">
              <span className="text-muted-foreground">回复</span>
              <Badge variant="secondary">@{replyTo.anonymousUserName || "匿名"}</Badge>
              <Button type="button" variant="ghost" size="sm" onClick={() => setReplyTo(null)}>
                取消
              </Button>
            </div>
          ) : null}
          <div className="flex justify-between">
            <Button type="button" variant="outline" onClick={() => void commentsQuery.refetch()} disabled={commentsQuery.isFetching}>
              <RefreshCw />
              刷新
            </Button>
            <Button type="submit" disabled={createComment.isPending}>
              发表
            </Button>
          </div>
        </form>

        {commentsQuery.isLoading ? (
          <p className="text-sm text-muted-foreground">加载评论中…</p>
        ) : tree.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">暂无讨论</p>
        ) : (
          <div className="space-y-6">
            {tree.map((comment) => (
              <CommentItem key={comment.id} comment={comment} onReply={setReplyTo} />
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
