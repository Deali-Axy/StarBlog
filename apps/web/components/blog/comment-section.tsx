"use client"

import { useI18n } from "@/components/app-providers"

import { RefreshCw, Reply } from "lucide-react"
import { FormEvent, createContext, useContext, useEffect, useMemo, useState, useSyncExternalStore, type ReactNode } from "react"
import { toast } from "sonner"

import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { useComments, useCreateComment, useSendCommentOtp } from "@/features/comments/api"
import { ApiError, type CommentResponse } from "@/lib/api/types"
import { formatDate } from "@/lib/format"
import { getAuthorInitials } from "@/lib/site"
import { headingClassName } from "@/lib/styles"
import { cn } from "@/lib/utils"

const PROFILE_KEY = "starblog.comment.profile"

interface StoredProfile {
  userName: string
  email: string
  url: string
}

const emptyProfile: StoredProfile = { userName: "", email: "", url: "" }

let cachedProfileRaw: string | null = null
let cachedProfile = emptyProfile

function readProfile(): StoredProfile {
  if(typeof window === "undefined") return emptyProfile
  try {
    const raw = localStorage.getItem(PROFILE_KEY)
    if(raw === cachedProfileRaw) return cachedProfile
    cachedProfileRaw = raw
    if(!raw) return cachedProfile = emptyProfile
    const parsed = JSON.parse(raw) as Partial<StoredProfile>
    return cachedProfile = {
      userName: parsed.userName ?? "",
      email: parsed.email ?? "",
      url: parsed.url ?? "",
    }
  } catch {
    return cachedProfile = emptyProfile
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
  const { t } = useI18n()
  const name = comment.anonymousUserName || t.anonymous
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
            {pending ? <Badge variant="secondary">{t.pending}</Badge> : null}
            <span className="text-xs text-muted-foreground">{formatDate(comment.createdTime, "YYYY-MM-DD HH:mm")}</span>
          </div>
          <p className="mt-2 whitespace-pre-wrap text-sm leading-relaxed">{comment.content}</p>
          {!pending ? (
            <Button variant="ghost" size="sm" className="mt-1 h-7 px-2 text-xs" onClick={() => onReply(comment)}>
              <Reply />
              {t.reply}
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

function useCommentSession({ postId }: { postId: string }) {
  const { locale, t } = useI18n()
  const commentsQuery = useComments(postId)
  const sendOtp = useSendCommentOtp()
  const createComment = useCreateComment(postId)
  const storedProfile = useSyncExternalStore(subscribeProfile, readProfile, () => emptyProfile)
  const [draft, setDraft] = useState<StoredProfile | null>(null)
  const profile = draft ?? storedProfile
  const setProfile = (updater: (current: StoredProfile) => StoredProfile) => {
    setDraft((current) => updater(current ?? storedProfile))
  }
  const [expanded, setExpanded] = useState(false)
  const [otp, setOtp] = useState("")
  const [content, setContent] = useState("")
  const [replyTo, setReplyTo] = useState<CommentResponse | null>(null)
  const [pending, setPending] = useState<CommentResponse[]>([])
  const [cooldown, setCooldown] = useState(0)

  useEffect(() => {
    if(cooldown <= 0) return
    const timer = window.setTimeout(() => setCooldown((value) => value - 1), 1000)
    return () => window.clearTimeout(timer)
  }, [cooldown])

  const tree = useMemo(() => [...(commentsQuery.data ?? []), ...pending], [commentsQuery.data, pending])
  const total = countComments(tree)

  async function handleOtp() {
    const email = profile.email.trim()
    if(!email) {
      toast.error(t.emailRequired)
      return
    }
    try {
      await sendOtp.mutateAsync(email)
      toast.success(t.otpSent)
      setCooldown(60)
    } catch(error) {
      toast.error(locale === "zh" && error instanceof ApiError ? error.message : t.otpFailed)
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if(!profile.email.trim() || !profile.userName.trim() || !content.trim() || !otp.trim()) {
      toast.error(t.commentRequired)
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
      if(!result.comment.visible || result.comment.isNeedAudit) {
        setPending((items) => [result.comment, ...items])
        toast.message(t.pending, { description: t.commentPending })
      } else {
        toast.success(t.commentPublished)
      }
    } catch(error) {
      toast.error(locale === "zh" && error instanceof ApiError ? error.message : t.publishFailed)
    }
  }

  return { expanded, setExpanded, t, commentsQuery, sendOtp, createComment, profile, setProfile, otp, setOtp, content, setContent, replyTo, setReplyTo, tree, total, cooldown, handleOtp, handleSubmit }
}
const CommentSessionContext = createContext<ReturnType<typeof useCommentSession> | null>(null)
export function CommentSessionProvider({ postId, children }: { postId: string; children: ReactNode }) {
  const session = useCommentSession({ postId })
  return <CommentSessionContext.Provider value={session}>{children}</CommentSessionContext.Provider>
}
export function useCommentSessionState() {
  const session = useContext(CommentSessionContext)
  if(!session) throw new Error("Comment session is required")
  return session
}
export function CommentSection() {
  const session = useCommentSessionState()
  const { expanded, setExpanded, t, commentsQuery, sendOtp, createComment, profile, setProfile, otp, setOtp, content, setContent, replyTo, setReplyTo, tree, total, cooldown, handleOtp, handleSubmit } = session

  return (
    <section id="article-comments" data-article-comments tabIndex={-1} aria-labelledby="article-comments-title" className="mt-10 scroll-mt-[calc(var(--blog-header-height)+1.5rem)] border-t border-border/70 pt-8 outline-none">
      <div className="mb-8 flex items-end justify-between">
        <h2 id="article-comments-title" className={cn(headingClassName, "text-xl")}>{t.discussion}</h2>
        <span className="text-sm text-muted-foreground">{t.commentCount(total)}</span>
      </div>
      <div className="space-y-8">
        <form data-comment-form className="space-y-3" onSubmit={handleSubmit}>
          <Textarea
            aria-label={t.commentPlaceholder}
            placeholder={t.commentPlaceholder}
            maxLength={300}
            value={content}
            onFocus={() => setExpanded(true)}
            onChange={(event) => setContent(event.target.value)}
            required
          />
          <div hidden={!expanded && !replyTo} className="space-y-3">
            <div className="grid gap-3 md:grid-cols-4">
              <Input
                type="email"
                aria-label={t.emailAddress}
                placeholder={t.emailAddress}
                value={profile.email}
                onChange={(event) => setProfile((current) => ({ ...current, email: event.target.value }))}
                required
              />
              <div className="flex gap-2">
                <Input
                  aria-label={t.otp}
                  placeholder={t.otp}
                  value={otp}
                  onChange={(event) => setOtp(event.target.value)}
                  maxLength={4}
                  required
                />
                <Button type="button" variant="outline" onClick={() => void handleOtp()} disabled={sendOtp.isPending || cooldown > 0}>
                  {cooldown > 0 ? `${cooldown}s` : t.send}
                </Button>
              </div>
              <Input
                aria-label={t.nickname}
                placeholder={t.nickname}
                value={profile.userName}
                onChange={(event) => setProfile((current) => ({ ...current, userName: event.target.value }))}
                required
              />
              <Input
                aria-label={t.optionalUrl}
                placeholder={t.optionalUrl}
                value={profile.url}
                onChange={(event) => setProfile((current) => ({ ...current, url: event.target.value }))}
              />
            </div>
            <p className="text-xs text-muted-foreground">{t.commentHint}</p>
            {replyTo ? (
              <div className="flex items-center gap-2 text-sm">
                <span className="text-muted-foreground">{t.reply}</span>
                <Badge variant="secondary">@{replyTo.anonymousUserName || t.anonymous}</Badge>
                <Button type="button" variant="ghost" size="sm" onClick={() => setReplyTo(null)}>
                  {t.cancel}
                </Button>
              </div>
            ) : null}
            <div className="flex justify-between">
              <Button type="button" variant="outline" onClick={() => void commentsQuery.refetch()} disabled={commentsQuery.isFetching}>
                <RefreshCw />
                {t.refresh}
              </Button>
              <Button type="submit" disabled={createComment.isPending}>
                {t.publish}
              </Button>
            </div>
          </div>
        </form>

        {commentsQuery.isLoading ? (
          <p className="text-sm text-muted-foreground">{t.loadingComments}</p>
        ) : tree.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">{t.noDiscussion}</p>
        ) : (
          <div className="space-y-6">
            {tree.map((comment) => (
              <CommentItem key={comment.id} comment={comment} onReply={(comment) => { setReplyTo(comment); setExpanded(true); document.querySelector<HTMLTextAreaElement>("[data-comment-form] textarea")?.focus() }} />
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
