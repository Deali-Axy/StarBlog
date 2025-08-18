using AutoMapper;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Links;

namespace StarBlog.Web.Properties.AutoMapper; 

public class LinkProfile : Profile{
    public LinkProfile() {
        CreateMap<LinkCreationDto, Link>();
    }
}