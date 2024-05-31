using System.Net.Sockets;
using FileShare.Business.Constants;
using FileShare.Business.Models;
using FluentResults;

namespace FileShare.Business.Abstraction;

public interface INotifyManager
{
    Task<Result<TcpListener>> CreateListenerAsync(CancellationToken token);
    Task<Result<TcpListener>> CreateRequestReceiverAsync(CancellationToken token);
    Task<Result<TcpListener>> CreateResponseReceiverAsync(CancellationToken token);
    Task<Result> SendRequestAsync(string destinationIp, int port, CancellationToken token);
    Task<Result> SendResponseAsync(Responses response, string destinationIp, int port, CancellationToken token);
    IAsyncEnumerable<MessageModel?> GetReceivedFilenameAsync(CancellationToken token);
    IAsyncEnumerable<string?> GetRequestAsync(CancellationToken token);
    Task<Result> SendFilenameAsync(string destinationIp, int port, string filename, CancellationToken token);
}