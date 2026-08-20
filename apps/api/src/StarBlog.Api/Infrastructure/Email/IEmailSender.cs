namespace StarBlog.Api.Infrastructure.Email;

/// <summary>
/// 邮件发送的可替换边界。
/// </summary>
public interface IEmailSender {
    Task SendAsync(string subject, string htmlBody, string toName, string toAddress, CancellationToken cancellationToken = default);
}
