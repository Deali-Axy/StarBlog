namespace StarBlog.Application.ViewModels.Categories; 

public class FeaturedCategoryCreationDto2 {
    /// <summary>
    /// 重新定义的推荐名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 推荐分类解释
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 图标
    /// <list type="number">
    ///     <listheader>例子</listheader>
    ///     <item>fa-solid fa-c</item>
    ///     <item>fa-brands fa-python</item>
    ///     <item>fa-brands fa-android</item>
    /// </list>
    /// </summary>
    public string IconCssClass { get; set; }
    
    /// <summary>
    /// 分类ID
    /// </summary>
    public int CategoryId { get; set; }
}