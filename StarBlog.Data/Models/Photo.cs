using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public class Photo {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// 作品标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 拍摄地点
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// 文件存储位置
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 高度
    /// </summary>
    public long Height { get; set; }

    /// <summary>
    /// 宽度
    /// </summary>
    public long Width { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}