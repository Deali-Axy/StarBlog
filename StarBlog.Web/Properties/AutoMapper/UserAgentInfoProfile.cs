using AutoMapper;
using StarBlog.Data.Models;

namespace StarBlog.Web.Properties.AutoMapper;

public class UserAgentInfoProfile : Profile {
    public UserAgentInfoProfile() {
        CreateMap<UAParser.OS, OS>();
        CreateMap<UAParser.Device, Device>();
        CreateMap<UAParser.UserAgent, UserAgent>();
        CreateMap<UAParser.ClientInfo, UserAgentInfo>();
    }
}