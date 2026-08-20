using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StarBlog.Api.Infrastructure.Email;

/// <summary>
/// 通过 MailKit 发送 HTML 邮件。
/// </summary>
public sealed class SmtpEmailSender : IEmailSender {
    private const string Footer = "<br><p>本消息由 <a href=\"https://deali.cn\">StarBlog</a> 自动发送，无需回复。</p>";
    private readonly EmailAccountOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(Microsoft.Extensions.Options.IOptions<EmailAccountOptions> options, ILogger<SmtpEmailSender> logger) {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>连接 SMTP 并发送一封 HTML 邮件。</summary>
    public async Task SendAsync(string subject, string htmlBody, string toName, string toAddress, CancellationToken cancellationToken = default) {
        _logger.LogDebug("发送邮件，主题：{Subject}，收件人：{ToAddress}", subject, toAddress);

        var message = new MimeMessage {
            Subject = subject,
            From = { new MailboxAddress("StarBlog", _options.FromAddress) },
            To = { new MailboxAddress(toName, toAddress) },
            Body = new BodyBuilder { HtmlBody = htmlBody + Footer }.ToMessageBody()
        };

        using var client = new SmtpClient {
            ServerCertificateValidationCallback = (_, _, _, _) => true
        };
        client.AuthenticationMechanisms.Remove("XOAUTH2");

        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.Auto, cancellationToken);
        await client.AuthenticateAsync(_options.FromUsername, _options.FromPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
