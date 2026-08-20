namespace StarBlog.Application.Contrib.SiteMessage;

public sealed class InMemoryMessageStore : IMessageStore {
    private readonly Queue<Message> _queue = new();

    public Queue<Message> GetQueue() => _queue;
}
