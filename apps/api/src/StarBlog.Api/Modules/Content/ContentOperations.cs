using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 文章、分类、精选与置顶。
/// </summary>
public sealed class ContentOperations : IPublishedContentQueries {
    private readonly StarBlogDbContext _db;
    private readonly IClock _clock;
    private readonly IFileStorage _storage;
    private readonly ISiteConfiguration _configuration;

    public ContentOperations(StarBlogDbContext db, IClock clock, IFileStorage storage, ISiteConfiguration configuration) {
        _db = db;
        _clock = clock;
        _storage = storage;
        _configuration = configuration;
    }

    /// <summary>确保存在默认分类，供首次发文使用。</summary>
    public async Task EnsureDefaultCategoryAsync(CancellationToken cancellationToken) {
        if (await _db.Categories.AnyAsync(cancellationToken)) return;
        _db.Categories.Add(new Category { Name = "未分类", ParentId = 0, Visible = true });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResult<PostResponse>> GetPostsAsync(int page, int pageSize, bool adminMode, string? search, int? categoryId, bool? isPublish, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.Posts.AsNoTracking().Include(post => post.Category).AsQueryable();
        if (!adminMode) query = query.Where(post => post.IsPublish);
        else if (isPublish != null) query = query.Where(post => post.IsPublish == isPublish);
        if (categoryId is > 0) query = query.Where(post => post.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(post => post.Title.Contains(search));
        query = query.OrderByDescending(post => post.LastUpdateTime);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<PostResponse> {
            Items = items.Select(post => PostResponse.From(post)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PostResponse?> GetPostAsync(string id, CancellationToken cancellationToken) {
        var post = await _db.Posts.AsNoTracking().Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return post == null ? null : PostResponse.From(post, includeToc: true);
    }

    /// <summary>创建文章。Slug 冲突时返回错误。</summary>
    public async Task<(PostResponse? Post, string? Error)> CreatePostAsync(PostUpsertRequest request, CancellationToken cancellationToken) {
        if (!string.IsNullOrWhiteSpace(request.Slug) &&
            await _db.Posts.AnyAsync(post => post.Slug == request.Slug, cancellationToken)) {
            return (null, "slug 已被占用");
        }

        var now = _clock.Now;
        var post = new Post {
            Id = Guid.NewGuid().ToString("N")[..16],
            Title = request.Title,
            Slug = string.IsNullOrWhiteSpace(request.Slug) ? null : request.Slug,
            Status = request.Status,
            IsPublish = request.IsPublish,
            Summary = request.Summary,
            Content = request.Content,
            CategoryId = request.CategoryId == 0 ? 1 : request.CategoryId,
            CreationTime = now,
            LastUpdateTime = now
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetPostAsync(post.Id, cancellationToken), null);
    }

    public async Task<(PostResponse? Post, string? Error)> UpdatePostAsync(string id, PostUpsertRequest request, CancellationToken cancellationToken) {
        var post = await _db.Posts.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (post == null) return (null, null);
        if (!string.IsNullOrWhiteSpace(request.Slug) &&
            await _db.Posts.AnyAsync(item => item.Slug == request.Slug && item.Id != id, cancellationToken)) {
            return (null, "slug 已被占用");
        }

        post.Title = request.Title;
        post.Slug = string.IsNullOrWhiteSpace(request.Slug) ? null : request.Slug;
        post.Status = request.Status;
        post.IsPublish = request.IsPublish;
        post.Summary = request.Summary;
        post.Content = request.Content;
        post.CategoryId = request.CategoryId == 0 ? post.CategoryId : request.CategoryId;
        post.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetPostAsync(post.Id, cancellationToken), null);
    }

    public async Task<bool> DeletePostAsync(string id, CancellationToken cancellationToken) {
        var post = await _db.Posts.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (post == null) return false;
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>将文章图片保存到 media/blog/{postId}/ 并返回绝对 URL。</summary>
    public async Task<string?> UploadImageAsync(string postId, Stream stream, string originalFileName, CancellationToken cancellationToken) {
        if (!await _db.Posts.AnyAsync(post => post.Id == postId, cancellationToken)) return null;
        await _storage.EnsureDirectoryAsync(Path.Combine("media", "blog", postId), cancellationToken);
        var filename = Guid.NewGuid().ToString("N")[..16] + Path.GetExtension(originalFileName);
        var relative = Path.Combine("media", "blog", postId, filename);
        await _storage.SaveAsync(relative, stream, cancellationToken: cancellationToken);
        var host = (await _configuration.GetAsync("host", cancellationToken))?.TrimEnd('/') ?? string.Empty;
        return $"{host}/{relative.Replace('\\', '/')}";
    }

    /// <summary>将文章加入精选；已精选时返回原记录。</summary>
    public async Task<FeaturedPlacementResponse?> AddFeaturedPostAsync(string postId, CancellationToken cancellationToken) {
        if (!await _db.Posts.AnyAsync(post => post.Id == postId, cancellationToken)) return null;
        var existing = await _db.FeaturedPosts.FirstOrDefaultAsync(item => item.PostId == postId, cancellationToken);
        if (existing != null) return new FeaturedPlacementResponse { Id = existing.Id, PostId = existing.PostId };
        var featured = new FeaturedPost { PostId = postId };
        _db.FeaturedPosts.Add(featured);
        await _db.SaveChangesAsync(cancellationToken);
        return new FeaturedPlacementResponse { Id = featured.Id, PostId = featured.PostId };
    }

    /// <summary>将文章设为唯一置顶。</summary>
    public async Task<FeaturedPlacementResponse?> SetTopPostAsync(string postId, CancellationToken cancellationToken) {
        if (!await _db.Posts.AnyAsync(post => post.Id == postId, cancellationToken)) return null;
        var current = await _db.TopPosts.ToListAsync(cancellationToken);
        _db.TopPosts.RemoveRange(current);
        var top = new TopPost { PostId = postId };
        _db.TopPosts.Add(top);
        await _db.SaveChangesAsync(cancellationToken);
        return new FeaturedPlacementResponse { Id = top.Id, PostId = top.PostId };
    }

    public async Task<PageResult<CategoryResponse>> GetCategoriesAsync(int page, int pageSize, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.Categories.AsNoTracking().OrderBy(category => category.ParentId).ThenBy(category => category.Id);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<CategoryResponse> {
            Items = items.Select(ToCategory).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<CategoryNodeResponse>?> GetCategoryTreeAsync(CancellationToken cancellationToken) {
        var categories = await _db.Categories.AsNoTracking().Include(category => category.Posts).ToListAsync(cancellationToken);
        return BuildTree(categories, 0);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CategoryUpsertRequest request, CancellationToken cancellationToken) {
        var category = new Category { Name = request.Name, ParentId = request.ParentId, Visible = request.Visible };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        return ToCategory(category);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, CategoryUpsertRequest request, CancellationToken cancellationToken) {
        var category = await _db.Categories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category == null) return null;
        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Visible = request.Visible;
        await _db.SaveChangesAsync(cancellationToken);
        return ToCategory(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken) {
        var category = await _db.Categories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category == null) return false;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FeaturedCategoryResponse?> AddFeaturedCategoryAsync(int categoryId, FeaturedCategoryCreateRequest request, CancellationToken cancellationToken) {
        if (!await _db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken)) return null;
        var existing = await _db.FeaturedCategories.FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);
        if (existing != null) return ToFeatured(existing);
        var featured = new FeaturedCategory {
            CategoryId = categoryId,
            Name = request.Name,
            Description = request.Description,
            IconCssClass = request.IconCssClass
        };
        _db.FeaturedCategories.Add(featured);
        await _db.SaveChangesAsync(cancellationToken);
        return ToFeatured(featured);
    }

    public async Task<PublishedPostSummary?> GetByIdAsync(string id, CancellationToken cancellationToken = default) {
        var post = await _db.Posts.AsNoTracking().Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return post == null ? null : ToSummary(post);
    }

    public async Task<IReadOnlyList<PublishedPostSummary>> GetPublishedAsync(CancellationToken cancellationToken = default) {
        var posts = await _db.Posts.AsNoTracking().Include(item => item.Category)
            .Where(item => item.IsPublish)
            .OrderByDescending(item => item.LastUpdateTime)
            .ToListAsync(cancellationToken);
        return posts.Select(ToSummary).ToList();
    }

    public async Task<PublishedPostSummary?> GetTopAsync(CancellationToken cancellationToken = default) {
        var top = await _db.TopPosts.AsNoTracking().Include(item => item.Post).ThenInclude(post => post.Category)
            .FirstOrDefaultAsync(cancellationToken);
        return top?.Post == null ? null : ToSummary(top.Post);
    }

    public async Task<IReadOnlyList<PublishedPostSummary>> GetFeaturedPostsAsync(CancellationToken cancellationToken = default) {
        var posts = await _db.FeaturedPosts.AsNoTracking()
            .Include(item => item.Post).ThenInclude(post => post.Category)
            .Select(item => item.Post)
            .ToListAsync(cancellationToken);
        return posts.Select(ToSummary).ToList();
    }

    public async Task<IReadOnlyList<FeaturedCategoryResponse>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default) {
        var items = await _db.FeaturedCategories.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(ToFeatured).ToList();
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetVisibleCategoriesAsync(CancellationToken cancellationToken = default) {
        var items = await _db.Categories.AsNoTracking().Where(category => category.Visible).ToListAsync(cancellationToken);
        return items.Select(ToCategory).ToList();
    }

    /// <summary>返回本模块拥有的数量快照，不读取 Media 实体。</summary>
    public async Task<ContentInventory> GetInventoryAsync(CancellationToken cancellationToken = default) => new() {
        PostsCount = await _db.Posts.CountAsync(cancellationToken),
        CategoriesCount = await _db.Categories.CountAsync(cancellationToken),
        FeaturedPostsCount = await _db.FeaturedPosts.CountAsync(cancellationToken),
        FeaturedCategoriesCount = await _db.FeaturedCategories.CountAsync(cancellationToken)
    };

    private static List<CategoryNodeResponse>? BuildTree(List<Category> categories, int parentId) {
        var children = categories.Where(category => category.ParentId == parentId && category.Visible).ToList();
        if (children.Count == 0) return null;
        return children.Select(category => {
            var nodes = BuildTree(categories, category.Id);
            var total = category.Posts.Count + (nodes?.Sum(node => int.Parse(node.Tags[0])) ?? 0);
            return new CategoryNodeResponse {
                Id = category.Id,
                Text = category.Name,
                Href = $"/blog?categoryId={category.Id}",
                Tags = [total.ToString()],
                Nodes = nodes
            };
        }).ToList();
    }

    private static CategoryResponse ToCategory(Category category) => new() {
        Id = category.Id, Name = category.Name, ParentId = category.ParentId, Visible = category.Visible
    };

    private static FeaturedCategoryResponse ToFeatured(FeaturedCategory item) => new() {
        Id = item.Id, CategoryId = item.CategoryId, Name = item.Name, Description = item.Description, IconCssClass = item.IconCssClass
    };

    private static PublishedPostSummary ToSummary(Post post) => new() {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        Content = post.Content,
        CreationTime = post.CreationTime,
        LastUpdateTime = post.LastUpdateTime,
        CategoryId = post.CategoryId,
        CategoryName = post.Category?.Name,
        IsPublish = post.IsPublish
    };
}
