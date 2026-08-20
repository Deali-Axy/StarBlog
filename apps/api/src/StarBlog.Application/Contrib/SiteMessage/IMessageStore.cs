namespace StarBlog.Application.Contrib.SiteMessage;

public interface IMessageStore {
    Queue<Message> GetQueue();
}
