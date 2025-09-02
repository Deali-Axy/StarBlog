using AutoMapper;
using StarBlog.Data.Models;

namespace DataProc.Properties;

public class UserAgentInfoProfile : Profile {
    public UserAgentInfoProfile() {
        CreateMap<UAParser.OS, OS>();
        CreateMap<UAParser.Device, Device>();
        CreateMap<UAParser.UserAgent, UserAgent>();
        CreateMap<UAParser.ClientInfo, UserAgentInfo>();
    }
}