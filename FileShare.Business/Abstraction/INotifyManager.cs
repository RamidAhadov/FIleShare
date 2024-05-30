using System.Net.Sockets;
using FluentResults;

namespace FileShare.Business.Abstraction;

public interface INotifyManager
{
    Task<Result<TcpListener>> CreateListenerAsync();
    Task<Result<TcpListener>> CreateRequestReceiverAsync();
    Task<Result<TcpListener>> CreateResponseReceiverAsync();
    Task<Result> SendRequestAsync(string destinationIp, int port);
    Task<Result> SendResponseAsync(bool response, string destinationIp, int port);
    IAsyncEnumerable<string> GetReceivedFilenameAsync();
    IAsyncEnumerable<string?> GetRequestAsync();
    Task<Result> SendFilenameAsync(string destinationIp, int port, string filename);
}