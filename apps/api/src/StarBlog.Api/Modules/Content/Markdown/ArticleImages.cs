using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Api.Modules.Content.Domain;

namespace StarBlog.Api.Modules.Content.Markdown;

public sealed record ArticleImage(string Url, string Alt);

/// <summary>Extract preview candidates without reading files or fetching remote resources.</summary>
public static class ArticleImages {
    public static string? ResolveUrl(string? value, string postId) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var url = value.Trim().Replace('\\', '/');
        if (url.StartsWith("//") || url.StartsWith('#') || url.Any(char.IsControl)) return null;
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && !url.StartsWith('/')) {
            if (absolute.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(absolute.UserInfo)) return null;
            // Legacy posts store absolute URLs for their own media; keep preview traffic local.
            if (absolute.AbsolutePath.StartsWith($"/media/blog/{Uri.EscapeDataString(postId)}/", StringComparison.Ordinal))
                url = absolute.PathAndQuery;
            else return absolute.AbsoluteUri;
        }
        var path = Uri.UnescapeDataString(url.Split('?', '#')[0]).Replace('\\', '/');
        if (path.Split('/').Any(part => part == "..")) return null;
        if (url.Contains(':')) return null;
        while (url.StartsWith("./")) url = url[2..];
        return url.StartsWith('/') ? url : $"/media/blog/{Uri.EscapeDataString(postId)}/{url}";
    }

    public static IReadOnlyList<ArticleImage> Candidates(Post post, string? body = null) {
        if (post.CoverHidden) return [];
        if (!string.IsNullOrWhiteSpace(post.CoverUrl)) {
            var manual = ResolveUrl(post.CoverUrl, post.Id);
            return manual == null ? [] : new[] { new ArticleImage(manual, post.Title) }
                .Concat(BodyCandidates(post, body)).DistinctBy(image => image.Url).Take(8).ToArray();
        }
        return BodyCandidates(post, body);
    }

    public static IReadOnlyList<ArticleImage> BodyCandidates(Post post, string? body = null) {
        var images = Extract(body ?? post.Content, post.Id, post.Title);
        return images.Count == 0 && body != null && body != post.Content ? Extract(post.Content, post.Id, post.Title) : images;
    }

    private static IReadOnlyList<ArticleImage> Extract(string? content, string postId, string title) {
        if (string.IsNullOrWhiteSpace(content)) return [];
        var document = Markdig.Markdown.Parse(content);
        var result = new List<ArticleImage>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var image in document.Descendants<LinkInline>().Where(item => item.IsImage)) {
            var url = ResolveUrl(image.Url, postId);
            if (url == null || !seen.Add(url)) continue;
            var alt = string.Concat(image.Descendants<LiteralInline>().Select(item => item.Content.ToString()));
            var hint = $"{url} {alt}".ToLowerInvariant();
            if (new[] { "shields.io", "badge", "qrcode", "qr-code", "二维码", "favicon", "emoji", "avatar", "logo.svg", ".ico" }.Any(hint.Contains)) continue;
            result.Add(new(url, string.IsNullOrWhiteSpace(alt) ? title : alt));
            if (result.Count == 8) break;
        }
        return result;
    }
}
