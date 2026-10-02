namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 公开站 Next 路由，供 sitemap、feed 与通知邮件共用。
/// </summary>
public static class PublicContentPaths {
    public static string Post(string id, string? slug) =>
        string.IsNullOrWhiteSpace(slug)
            ? $"/blog/{Uri.EscapeDataString(id)}"
            : $"/blog/{Uri.EscapeDataString(slug)}";

    public static string BlogList(int? categoryId = null) =>
        categoryId is > 0 ? $"/blog?categoryId={categoryId.Value}" : "/blog";

    public static string Photography() => "/photography";

    public static string Photo(string id) => $"/photography/{Uri.EscapeDataString(id)}";

    public static string MediaPhoto(string filePath) =>
        $"/media/photography/{filePath.Replace('\\', '/').TrimStart('/')}";
}
