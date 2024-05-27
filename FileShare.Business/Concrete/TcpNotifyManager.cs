using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FileShare.Business.Abstraction;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem.Concrete;
using FluentResults;
using NLog;
using NuGet.Protocol;

namespace FileShare.Business.Concrete;

public class TcpNotifyManager:INotifyManager
{
    private static TcpListener? _tcpListener;
    
    private readonly TcpListenerParameters _tcpListenerParameters;
    private readonly Logger _logger;

    public TcpNotifyManager(IConfigFactory configFactory)
    {
        _tcpListenerParameters = (TcpListenerParameters)configFactory.GetConfiguration("TcpListenerParameters");
        _logger = LogManager.GetLogger("NotifyManagerLogger");
    }

    public async Task<Result<TcpListener>> CreateListenerAsync()
    {
        var sw = Stopwatch.StartNew();
        var ipAddress = IPAddress.Parse(_tcpListenerParameters.Host);
        try
        {
            await Task.Run(() =>
            {
                if (_tcpListener == null)
                {
                    _tcpListener = new TcpListener(ipAddress, _tcpListenerParameters.Port);
                    _tcpListener.Start();
                
                    _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync), Message = $"Tcp listener started and listening on: {_tcpListenerParameters.Host}:{_tcpListenerParameters.Port}"}.ToJson());
                }
            });
            
            return Result.Ok(_tcpListener);
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }
    
    private static async Task HandleClient(TcpClient client)
    {
        byte[] buffer = new byte[256];
        NetworkStream stream = client.GetStream();

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {data}");

            byte[] msg = Encoding.UTF8.GetBytes("Echo: " + data);
            Console.WriteLine($"Sent: Echo: {data}");
        }

        client.Close();
    }
}