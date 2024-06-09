using FluentResults;

namespace FileShare.App.Services.Abstraction;

public interface IEngineService
{
    Task<Result> StartEngineAsync(IProgress<int> progress, CancellationToken token, bool restart = false);
}