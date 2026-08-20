namespace StarBlog.Application.Contrib.SiteMessage; 

public class MessageService {
    private readonly IMessageStore _store;
    private const string DefaultTitle = "提示信息";

    public MessageService(IMessageStore store) {
        _store = store;
    }

    public Queue<Message> CurrentQueue {
        get {
            return _store.GetQueue();
        }
    }

    public bool IsEmpty => CurrentQueue.Count == 0;

    public Message Dequeue() => CurrentQueue.Dequeue();

    public Message Enqueue(string tag, string title, string content) {
        var message = new Message {
            Tag = tag,
            Title = title,
            Content = content
        };
        CurrentQueue.Enqueue(message);

        return message;
    }

    public Message Debug(string content, string title = DefaultTitle) {
        return Enqueue(MessageTags.Debug, title, content);
    }

    public Message Success(string content, string title = DefaultTitle) {
        return Enqueue(MessageTags.Success, title, content);
    }

    public Message Info(string content, string title = DefaultTitle) {
        return Enqueue(MessageTags.Info, title, content);
    }

    public Message Warning(string content, string title = DefaultTitle) {
        return Enqueue(MessageTags.Warning, title, content);
    }

    public Message Error(string content, string title = DefaultTitle) {
        return Enqueue(MessageTags.Error, title, content);
    }
}
