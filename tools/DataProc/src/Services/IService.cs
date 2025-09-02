using FluentResults;

namespace DataProc.Services;

public interface IService {
    Task<Result> Run();
}