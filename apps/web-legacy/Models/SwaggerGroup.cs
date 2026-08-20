using Microsoft.OpenApi;

namespace StarBlog.Web.Models; 

/// <summary>
/// 接口组
/// <para>分组设计参考：https://wangyou233.wang/archives/73</para>
/// </summary>
public class SwaggerGroup {
    /// <summary>
    /// 组名称（同时用于做URL前缀）
    /// </summary>
    public string Name { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public SwaggerGroup(string name, string? title = null, string? description = null) {
        Name = name;
        Title = title;
        Description = description;
    }

    /// <summary>
    /// 生成 <see cref="Microsoft.OpenApi.OpenApiInfo"/>
    /// </summary>
    public OpenApiInfo ToOpenApiInfo(string version = "1.0") {
        var item = new OpenApiInfo();
        Title ??= Name;
        Description ??= Name;
        return new OpenApiInfo { Title = Title, Description = Description, Version = version };
    }
}
