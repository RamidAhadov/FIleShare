using FluentResults;
using Renci.SshNet;

namespace FileShare.Business.Abstraction;

public interface IConnectionManager
{
    Task<Result> ConnectServerAsync(CancellationToken token);
    Task<Result<string>> UploadFileAsync(string filePath, string receiverIp, CancellationToken token);
    Task<Result> DownloadFileAsync(string filename, string localTargetPath, CancellationToken token);
}