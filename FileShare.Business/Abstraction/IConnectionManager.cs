using Renci.SshNet;

namespace FileShare.Business.Abstraction;

public interface IConnectionManager
{
    Task<SftpClient> ConnectAsServerAsync(CancellationToken token);
    Task UploadFileAsync(SftpClient client, string filePath);
}