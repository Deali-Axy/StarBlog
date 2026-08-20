using System.Text;
using MailKit;
using Microsoft.Extensions.Options;
using StarBlog.Content.Utils;

namespace StarBlog.Application.Services;

public class EmailService {
    private readonly ILogger<EmailService> _logger;
    private readonly EmailAccountConfig _emailAccountConfig;
    private const string StarblogLink = "<a href=\"https://deali.cn\">StarBlog</a>";


    public EmailService(ILogger<EmailService> logger, IOptions<EmailAccountConfig> options) {
        _logger = logger;
        _emailAccountConfig = options.Value;
    }

    public async Task<MessageSentEventArgs> SendEmailAsync(string subject, string body, string toName, string toAddress) {
        _logger.LogDebug("发送邮件，主题：{Subject}，收件人：{ToAddress}", subject, toAddress);
        body += $"<br><p>本消息由 {StarblogLink} 自动发送，无需回复。</p>";
        return await EmailUtils.SendEmailAsync(_emailAccountConfig, subject, body, toName, toAddress);
    }

    /// <summary>
    /// 发送邮箱验证码
    /// <returns>生成随机验证码</returns>
    /// <param name="mock">只生成验证码，不发邮件</param>
    /// </summary>
    public async Task<string> SendOtpMail(string email, bool mock = false) {
        var otp = Random.Shared.NextInt64(1000, 9999).ToString();

        var sb = new StringBuilder();
        sb.AppendLine($"<p>欢迎访问StarBlog！验证码：{otp}</p>");
        sb.AppendLine($"<p>如果您没有进行任何操作，请忽略此邮件。</p>");

        if (!mock) {
            await SendEmailAsync(
                "[StarBlog]邮箱验证码",
                sb.ToString(),
                email,
                email
            );
        }

        return otp;
    }
}
