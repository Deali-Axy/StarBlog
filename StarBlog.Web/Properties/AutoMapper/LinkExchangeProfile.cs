using AutoMapper;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.LinkExchange;

namespace StarBlog.Web.Properties.AutoMapper; 

public class LinkExchangeProfile : Profile {
    public LinkExchangeProfile() {
        CreateMap<LinkExchangeAddViewModel, LinkExchange>();
    }
}