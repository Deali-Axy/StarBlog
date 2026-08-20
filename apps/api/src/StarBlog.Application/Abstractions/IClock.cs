namespace StarBlog.Application.Abstractions;

public interface IClock {
    DateTime UtcNow { get; }
    DateTime Now { get; }
}
