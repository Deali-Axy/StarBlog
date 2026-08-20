using AutoMapper;
using StarBlog.Application.ViewModels.Blog;
using StarBlog.Application.ViewModels.Categories;
using StarBlog.Application.ViewModels.Links;
using StarBlog.Application.ViewModels.Photography;
using StarBlog.Data.Models;

namespace StarBlog.Api.Mappings;

/// <summary>
/// API 层使用的 DTO 与实体映射。
///
/// 原 Web 项目中的映射配置位于 Razor 项目，因此纯 API 启动时不会被
/// <c>AddAutoMapper(typeof(Program))</c> 扫描到。将它们集中在 API 项目，
/// 可以确保文章、分类、图片和友链的写操作在迁移后仍能正常工作。
/// </summary>
public sealed class ApiMappingProfile : Profile {
    public ApiMappingProfile() {
        // 写入文章时只复制 DTO 明确提供的字段；主键和时间戳由控制器/服务维护。
        CreateMap<PostCreationDto, Post>();
        CreateMap<PostUpdateDto, Post>();

        // 分类、图片和友链的后台写模型。
        CreateMap<CategoryCreationDto, Category>();
        CreateMap<PhotoUpdateDto, Photo>();
        CreateMap<LinkCreationDto, Link>();

        // 访问记录中解析 User-Agent 时需要的映射。
        CreateMap<UAParser.OS, OS>();
        CreateMap<UAParser.Device, Device>();
        CreateMap<UAParser.UserAgent, UserAgent>();
        CreateMap<UAParser.ClientInfo, UserAgentInfo>();
    }
}
