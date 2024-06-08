using System.Runtime.CompilerServices;
using FileShare.App.Services.Abstraction;
using FileShare.Business.Abstraction;
using FileShare.Business.Constants;
using FluentResults;

namespace FileShare.App.Services.Concrete;

public class FileService : IFileService
{
    private INotifyManager _notifyManager;

    public FileService(INotifyManager notifyManager)
    {
        _notifyManager = notifyManager;
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
}