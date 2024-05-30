using System.Net.Sockets;
using FluentResults;

namespace FileShare.Business.Abstraction;

public interface INotifyManager
{
    Task<Result<TcpListener>> CreateListenerAsync();
    Task<Result> SendRequestAsync(string destinationIp, int port);
    Task<Result> SendResponseAsync(bool response);
    IAsyncEnumerable<string> GetReceivedFilenameAsync();
    Task<Result> SendFilenameAsync(string destinationIp, int port, string filename);
}