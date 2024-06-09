using System.Net.Sockets;
using FileShare.Business.Constants;
using FileShare.Business.Models;
using FluentResults;

namespace FileShare.Business.Abstraction;

public interface INotifyManager
{
    Task<Result> CreateListenerAsync(CancellationToken token, bool restart);
    Task<Result> CreateRequestReceiverAsync(CancellationToken token, bool restart);
    Task<Result> CreateResponseReceiverAsync(CancellationToken token, bool restart);
    Task<Result> SendRequestAsync(string destinationIp, CancellationToken token, int port = 8990);
    Task<Result> SendResponseAsync(Responses response, string destinationIp, CancellationToken token, int port = 8991);
    IAsyncEnumerable<MessageModel?> GetReceivedFilenameAsync(CancellationToken token);
    IAsyncEnumerable<string?> GetRequestAsync(CancellationToken token);
    Task<Result> SendFilenameAsync(string destinationIp, string filename, CancellationToken token, int port = 8989);
}