using StarBlog.Data.Models;

namespace StarBlog.Application.Services.OutboxServices;

public interface IOutboxHandler {
    string Type { get; }

    Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken);
}
