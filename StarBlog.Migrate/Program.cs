// See https://aka.ms/new-console-template for more information

using StarBlog.Contrib.Extensions;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Migrate;

var log = new System.Collections.Specialized.StringCollection();

const string importDir = @"E:\Documents\0_Write\0_blog\";
// const string importDir = @"D:\blog\";
const string assetsDir = @"D:\Code\C#\0_NetCore\Asp.Net.Core\StarBlog\StarBlog.Web\wwwroot\assets\blog";

var exclusionDirs = new List<string> {".git", "logseq", "pages"};

// 删除旧文件
var removeFileList = new List<string> {"app.db", "app.db-shm", "app.db-wal"};
foreach (var filename in removeFileList.Where(filename => File.Exists(filename))) {
    Console.WriteLine($"删除旧文件：{filename}");
    File.Delete(filename);
}

// 数据库
var freeSql = FreeSqlFactory.Create("Data Source=app.db;Synchronous=Off;Journal Mode=WAL;Cache Size=5000;");
var postRepo = freeSql.GetRepository<Post>();
var categoryRepo = freeSql.GetRepository<Category>();

// 数据导入
WalkDirectoryTree(new DirectoryInfo(importDir));

// 覆盖数据库
var destFile = Path.GetFullPath("../../../../StarBlog.Web/app.db");
Console.WriteLine($"覆盖数据库：{destFile}");
File.Copy("app.db", destFile, true);


void WalkDirectoryTree(DirectoryInfo root) {
    // 参考资料：https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

    FileInfo[] files = null;
    DirectoryInfo[] subDirs = null;

    // First, process all the files directly under this folder
    try {
        files = root.GetFiles("*.*");
    }
    // This is thrown if even one of the files requires permissions greater
    // than the application provides.
    catch (UnauthorizedAccessException e) {
        // This code just writes out the message and continues to recurse.
        // You may decide to do something different here. For example, you
        // can try to elevate your privileges and access the file again.
        log.Add(e.Message);
    }

    catch (DirectoryNotFoundException e) {
        Console.WriteLine(e.Message);
    }

    if (files != null) {
        foreach (var fi in files) {
            // In this example, we only access the existing FileInfo object. If we
            // want to open, delete or modify the file, then
            // a try-catch block is required here to handle the case
            // where the file has been deleted since the call to TraverseTree().
            Console.WriteLine(fi.FullName);
            var postPath = fi.DirectoryName!.Replace(importDir, "");

            var categoryNames = postPath.Split("\\");
            var categories = new List<Category>();
            if (categoryNames.Length > 0) {
                var rootCategory = categoryRepo.Where(a => a.Name == categoryNames[0]).First()
                                   ?? categoryRepo.Insert(new Category {Name = categoryNames[0]});
                categories.Add(rootCategory);
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
                }
            }

            var reader = fi.OpenText();
            var content = reader.ReadToEnd();
            reader.Close();

            // 保存文章
            var post = new Post {
                Id = GuidUtils.GuidTo16String(),
                Title = fi.Name.Replace(".md", ""),
                Summary = content.Limit(200),
                Content = content,
                Path = postPath,
                CreationTime = fi.CreationTime,
                LastUpdateTime = fi.LastWriteTime,
                CategoryId = categories[^1].Id,
                Categories = string.Join(",", categories.Select(a => a.Id))
            };
            postRepo.Insert(post);
        }
    }

    // Now find all the subdirectories under this directory.
    subDirs = root.GetDirectories();

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