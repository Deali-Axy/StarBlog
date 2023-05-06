using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels.LinkExchange;

public class LinkExchangeAddViewModel {
    /// <summary>
    /// 网站名称
    /// </summary>
    [Display(Name = "网站名称")]
    [Required(ErrorMessage = "必须填写网站名称")]
    public string Name { get; set; }

    /// <summary>
    /// 介绍
    /// </summary>
    [Display(Name = "介绍")]
    public string? Description { get; set; }

    /// <summary>
    /// 网址
    /// </summary>
    [Display(Name = "网址")]
    [Required(ErrorMessage = "必须填写网址")]
    [DataType(DataType.Url)]
    public string Url { get; set; }

    /// <summary>
    /// 站长
    /// </summary>
    [Display(Name = "站长名称")]
    [Required(ErrorMessage = "必须填写站长名称")]
    public string WebMaster { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    [Display(Name = "联系邮箱")]
    [Required(ErrorMessage = "必须填写联系邮箱")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}