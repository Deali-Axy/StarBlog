using AutoMapper;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Properties.AutoMapper; 

public class PhotoProfile : Profile {
    public PhotoProfile() {
        CreateMap<PhotoUpdateDto, Photo>();
    }
}