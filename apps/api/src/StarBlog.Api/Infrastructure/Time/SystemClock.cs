namespace StarBlog.Api.Infrastructure.Time;

/// <summary>
/// 使用操作系统时钟的默认实现。
/// </summary>
public sealed class SystemClock : IClock {
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
