namespace StarBlog.Application.ViewModels.Links; 

public class LinkCreationDto {
    /// <summary>
    /// 网站名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 介绍
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 网址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Visible { get; set; }
}