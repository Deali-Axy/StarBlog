namespace StarBlog.Application.Services.OutboxServices;

/// <summary>
/// Outbox 运行参数
/// <para>先提供默认值，后续可通过 appsettings.json 的 Outbox 节进行覆盖。</para>
/// </summary>
public sealed class OutboxOptions {
    /// <summary>
    /// 每轮最多领取的任务数
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// 队列空闲时的轮询间隔
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// 租约时长（领取任务后锁定多久）
    /// </summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 最大退避时长（指数退避上限）
    /// </summary>
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 退避随机抖动（避免多实例同时重试形成“惊群”）
    /// </summary>
    public TimeSpan BackoffJitter { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// 默认最大重试次数（入队未指定时使用）
    /// </summary>
    public int DefaultMaxAttempts { get; set; } = 5;
}
