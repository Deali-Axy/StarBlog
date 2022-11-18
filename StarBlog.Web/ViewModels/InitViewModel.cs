using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels;

public class InitViewModel {
    [Display(Name = "用户名")]
    public string Username { get; set; }

    [Display(Name = "密码")]
    public string Password { get; set; }

    [Display(Name = "博客域名")]
    public string Host { get; set; }

    [Display(Name = "文章默认渲染方式")]
    public string DefaultRender { get; set; }
}