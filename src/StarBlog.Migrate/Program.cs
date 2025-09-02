// See https://aka.ms/new-console-template for more information

using StarBlog.Share.Extensions;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Share.Utils;
using StarBlog.Share;

const string importDir = @"E:\Documents\0_Write\0_blog\";
// const string importDir = @"D:\blog\";

var assetsPath = Path.GetFullPath("../../../../StarBlog.Web/wwwroot/media/blog");

var exclusionDirs = new List<string> {".git", "logseq", "pages"};

// 删除旧文件
var removeFileList = new List<string> {"app.db", "app.db-shm", "app.db-wal"};
foreach (var filename in removeFileList.Where(File.Exists)) {
    Console.WriteLine($"删除旧文件：{filename}");
    File.Delete(filename);
}

// 数据库
var freeSql = FreeSqlFactory.Create("Data Source=app.db;Synchronous=Off;Cache Size=5000;");
var postRepo = freeSql.GetRepository<Post>();
var categoryRepo = freeSql.GetRepository<Category>();

// 数据导入
WalkDirectoryTree(new DirectoryInfo(importDir));

// 复制数据库
var destFile = Path.GetFullPath("../../../../StarBlog.Web/app.db");
if (File.Exists("app.db")) {
    Console.WriteLine($"复制数据库：{destFile}");
    File.Copy("app.db", destFile, true);
}

void WalkDirectoryTree(DirectoryInfo root) {
    // 参考资料：https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

    Console.WriteLine($"正在扫描文件夹：{root.FullName}");

    FileInfo[]? files = null;
    DirectoryInfo[]? subDirs = null;

    try {
        files = root.GetFiles("*.md");
    }
    catch (UnauthorizedAccessException e) {
        Console.WriteLine(e.Message);
    }
    catch (DirectoryNotFoundException e) {
        Console.WriteLine(e.Message);
    }

    if (files != null) {
        foreach (var fi in files) {
            Console.WriteLine(fi.FullName);
            var postPath = fi.DirectoryName!.Replace(importDir, "");

            var categoryNames = postPath.Split("\\");
            Console.WriteLine($"categoryNames: {string.Join(",", categoryNames)}");
            var categories = new List<Category>();
            if (categoryNames.Length > 0) {
                var rootCategory = categoryRepo.Where(a => a.Name == categoryNames[0]).First()
                                   ?? categoryRepo.Insert(new Category {Name = categoryNames[0]});
                categories.Add(rootCategory);
                Console.WriteLine($"+ 添加分类: {rootCategory.Id}.{rootCategory.Name}");
                for (var i = 1; i < categoryNames.Length; i++) {
                    var name = categoryNames[i];
                    var parent = categories[i - 1];
                    var category = categoryRepo.Where(
                                       a => a.ParentId == parent.Id && a.Name == name).First()
                                   ?? categoryRepo.Insert(new Category {
                                       Name = name,
                                       ParentId = parent.Id
                                   });
                    categories.Add(category);
                    Console.WriteLine($"+ 添加子分类：{category.Id}.{category.Name}");
                }
            }

            var reader = fi.OpenText();
            var content = reader.ReadToEnd();
            reader.Close();

            // 保存文章
            var post = new Post {
                Id = GuidUtils.GuidTo16String(),
                Status = "已发布",
                Title = fi.Name.Replace(".md", ""),
                IsPublish = true,
                Content = content,
                Path = postPath,
                CreationTime = fi.CreationTime,
                LastUpdateTime = fi.LastWriteTime,
                CategoryId = categories[^1].Id,
                Categories = string.Join(",", categories.Select(a => a.Id))
            };


            var processor = new PostProcessor(importDir, assetsPath, post);

            // 处理文章标题和状态
            processor.InflateStatusTitle();

            // 处理文章正文内容
            // 导入文章的时候一并导入文章里的图片，并对图片相对路径做替换操作
            post.Content = processor.MarkdownParse();
            post.Summary = processor.GetSummary(200);

            postRepo.Insert(post);
        }
    }

    // Now find all the subdirectories under this directory.
    subDirs = root.GetDirectories();

    if (subDirs != null) {
        foreach (var dirInfo in subDirs) {
            if (exclusionDirs.Contains(dirInfo.Name)) {
                continue;
            }

            if (dirInfo.Name.EndsWith(".assets")) {
                continue;
            }

            // Resursive call for each subdirectory.
            WalkDirectoryTree(dirInfo);
        }
    }
}