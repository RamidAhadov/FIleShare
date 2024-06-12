using System.Runtime.CompilerServices;
using FileShare.App.Services.Abstraction;
using FileShare.Business.Abstraction;
using FileShare.Business.Constants;
using FileShare.Business.Models;
using FluentResults;

namespace FileShare.App.Services.Concrete;

public class FileService : IFileService
{
    private readonly INotifyManager _notifyManager;
    private readonly IConnectionManager _connectionManager;

    public FileService(INotifyManager notifyManager, IConnectionManager connectionManager)
    {
        _notifyManager = notifyManager;
        _connectionManager = connectionManager;
    }

    public async Task<Result> SendRequestAsync(string destinationIp, CancellationToken token)
    {
        return await _notifyManager.SendRequestAsync(destinationIp, token);
    }

    public async Task<Result> SendFilenameAsync(string destinationIp, string filename, CancellationToken token)
    {
        return await _notifyManager.SendFilenameAsync(destinationIp, filename, token);
    }

    public async IAsyncEnumerable<string> GetRequestAsync([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var request in _notifyManager.GetRequestAsync(token))
        {
            yield return request;
        }
    }

    public async Task<Result> SendResponseAsync(Responses response, string destinationIp, CancellationToken token)
    {
        return await _notifyManager.SendResponseAsync(response, destinationIp, token);
    }

    public async Task<Result<string>> UploadFileAsync(string filePath, string receiverIp, CancellationToken token)
    {
        return await _connectionManager.UploadFileAsync(filePath, receiverIp, token);
    }

    public async Task<Result> DownloadFileAsync(string filename, string? localTargetPath, CancellationToken token)
    {
        return await _connectionManager.DownloadFileAsync(filename, token, localTargetPath);
    }

    public async IAsyncEnumerable<MessageModel?> GetReceivedFilenameAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var filename in _notifyManager.GetReceivedFilenameAsync(token))
        {
            yield return filename;
        }
    }
}