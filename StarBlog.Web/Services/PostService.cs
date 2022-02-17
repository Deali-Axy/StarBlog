using FreeSql;
using Markdig;
using Markdown.ColorCode;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Services;

public class PostService {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;

    public PostService(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public PostViewModel GetPostViewModel(Post post) {
        var mdPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseGenericAttributes()
            .UseDefinitionLists()
            .UseAutoIdentifiers()
            .UseAutoLinks()
            .UseTaskLists()
            .UseBootstrap()
            // .UseColorCode()
            .Build();

        var vm = new PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            ContentHtml = Markdig.Markdown.ToHtml(post.Content, mdPipeline),
            Path = post.Path,
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category,
            Categories = new List<Category>()
        };

        foreach (var itemId in post.Categories.Split(",").Select(int.Parse)) {
            var item = _categoryRepo.Where(a => a.Id == itemId).First();
            if (item != null) vm.Categories.Add(item);
        }

        return vm;
    }
}