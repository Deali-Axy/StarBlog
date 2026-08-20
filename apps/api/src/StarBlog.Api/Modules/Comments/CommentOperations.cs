using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StarBlog.Api.Infrastructure.Email;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Comments.Domain;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Notifications;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Comments;

/// <summary>
/// 评论、OTP、审核与回复通知。
/// </summary>
public sealed class CommentOperations {
    private static readonly Regex EmailPattern = new(@"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+", RegexOptions.Compiled);
    private readonly StarBlogDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly IEmailSender _emailSender;
    private readonly INotificationOutbox _outbox;
    private readonly IPublishedContentQueries _posts;
    private readonly ISiteConfiguration _configuration;
    private readonly CommentFilter _filter;
    private readonly IClock _clock;

    public CommentOperations(
        StarBlogDbContext db,
        IMemoryCache cache,
        IEmailSender emailSender,
        INotificationOutbox outbox,
        IPublishedContentQueries posts,
        ISiteConfiguration configuration,
        CommentFilter filter,
        IClock clock) {
        _db = db;
        _cache = cache;
        _emailSender = emailSender;
        _outbox = outbox;
        _posts = posts;
        _configuration = configuration;
        _filter = filter;
        _clock = clock;
    }

    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Length >= 7 && EmailPattern.IsMatch(email);

    public async Task<PageResult<CommentResponse>> GetPagedAsync(int page, int pageSize, string? postId, bool adminMode, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.Comments.AsNoTracking().Include(comment => comment.AnonymousUser).AsQueryable();
        if (!adminMode) query = query.Where(comment => comment.Visible);
        if (!string.IsNullOrWhiteSpace(postId)) query = query.Where(comment => comment.PostId == postId);
        query = query.OrderByDescending(comment => comment.CreatedTime);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<CommentResponse> {
            Items = items.Select(comment => CommentResponse.From(comment)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<CommentResponse>> GetTreeAsync(string postId, CancellationToken cancellationToken) {
        var comments = await _db.Comments.AsNoTracking()
            .Include(comment => comment.AnonymousUser)
            .Where(comment => comment.PostId == postId && comment.Visible)
            .ToListAsync(cancellationToken);
        return BuildTree(comments, null);
    }

    /// <summary>生成 5 分钟有效的邮箱验证码。已有未过期验证码时拒绝重复发送。</summary>
    public async Task<(bool Sent, string? Otp, string Message)> GenerateOtpAsync(string email, CancellationToken cancellationToken) {
        var cacheKey = $"comment-otp-{email}";
        if (_cache.TryGetValue(cacheKey, out string? existing)) {
            return (false, existing, "上一个验证码还在有效期内，请勿重复请求验证码");
        }

        var otp = Random.Shared.NextInt64(1000, 9999).ToString();
        await _emailSender.SendAsync("[StarBlog]邮箱验证码", $"<p>欢迎访问StarBlog！验证码：{otp}</p><p>如果您没有进行任何操作，请忽略此邮件。</p>", email, email, cancellationToken);
        _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));
        return (true, otp, "发送邮件验证码成功，五分钟内有效");
    }

    public bool VerifyOtp(string email, string otp, bool clear = true) {
        var cacheKey = $"comment-otp-{email}";
        if (!_cache.TryGetValue<string>(cacheKey, out var value) || otp != value) return false;
        if (clear) _cache.Remove(cacheKey);
        return true;
    }

    /// <summary>发表评论。命中敏感词时进入待审核。</summary>
    public async Task<(CommentResponse Comment, string Message)> AddAsync(CommentCreateRequest request, string? ip, string? userAgent, CancellationToken cancellationToken = default) {
        var anonymous = await GetOrCreateAnonymousAsync(request.UserName, request.Email, request.Url, ip, cancellationToken);
        var comment = new Comment {
            Id = Guid.NewGuid().ToString("N")[..16],
            ParentId = request.ParentId,
            PostId = request.PostId,
            AnonymousUserId = anonymous.Id,
            UserAgent = userAgent,
            Content = request.Content,
            CreatedTime = _clock.Now,
            UpdatedTime = _clock.Now
        };

        string message;
        if (_filter.ContainsBadWord(request.Content)) {
            comment.IsNeedAudit = true;
            comment.Visible = false;
            message = "小管家发现您可能使用了不良用语，该评论将在审核通过后展示~";
        }
        else {
            comment.Visible = true;
            message = "评论由小管家审核通过，感谢您参与讨论~";
        }

        _db.Comments.Add(comment);
        if (comment.ParentId != null && comment.Visible) {
            await EnqueueReplyNotificationAsync(comment, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (CommentResponse.From(comment), message);
    }

    public async Task<CommentResponse?> AcceptAsync(string id, string? reason, CancellationToken cancellationToken) {
        var comment = await _db.Comments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (comment == null) return null;
        var wasNeedAudit = comment.IsNeedAudit;
        var wasVisible = comment.Visible;
        comment.Visible = true;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        comment.UpdatedTime = _clock.Now;
        if (comment.ParentId != null && (wasNeedAudit || !wasVisible)) {
            await EnqueueReplyNotificationAsync(comment, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return CommentResponse.From(comment);
    }

    public async Task<CommentResponse?> RejectAsync(string id, string? reason, CancellationToken cancellationToken) {
        var comment = await _db.Comments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (comment == null) return null;
        comment.Visible = false;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        comment.UpdatedTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return CommentResponse.From(comment);
    }

    private async Task<AnonymousUser> GetOrCreateAnonymousAsync(string name, string email, string? url, string? ip, CancellationToken cancellationToken) {
        var item = await _db.AnonymousUsers.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        if (item == null) {
            item = new AnonymousUser {
                Id = Guid.NewGuid().ToString("N")[..16],
                Email = email,
                CreatedTime = _clock.Now
            };
            _db.AnonymousUsers.Add(item);
        }

        item.Name = name;
        item.Url = url;
        item.Ip = ip;
        item.UpdatedTime = _clock.Now;
        return item;
    }

    private async Task EnqueueReplyNotificationAsync(Comment reply, CancellationToken cancellationToken) {
        if (reply.ParentId == null || !reply.Visible || string.IsNullOrWhiteSpace(reply.AnonymousUserId)) return;
        var replier = await _db.AnonymousUsers.FindAsync([reply.AnonymousUserId], cancellationToken);
        var parent = await _db.Comments.FindAsync([reply.ParentId], cancellationToken);
        if (replier == null || parent == null || string.IsNullOrWhiteSpace(parent.AnonymousUserId)) return;
        var parentAuthor = parent.AnonymousUser
                           ?? await _db.AnonymousUsers.FindAsync([parent.AnonymousUserId], cancellationToken);
        if (parentAuthor == null) return;
        if (string.Equals(parent.AnonymousUserId, reply.AnonymousUserId, StringComparison.Ordinal)) return;
        if (string.Equals(parentAuthor.Email, replier.Email, StringComparison.OrdinalIgnoreCase)) return;

        var post = await _posts.GetByIdAsync(reply.PostId, cancellationToken);
        var baseUrl = (await _configuration.GetAsync("host", cancellationToken) ?? "https://blog.deali.cn").TrimEnd('/');
        var postUrl = post?.Slug != null
            ? $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}"
            : $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(reply.PostId)}";
        var encoder = HtmlEncoder.Default;
        var subject = $"[StarBlog]你在《{encoder.Encode(post?.Title ?? reply.PostId)}》下的评论收到了回复";
        var body = new StringBuilder();
        body.AppendLine($"<p>你好，{encoder.Encode(parentAuthor.Name)}：</p>");
        body.AppendLine($"<p>你的评论收到了 <b>{encoder.Encode(replier.Name)}</b> 的回复：</p>");
        body.AppendLine($"<blockquote style=\"margin:12px 0;padding:10px 12px;border-left:4px solid #ddd;background:#fafafa;\">{encoder.Encode(reply.Content).Replace("\n", "<br>")}</blockquote>");
        body.AppendLine($"<p>点击查看：<a href=\"{encoder.Encode(postUrl)}\">{encoder.Encode(postUrl)}</a></p>");
        await _outbox.EnqueueEmailAsync(subject, body.ToString(), parentAuthor.Name, parentAuthor.Email, $"comment-reply:{reply.Id}", cancellationToken);
    }

    private static List<CommentResponse> BuildTree(List<Comment> comments, string? parentId) {
        return comments
            .Where(comment => comment.ParentId == parentId)
            .Select(comment => {
                var response = CommentResponse.From(comment);
                var children = BuildTree(comments, comment.Id);
                return new CommentResponse {
                    Id = response.Id,
                    ParentId = response.ParentId,
                    PostId = response.PostId,
                    Content = response.Content,
                    Visible = response.Visible,
                    IsNeedAudit = response.IsNeedAudit,
                    Reason = response.Reason,
                    CreatedTime = response.CreatedTime,
                    AnonymousUserName = response.AnonymousUserName,
                    Comments = children.Count == 0 ? null : children
                };
            })
            .ToList();
    }
}
