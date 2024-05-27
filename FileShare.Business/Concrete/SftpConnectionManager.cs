using System.Diagnostics;
using FileShare.Business.Abstraction;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem.Concrete;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using NLog;
using NuGet.Protocol;
using Renci.SshNet;

namespace FileShare.Business.Concrete;

public class SftpConnectionManager:IConnectionManager
{
    private readonly ConnectionParameters _connectionParameters;
    private readonly SftpDirectory _sftpDirectory;
    private readonly Logger _logger;
    public SftpConnectionManager(IConfigFactory configFactory)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _connectionParameters = (ConnectionParameters)configFactory.GetConfiguration("ConnectionParameters");
            _sftpDirectory = (SftpDirectory)configFactory.GetConfiguration("SftpDirectory");
            _logger = LogManager.GetLogger("ConnectionManagerLogger");
        }
        catch (Exception e)
        {
            _logger = LogManager.GetLogger("ConnectionManagerLogger");
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SftpConnectionManager), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            throw;
        }
    }
    
    public async Task<SftpClient> ConnectAsServerAsync(CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var sftpClient = new SftpClient(_connectionParameters.Uri, _connectionParameters.Port,
                _connectionParameters.Username, _connectionParameters.Password);
            await sftpClient.ConnectAsync(token);
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectAsServerAsync), Message = "Connected to SFTP server as server." }.ToJson());

            return sftpClient;
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SftpConnectionManager), Message = "Operation was cancelled."});
            throw;
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectAsServerAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            throw;
        }
    }

    public async Task UploadFileAsync(SftpClient client, string filePath)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using (var fileStream = new FileStream(filePath,FileMode.Open))
            {
                await Task.Run(() =>
                {
                    client.UploadFile(fileStream, _sftpDirectory.Path, ProgressCallback);
                });
                
                _logger.Info(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(UploadFileAsync), Message = $"File uploaded from {filePath} to {_sftpDirectory.Path}."});
            }
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(UploadFileAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            await Task.CompletedTask;
        }
    }


    private void ProgressCallback(ulong uploadBytes)
    {
        _logger.Info(new {Method = nameof(UploadFileAsync), Message = $"File uploaded to the server. Bytes: {uploadBytes}"}.ToJson());
    }
}