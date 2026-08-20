using AutoMapper;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Properties.AutoMapper; 

public class PostProfile : Profile {
    public PostProfile() {
        CreateMap<PostUpdateDto, Post>();
        CreateMap<PostCreationDto, Post>();
    }
}