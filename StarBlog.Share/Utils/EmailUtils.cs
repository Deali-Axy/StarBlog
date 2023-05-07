using MailKit;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StarBlog.Share.Utils;

public class EmailAccountConfig {
    public string Host { get; set; }
    public int Port { get; set; }
    public string FromUsername { get; set; }
    public string FromPassword { get; set; }
    public string FromAddress { get; set; }
}

public static class EmailUtils {
    public static async Task<MessageSentEventArgs> SendEmailAsync(
        EmailAccountConfig config,
        string subject,
        string htmlBody,
        string toName,
        string toAddress,
        string fromName = "StarBlog"
    ) {
        return await SendEmailAsync(config,
            new MimeMessage {
                Subject = subject,
                From = {new MailboxAddress(fromName, config.FromAddress)},
                To = {new MailboxAddress(toName, toAddress)},
                Body = new BodyBuilder {
                    HtmlBody = htmlBody
                }.ToMessageBody()
            }
        );
    }

    public static async Task<MessageSentEventArgs> SendEmailAsync(EmailAccountConfig config, MimeMessage message,
        HttpProxyClient? proxyClient = null) {
        MessageSentEventArgs result = null;
        using var client = new SmtpClient {
            ServerCertificateValidationCallback = (s, c, h, e) => true,
        };
        if (proxyClient != null) {
            client.ProxyClient = proxyClient;
        }

        client.AuthenticationMechanisms.Remove("XOAUTH2");
        client.MessageSent += (sender, args) => { result = args; };

        await client.ConnectAsync(config.Host, config.Port, SecureSocketOptions.Auto);
        await client.AuthenticateAsync(config.FromUsername, config.FromPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return result;
    }
}