using System.Diagnostics;
using FileShare.Business.Abstraction;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem;
using FileShare.Configuration.ConfigItem.Concrete;
using FluentResults;
using NLog;
using NuGet.Protocol;
using Renci.SshNet;

namespace FileShare.Business.Concrete;

public class SftpConnectionManager : IConnectionManager
{
    private static SftpClient _sftpClient;

    private readonly ConnectionParameters _connectionParameters;
    private readonly SftpDirectory _sftpDirectory;
    private readonly DownloadDirectory _downloadDirectory;
    private readonly Logger _logger;

    public SftpConnectionManager(IConfigFactory configFactory)
    {
        _connectionParameters = (ConnectionParameters)configFactory.GetConfiguration("ConnectionParameters");
        _sftpDirectory = (SftpDirectory)configFactory.GetConfiguration("SftpDirectory");
        _downloadDirectory = (DownloadDirectory)configFactory.GetConfiguration("DownloadDirectory");
        _logger = LogManager.GetLogger("ConnectionManagerLogger");
    }

    public SftpConnectionManager(ConnectionParameters connectionParameters, IConfigFactory configFactory) : this(
        configFactory)
    {
        _connectionParameters = connectionParameters;
    }

    public async Task<Result> ConnectServerAsync(CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _sftpClient = new SftpClient(_connectionParameters.Uri, _connectionParameters.Port,
                _connectionParameters.Username, _connectionParameters.Password);
            await _sftpClient.ConnectAsync(token);
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectServerAsync),
                Message = "Connected to SFTP server as server."
            }.ToJson());

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectServerAsync),
                Message = "Operation was cancelled."
            });

            return Result.Fail(errorMessage: "Operation was cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectServerAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
        finally
        {
            sw.Stop();
        }
    }

    public async Task<Result<string>> UploadFileAsync(string filePath, string receiverIp, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (!File.Exists(filePath))
            {
                return Result.Fail(errorMessage: $"File '{filePath}' does not exist.");
            }

            var fileName = Path.GetFileName(filePath) + "-" + receiverIp;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await Task.Run(
                    () =>
                    {
                        _sftpClient.UploadFile(fileStream, _sftpDirectory.Path + fileName, UploadProgressCallback);
                    },
                    token);
            }

            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms",
                Method = nameof(UploadFileAsync),
                Message = $"File uploaded from {filePath} to {_sftpDirectory.Path} directory."
            });

            return Result.Ok(fileName);
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms",
                Method = nameof(UploadFileAsync),
                Message = "Operation was cancelled."
            });

            return Result.Fail(errorMessage: "Operation was cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms",
                Method = nameof(UploadFileAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(errorMessage: e.InnerException?.Message ?? e.Message);
        }
        finally
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms",
                Method = nameof(UploadFileAsync),
                Message = "Process completed."
            });
            sw.Stop();
        }
    }


    public async Task<Result> DownloadFileAsync(string filename, CancellationToken token,
        string? localTargetPath = default)//TODO - filename is the path of the file where located on senders end. Just need a filename + endpoint.
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var remoteFilePath = Path.Combine(_sftpDirectory.Path, filename);
            var dashIndex = filename.LastIndexOf('-');
            filename = filename[..dashIndex];
            string localFilePath;
            if (localTargetPath != null)
            {
                localFilePath = Path.Combine(localTargetPath, filename);
            }
            else
            {
                localFilePath = Path.Combine(_downloadDirectory.Path, filename);
            }
            
            using (var file = File.OpenWrite(localFilePath))
            {
                await Task.Run(() => { _sftpClient.DownloadFile(remoteFilePath, file, DownloadProgressCallback); },
                    token);
            }

            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(DownloadFileAsync),
                Message = $"File downloaded from {_sftpDirectory.Path} to {localFilePath} directory."
            });

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(DownloadFileAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(DownloadFileAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(errorMessage: e.InnerException?.Message ?? e.Message);
        }
    }


    private void UploadProgressCallback(ulong uploadBytes)
    {
        _logger.Info(new
                { Method = nameof(UploadFileAsync), Message = $"File uploaded to the server. Bytes: {uploadBytes}" }
            .ToJson());
    }

    private void DownloadProgressCallback(ulong uploadBytes)
    {
        _logger.Info(new
                { Method = nameof(UploadFileAsync), Message = $"File downloaded to the server. Bytes: {uploadBytes}" }
            .ToJson());
    }
}