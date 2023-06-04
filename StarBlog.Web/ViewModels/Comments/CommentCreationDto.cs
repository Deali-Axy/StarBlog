namespace StarBlog.Web.ViewModels.Comments;

public class CommentCreationDto {
    public string? ParentId { get; set; }
    public string PostId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? Url { get; set; }
    public string EmailOtp { get; set; }
    public string Content { get; set; }
}