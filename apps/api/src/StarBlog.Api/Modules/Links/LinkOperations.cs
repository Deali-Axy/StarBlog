using System.Text;
using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Links.Domain;
using StarBlog.Api.Modules.Notifications;

namespace StarBlog.Api.Modules.Links;

/// <summary>
/// 友链 CRUD、申请审核，以及审核后的 Outbox 邮件。审核与入队在同一 SaveChanges 事务中完成。
/// </summary>
public sealed class LinkOperations : IPublicLinkCatalog {
    private readonly StarBlogDbContext _db;
    private readonly INotificationOutbox _outbox;
    private readonly IClock _clock;

    public LinkOperations(StarBlogDbContext db, INotificationOutbox outbox, IClock clock) {
        _db = db;
        _outbox = outbox;
        _clock = clock;
    }

    /// <summary>返回前台可见友链。</summary>
    public async Task<IReadOnlyList<LinkResponse>> GetVisibleAsync(CancellationToken cancellationToken = default) {
        var links = await _db.Links.AsNoTracking().Where(link => link.Visible).ToListAsync(cancellationToken);
        return links.Select(LinkResponse.From).ToList();
    }

    /// <summary>管理端返回全部友链。</summary>
    public async Task<IReadOnlyList<LinkResponse>> GetAllAsync(CancellationToken cancellationToken) {
        var links = await _db.Links.AsNoTracking().ToListAsync(cancellationToken);
        return links.Select(LinkResponse.From).ToList();
    }

    public async Task<LinkResponse?> GetByIdAsync(int id, CancellationToken cancellationToken) {
        var link = await _db.Links.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return link == null ? null : LinkResponse.From(link);
    }

    /// <summary>创建友链。</summary>
    public async Task<LinkResponse> CreateAsync(LinkUpsertRequest request, CancellationToken cancellationToken) {
        var link = new Link {
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            Visible = request.Visible
        };
        _db.Links.Add(link);
        await _db.SaveChangesAsync(cancellationToken);
        return LinkResponse.From(link);
    }

    /// <summary>更新友链；不存在时返回 null。</summary>
    public async Task<LinkResponse?> UpdateAsync(int id, LinkUpsertRequest request, CancellationToken cancellationToken) {
        var link = await _db.Links.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (link == null) return null;
        link.Name = request.Name;
        link.Description = request.Description;
        link.Url = request.Url;
        link.Visible = request.Visible;
        await _db.SaveChangesAsync(cancellationToken);
        return LinkResponse.From(link);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) {
        var link = await _db.Links.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (link == null) return false;
        _db.Links.Remove(link);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<LinkExchangeResponse>> GetExchangesAsync(CancellationToken cancellationToken) {
        var items = await _db.LinkExchanges.AsNoTracking().OrderByDescending(item => item.ApplyTime).ToListAsync(cancellationToken);
        return items.Select(LinkExchangeResponse.From).ToList();
    }

    public async Task<LinkExchangeResponse?> GetExchangeAsync(int id, CancellationToken cancellationToken) {
        var item = await _db.LinkExchanges.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        return item == null ? null : LinkExchangeResponse.From(item);
    }

    /// <summary>提交友链申请并入队确认邮件。</summary>
    public async Task<(LinkExchangeResponse? Response, string? Error)> ApplyAsync(
        LinkExchangeApplicationRequest request,
        CancellationToken cancellationToken) {
        var duplicated = await _db.LinkExchanges.AnyAsync(item => item.Url.Contains(request.Url), cancellationToken);
        if (duplicated) return (null, "相同网址的友链申请已提交");

        var item = new LinkExchange {
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            WebMaster = request.WebMaster,
            Email = request.Email,
            Verified = false,
            ApplyTime = _clock.Now
        };
        _db.LinkExchanges.Add(item);
        await EnqueueMailAsync(
            item,
            "友链申请已提交",
            "友链申请已提交，正在处理中，请及时关注邮件通知~",
            $"link-exchange-apply:{item.Url}",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return (LinkExchangeResponse.From(item), null);
    }

    /// <summary>审核通过：显示对应友链并发送通过邮件；拒绝：删除对应友链并发送拒绝邮件。</summary>
    public async Task<LinkExchangeResponse?> DecideAsync(int id, bool accepted, string? reason, CancellationToken cancellationToken) {
        var item = await _db.LinkExchanges.FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (item == null) return null;

        item.Verified = accepted;
        item.Reason = reason;

        var link = await _db.Links.FirstOrDefaultAsync(entry => entry.Name == item.Name, cancellationToken);
        if (accepted) {
            await EnqueueMailAsync(item, "友链申请结果反馈", "您好，友链申请已通过！感谢支持，欢迎互访哦~", $"link-exchange:{item.Id}:友链申请结果反馈:accept", cancellationToken);
            if (link == null) {
                _db.Links.Add(new Link {
                    Name = item.Name,
                    Description = item.Description,
                    Url = item.Url,
                    Visible = true
                });
            }
            else {
                link.Visible = true;
            }
        }
        else {
            await EnqueueMailAsync(item, "友链申请结果反馈", "很抱歉，友链申请未通过！建议您查看补充信息，调整后再次进行申请，感谢您的理解与支持~", $"link-exchange:{item.Id}:友链申请结果反馈:reject", cancellationToken);
            if (link != null) _db.Links.Remove(link);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return LinkExchangeResponse.From(item);
    }

    public async Task<bool> DeleteExchangeAsync(int id, CancellationToken cancellationToken) {
        var item = await _db.LinkExchanges.FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (item == null) return false;
        _db.LinkExchanges.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnqueueMailAsync(LinkExchange item, string subject, string message, string dedupKey, CancellationToken cancellationToken) {
        var body = new StringBuilder();
        body.AppendLine($"<p>{message}</p>");
        body.AppendLine("<br>");
        body.AppendLine("<p>以下是您申请的友链信息：</p>");
        body.AppendLine($"<p>网站名称：{item.Name}</p>");
        body.AppendLine($"<p>介绍：{item.Description}</p>");
        body.AppendLine($"<p>网址：{item.Url}</p>");
        body.AppendLine($"<p>站长：{item.WebMaster}</p>");
        if (item.Reason != null) body.AppendLine($"<p>补充信息：{item.Reason}</p>");
        await _outbox.EnqueueEmailAsync(
            $"[StarBlog]{subject}",
            body.ToString(),
            item.WebMaster,
            item.Email,
            dedupKey,
            cancellationToken);
    }
}
