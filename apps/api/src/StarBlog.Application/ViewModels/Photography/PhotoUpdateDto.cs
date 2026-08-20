using System.ComponentModel.DataAnnotations;

namespace StarBlog.Application.ViewModels.Photography;

public class PhotoUpdateDto {
    public string? Id { get; set; }
    
    /// <summary>
    /// 作品标题
    /// </summary>
    [Required(ErrorMessage = "作品标题不能为空")]
    public string Title { get; set; }

    /// <summary>
    /// 拍摄地点
    /// </summary>
    [Required(ErrorMessage = "拍摄地点不能为空")]
    public string Location { get; set; }

    public DateTime CreateTime { get; set; }
}