using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
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
    private static TcpListener? _requestReceiver;
    private static List<Socket> _sockets;
    private static string _myIp;
    
    private readonly TcpListenerParameters _tcpListenerParameters;
    private readonly Logger _logger;

    public TcpNotifyManager(IConfigFactory configFactory)
    {
        _tcpListenerParameters = (TcpListenerParameters)configFactory.GetConfiguration("TcpListenerParameters");
        _sockets = new List<Socket>();
        _myIp = GetCurrentIPv4Address();
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

    public async Task<Result> SendRequestAsync(string destinationIp, int port)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(destinationIp, port);
                _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request sent to {destinationIp}"}.ToJson());

                using (var stream = client.GetStream())
                {
                    var response = await ReadBufferAsync(stream);

                    while (!IsResponse(response))
                    {
                        response = await ReadBufferAsync(stream);
                    }

                    if (ResponseResult(response))
                    {
                        _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request accepted by {destinationIp}."}.ToJson());
                        
                        return Result.Ok();
                    }
                    
                    _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request not accepted by {destinationIp}."}.ToJson());
                    
                    return Result.Fail("Receiver not accepted the file.");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public Task<Result> SendResponseAsync(bool response)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<string> GetReceivedFilenameAsync()
    {
        await foreach (var client in this.AcceptClientsAsync())
        {
            yield return await GetMessage(client);
        }
    }

    public async Task<Result> SendFilenameAsync(string destinationIp, int port, string filename)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var tcpClient = new TcpClient(destinationIp, port);
            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(filename);
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendFilenameAsync), Message = $"Filename sent to {destinationIp}", Filename = filename}.ToJson());
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendFilenameAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }


    private async IAsyncEnumerable<Socket> AcceptClientsAsync()
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            if (_tcpListener != null)
            {
                var socket = await _tcpListener.AcceptSocketAsync();
                _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(AcceptClientsAsync), Message = "New socket accepted."}.ToJson());
                AddClient(socket);
                
                yield return socket;
            }

            yield break;
        }
    }
    
    private async Task<string> GetMessage(Socket client)
    {
        var endPointIP = this.GetEndpointFromClient(client);

        if(client.Connected)
        {
            var message = await Task.Run(() => ReceiveMessages(client));
            message = message + "-" + endPointIP;
            await client.DisconnectAsync(true);
            RemoveClient(client);

            return message;
        }
        
        return string.Empty;
    }

    private async Task<string?> ReceiveMessages(Socket client)
    {
        try
        {
            byte[] data = new byte[100];

            int bytesRead = await client.ReceiveAsync(data, SocketFlags.None);

            if (bytesRead > 0)
            {
                string result = Encoding.UTF8.GetString(data, 0, bytesRead);
                return result;
            }

            await client.DisconnectAsync(false);
            return "null";
        }
        catch (Exception e)
        {
            client.Dispose();
            return null;
        }
    }
    
    private string GetEndpointFromClient(Socket client)
    {
        return ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
    }

    private void AddClient(Socket socket)
    {
        _sockets.Add(socket);
    }

    private void RemoveClient(Socket socket)
    {
        _sockets.Remove(socket);
    }

    private bool IsResponse(string stream)
    {
        if (stream.EndsWith(_myIp))
        {
            return true;
        }

        return false;
    }

    private async Task<string> ReadBufferAsync(NetworkStream? stream)
    {
        var buffer = new byte[256];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    private bool ResponseResult(string response)
    {
        return bool.Parse(response);
    }
    
    private string GetCurrentIPv4Address()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var uniCastAddresses = ipProperties.UnicastAddresses;

                foreach (var uniCastAddress in uniCastAddresses)
                {
                    if (uniCastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IPAddress.Parse(uniCastAddress.Address.ToString()).ToString();
                    }
                }
            }
        }

        throw new NetworkInformationException();
    }
}