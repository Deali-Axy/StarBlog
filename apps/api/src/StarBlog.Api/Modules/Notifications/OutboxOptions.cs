namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// Outbox Worker 运行参数。
/// </summary>
public sealed class OutboxOptions {
    public const string SectionName = "Outbox";
    public int BatchSize { get; set; } = 20;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan BackoffJitter { get; set; } = TimeSpan.FromMilliseconds(500);
    public int DefaultMaxAttempts { get; set; } = 5;
}
