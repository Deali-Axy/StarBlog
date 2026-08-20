using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels;
using FreeSql;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StarBlog.Data.Models;
using StarBlog.Content.Utils;
using StarBlog.Web.Services.OutboxServices;
using StarBlog.Web.Criteria;
using X.PagedList;

namespace StarBlog.Web.Services;

public class CommentService {
    private readonly ILogger<CommentService> _logger;
    private readonly IBaseRepository<Comment> _commentRepo;
    private readonly IBaseRepository<AnonymousUser> _anonymousRepo;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IMemoryCache _memoryCache;
    private readonly EmailService _emailService;
    private readonly OutboxService _outboxService;
    private readonly IConfiguration _configuration;

    public CommentService(ILogger<CommentService> logger, IBaseRepository<Comment> commentRepo,
        IBaseRepository<AnonymousUser> anonymousRepo, IBaseRepository<Post> postRepo, IMemoryCache memoryCache, EmailService emailService,
        OutboxService outboxService, IConfiguration configuration) {
        _logger = logger;
        _commentRepo = commentRepo;
        _anonymousRepo = anonymousRepo;
        _postRepo = postRepo;
        _memoryCache = memoryCache;
        _emailService = emailService;
        _outboxService = outboxService;
        _configuration = configuration;
    }

    private List<Comment>? GetCommentsTree(List<Comment> commentsList, string? parentId = null) {
        var comments = commentsList.Where(e => e.Visible).ToList();

        if (parentId != null) {
            comments = comments.Where(e => e.ParentId == parentId).ToList();
        }

        if (comments.Count == 0) return null;

        return comments.Select(comment => {
            comment.Comments = GetCommentsTree(commentsList, comment.Id);
            return comment;
        }).ToList();
    }

    public async Task<List<Comment>?> GetAll(string postId) {
        var comments = await _commentRepo.Where(a => a.PostId == postId).ToListAsync();
        return GetCommentsTree(comments);
    }

    public async Task<(List<Comment>, PaginationMetadata)> GetPagedList(CommentQueryParameters param,
        bool adminMode = false, bool includePost = false) {
        var querySet = _commentRepo.Select;

        if (!adminMode) {
            querySet = querySet.Where(a => a.Visible);
        }

        if (includePost) {
            querySet = querySet.Include(a => a.Post);
        }

        if (!string.IsNullOrWhiteSpace(param.PostId)) {
            querySet = querySet.Where(a => a.PostId == param.PostId);
        }

        if (!string.IsNullOrWhiteSpace(param.Search)) {
            querySet = querySet.Where(a => a.Content.Contains(param.Search));
        }

        // 排序
        if (!string.IsNullOrEmpty(param.SortBy)) {
            // 是否升序
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        var data = await querySet.Page(param.Page, param.PageSize)
            .Include(a => a.AnonymousUser)
            .Include(a => a.Parent.AnonymousUser)
            .ToListAsync();

        IPagedList<Comment> pagedList = new StaticPagedList<Comment>(data, param.Page, param.PageSize, Convert.ToInt32(await querySet.CountAsync()));

        return (data, pagedList.ToPaginationMetadata());
    }

    public async Task<Comment?> GetById(string id) {
        return await _commentRepo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Comment> Accept(Comment comment, string? reason = null) {
        var wasNeedAudit = comment.IsNeedAudit;
        var wasVisible = comment.Visible;
        comment.Visible = true;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        await _commentRepo.UpdateAsync(comment);

        if (comment.ParentId != null && (wasNeedAudit || !wasVisible)) {
            await EnqueueReplyNotificationIfNeededAsync(comment);
        }
        return comment;
    }

    public async Task<Comment> Reject(Comment comment, string reason) {
        comment.Visible = false;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        await _commentRepo.UpdateAsync(comment);
        return comment;
    }

    public async Task<AnonymousUser?> GetAnonymousUser(string email) {
        return await _anonymousRepo.Where(a => a.Email == email).FirstAsync();
    }

    public async Task<AnonymousUser> GetOrCreateAnonymousUser(string name, string email, string? url, string? ip) {
        var item =
            await _anonymousRepo.Where(a => a.Email == email).FirstAsync() ??
            new AnonymousUser { Id = GuidUtils.GuidTo16String(), Email = email };

        item.Name = name;
        item.Ip = ip;
        item.Url = url;
        item.UpdatedTime = DateTime.Now;
        return await _anonymousRepo.InsertOrUpdateAsync(item);
    }

    /// <summary>
    /// 检查邮箱地址是否有效
    /// </summary>
    public static bool IsValidEmail(string email) {
        if (string.IsNullOrEmpty(email) || email.Length < 7) {
            return false;
        }

        var match = Regex.Match(email, @"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+");
        var isMatch = match.Success;
        return isMatch;
    }

    /// <summary>
    /// 生成邮箱验证码，发送验证码邮件
    /// </summary>
    public async Task<(bool, string?)> GenerateOtp(string email, bool mock = false) {
        var cacheKey = $"comment-otp-{email}";
        var hasCache = _memoryCache.TryGetValue<string>(cacheKey, out var existingValue);
        if (hasCache) return (false, existingValue);

        var otp = await _emailService.SendOtpMail(email, mock);
        _memoryCache.Set<string>(cacheKey, otp, new MemoryCacheEntryOptions {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return (true, otp);
    }

    /// <summary>
    /// 验证一次性密码
    /// </summary>
    /// <param name="clear">验证通过后是否清除</param>
    public bool VerifyOtp(string email, string otp, bool clear = true) {
        var cacheKey = $"comment-otp-{email}";
        _memoryCache.TryGetValue<string>(cacheKey, out var value);

        if (otp != value) return false;

        if (clear) _memoryCache.Remove(cacheKey);
        return true;
    }

    public async Task<Comment> Add(Comment comment) {
        comment.Id = GuidUtils.GuidTo16String();
        return await _commentRepo.InsertAsync(comment);
    }

    public async Task EnqueueReplyNotificationIfNeededAsync(Comment replyComment) {
        if (replyComment.ParentId == null) return;
        if (!replyComment.Visible) return;

        if (string.IsNullOrWhiteSpace(replyComment.AnonymousUserId)) return;
        var replier = await _anonymousRepo.Where(a => a.Id == replyComment.AnonymousUserId).FirstAsync();
        if (replier == null) return;

        var parent = await _commentRepo.Select
            .Where(a => a.Id == replyComment.ParentId)
            .Include(a => a.AnonymousUser)
            .FirstAsync();
        if (parent == null) return;
        if (parent.AnonymousUser == null) return;
        if (string.IsNullOrWhiteSpace(parent.AnonymousUser.Email)) return;

        if (string.Equals(parent.AnonymousUserId, replyComment.AnonymousUserId, StringComparison.Ordinal)) return;
        if (string.Equals(parent.AnonymousUser.Email, replier.Email, StringComparison.OrdinalIgnoreCase)) return;

        var baseUrl = _configuration["host"] ?? "https://blog.deali.cn";
        baseUrl = baseUrl.TrimEnd('/');

        var post = await _postRepo.Where(a => a.Id == replyComment.PostId).FirstAsync();
        var postUrl = post?.Slug != null
            ? $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}"
            : $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(replyComment.PostId)}";

        var safePostTitle = HtmlEncoder.Default.Encode(post?.Title ?? replyComment.PostId);
        var safeReplierName = HtmlEncoder.Default.Encode(replier.Name);
        var safeReplyContent = HtmlEncoder.Default.Encode(replyComment.Content).Replace("\n", "<br>");

        var subject = $"[StarBlog]你在《{safePostTitle}》下的评论收到了回复";
        var body = new StringBuilder();
        body.AppendLine($"<p>你好，{HtmlEncoder.Default.Encode(parent.AnonymousUser.Name)}：</p>");
        body.AppendLine($"<p>你的评论收到了 <b>{safeReplierName}</b> 的回复：</p>");
        body.AppendLine($"<blockquote style=\"margin:12px 0;padding:10px 12px;border-left:4px solid #ddd;background:#fafafa;\">{safeReplyContent}</blockquote>");
        body.AppendLine($"<p>点击查看：<a href=\"{HtmlEncoder.Default.Encode(postUrl)}\">{HtmlEncoder.Default.Encode(postUrl)}</a></p>");

        await _outboxService.EnqueueEmailAsync(
            subject,
            body.ToString(),
            parent.AnonymousUser.Name,
            parent.AnonymousUser.Email,
            dedupKey: $"comment-reply:{replyComment.Id}"
        );
    }
}
