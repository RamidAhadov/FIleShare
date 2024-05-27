using System.Diagnostics;
using FileShare.Business.Abstraction;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem.Concrete;
using FluentResults;
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
    
    public async Task<Result<SftpClient>> ConnectAsServerAsync(CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var sftpClient = new SftpClient(_connectionParameters.Uri, _connectionParameters.Port,
                _connectionParameters.Username, _connectionParameters.Password);
            await sftpClient.ConnectAsync(token);
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectAsServerAsync), Message = "Connected to SFTP server as server." }.ToJson());

            return Result.Ok(sftpClient);
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SftpConnectionManager), Message = "Operation was cancelled."});
            
            return Result.Fail(errorMessage: "Operation was cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(ConnectAsServerAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());

            return Result.Fail(errorMessage: e.InnerException?.Message ?? e.Message);
        }
    }

    public async Task<Result<string>> UploadFileAsync(SftpClient client, string filePath, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using (var fileStream = new FileStream(filePath,FileMode.Open))
            {
                var fileName = Path.GetFileName(filePath);
                await Task.Run(() =>
                {
                    client.UploadFile(fileStream, _sftpDirectory.Path + fileName, ProgressCallback);
                }, token);
                
                _logger.Info(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(UploadFileAsync), Message = $"File uploaded from {filePath} to {_sftpDirectory.Path} directory."});

                return Result.Ok(fileName);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(UploadFileAsync), Message = "Operation was cancelled."});
            
            return Result.Fail(errorMessage: "Operation was cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(UploadFileAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(errorMessage: e.InnerException?.Message ?? e.Message);
        }
    }


    private void ProgressCallback(ulong uploadBytes)
    {
        _logger.Info(new {Method = nameof(UploadFileAsync), Message = $"File uploaded to the server. Bytes: {uploadBytes}"}.ToJson());
    }
}