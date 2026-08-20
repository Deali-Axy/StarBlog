using StarBlog.Application.Abstractions;

namespace StarBlog.Api.Adapters;

public sealed class SystemClock : IClock {
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
