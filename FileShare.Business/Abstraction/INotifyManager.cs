using System.Net.Sockets;
using FluentResults;

namespace FileShare.Business.Abstraction;

public interface INotifyManager
{
    Task<Result<TcpListener>> CreateListenerAsync();
}