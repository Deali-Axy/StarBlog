namespace StarBlog.Api.Infrastructure.Time;

/// <summary>
/// 可替换的系统时钟，便于测试固定时间。
/// </summary>
public interface IClock {
    DateTime UtcNow { get; }
    DateTime Now { get; }
}
