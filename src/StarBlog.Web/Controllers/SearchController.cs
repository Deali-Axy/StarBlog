using System.Text.RegularExpressions;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Search;

namespace StarBlog.Web.Controllers;

public class SearchController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;

    public SearchController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public IActionResult Blog(string keyword, int categoryId = 0, int page = 1, int pageSize = 5) {
        var searchPosts = _postRepo
            .Where(a => a.IsPublish)
            .Where(a =>
                a.Title!.Contains(keyword) ||
                a.Content.Contains(keyword)
            )
            .Include(a => a.Category)
            .ToList()
            .Select(p => {
                var item = new SearchPost {
                    Post = p,
                    TitleScore = p.Title.Split(keyword).Length - 1,
                    ContentScore = p.Content.Split(keyword).Length - 1,
                };

                // 标题每命中一次+100分
                // 内容命中+1分
                item.Score = item.TitleScore * 100 + item.ContentScore * 1;

                return item;
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        // 实现搜索结果高亮
        var regex = new Regex(Regex.Escape(keyword), RegexOptions.IgnoreCase);
        foreach (var item in searchPosts) {
            item.HighlightedTitle = regex.Replace(item.Post.Title, m => $"<mark>{m.Value}</mark>");
            item.HighlightedSnippet = GetHighlightedSnippet(item.Post.Content, keyword);
        }

        return View("Result", new SearchResultViewModel {
            Keyword = keyword,
            SearchPosts = searchPosts
        });
    }

    public static string GetHighlightedSnippet(string content, string keyword, int snippetLength = 100) {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keyword))
            return string.Empty;

        var regex = new Regex(Regex.Escape(keyword), RegexOptions.IgnoreCase);
        var match = regex.Match(content);

        if (!match.Success) {
            // 没匹配到，直接取前 snippetLength*2 个字符作为摘要
            return content.Length > snippetLength * 2
                ? content.Substring(0, snippetLength * 2) + "..."
                : content;
        }

        // 计算截取范围（匹配位置前后各 snippetLength）
        int start = Math.Max(0, match.Index - snippetLength);
        int length = Math.Min(content.Length - start, match.Length + snippetLength * 2);

        string snippet = content.Substring(start, length);

        // 高亮处理
        snippet = regex.Replace(snippet, m => $"<mark>{m.Value}</mark>");

        // 前后补省略号（如果不是全文开头或结尾）
        if (start > 0) snippet = "..." + snippet;
        if (start + length < content.Length) snippet += "...";

        return snippet;
    }
}