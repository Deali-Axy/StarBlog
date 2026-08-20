using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels.Search;

public class SearchPost {
    public Post Post { get; set; }
    public int TitleScore { get; set; }

    public int ContentScore { get; set; }

    // 标题每命中一次+100分
    // 内容命中+1分
    public int Score => TitleScore * 100 + ContentScore;
    public string HighlightedTitle { get; set; }
    public string HighlightedSnippet { get; set; }
}