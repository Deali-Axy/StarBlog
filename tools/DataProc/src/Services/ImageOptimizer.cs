using FluentResults;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

public class ImageOptimizer(
    ILogger<ImageOptimizer> logger,
    IConfiguration conf,
    IBaseRepository<Post> postRepo
) : IService {
    public async Task<Result> Run() {
        var posts = await postRepo.Select.ToListAsync();
        var wwwroot = conf.GetValue<string>("StarBlog:wwwroot");
        if (string.IsNullOrWhiteSpace(wwwroot)) {
            throw new Exception("wwwroot 配置错误");
        }

        foreach (var post in posts) {
            var blogImageDir = Path.Combine(wwwroot, "media", "blog", post.Id);
            if (!Directory.Exists(blogImageDir)) {
                continue;
            }
            
            logger.LogInformation("处理文章 {PostId} 的图片, {blogImageDir}", post.Id, blogImageDir);

            var files = Directory.GetFiles(blogImageDir);
            foreach (var file in files) {
                if (!IsImage(file)) {
                    continue;
                }
                logger.LogInformation("处理图片 {FileName}", file);
                
            }
        }

        return Result.Ok();
    }

    bool IsImage(string fileName) {
        var ext = Path.GetExtension(fileName);
        return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".webp", StringComparison.OrdinalIgnoreCase);
    }
}