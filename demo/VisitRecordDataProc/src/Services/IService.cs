using FluentResults;

namespace VisitRecordDataProc.Services;

public interface IService {
    Task<Result> Run();
}