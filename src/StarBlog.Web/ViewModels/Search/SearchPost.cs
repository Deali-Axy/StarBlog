using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels.Search;

public class SearchPost {
    public Post Post { get; set; }
    public int TitleScore { get; set; }
    public int ContentScore { get; set; }
    public int Score { get; set; }
    public string HighlightedTitle { get; set; }
    public string HighlightedSnippet { get; set; }
}