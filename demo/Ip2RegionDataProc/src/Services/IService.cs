using FluentResults;

namespace Ip2RegionDataProc.Services;

public interface IService {
    Task<Result> Run();
}