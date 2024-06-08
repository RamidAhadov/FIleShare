using FileShare.Business.Constants;
using FluentResults;

namespace FileShare.App.Services.Abstraction;

public interface IFileService
{
    Task<Result> SendRequestAsync(string destinationIp, CancellationToken token);
    Task<Result> SendFilenameAsync(string destinationIp, string filename, CancellationToken token);
    IAsyncEnumerable<string> GetRequestAsync(CancellationToken token);
    Task<Result> SendResponseAsync(Responses response, string destinationIp, CancellationToken token);
}