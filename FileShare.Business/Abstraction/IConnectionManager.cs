using FluentResults;
using Renci.SshNet;

namespace FileShare.Business.Abstraction;

public interface IConnectionManager
{
    Task<Result<SftpClient>> ConnectAsServerAsync(CancellationToken token);
    Task<Result<string>> UploadFileAsync(SftpClient client, string filePath, string receiverIp, CancellationToken token);
}