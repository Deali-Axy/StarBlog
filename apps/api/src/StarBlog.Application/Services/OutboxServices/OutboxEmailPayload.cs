namespace StarBlog.Application.Services.OutboxServices;

public sealed record OutboxEmailPayload {
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public required string ToName { get; init; }
    public required string ToAddress { get; init; }
}
