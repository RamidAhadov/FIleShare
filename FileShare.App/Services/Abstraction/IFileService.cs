using FileShare.Business.Constants;
using FileShare.Business.Models;
using FluentResults;

namespace FileShare.App.Services.Abstraction;

public interface IFileService
{
    Task<Result> SendRequestAsync(string destinationIp, CancellationToken token);
    Task<Result> SendFilenameAsync(string destinationIp, string filename, CancellationToken token);
    IAsyncEnumerable<string> GetRequestAsync(CancellationToken token);
    Task<Result> SendResponseAsync(Responses response, string destinationIp, CancellationToken token);
    Task<Result<string>> UploadFileAsync(string filePath, string receiverIp, CancellationToken token);
    Task<Result> DownloadFileAsync(string filename, string? localTargetPath, CancellationToken token);
    IAsyncEnumerable<MessageModel?> GetReceivedFilenameAsync(CancellationToken token);
}