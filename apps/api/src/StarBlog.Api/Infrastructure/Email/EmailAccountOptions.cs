namespace StarBlog.Api.Infrastructure.Email;

/// <summary>
/// SMTP 账号配置，对应 appsettings-email.json 中的 EmailAccountConfig 节。
/// </summary>
public sealed class EmailAccountOptions {
    public const string SectionName = "EmailAccountConfig";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 465;
    public string FromUsername { get; set; } = string.Empty;
    public string FromPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}
