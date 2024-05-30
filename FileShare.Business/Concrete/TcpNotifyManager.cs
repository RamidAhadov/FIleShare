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
    private static TcpListener? _tcpRequestReceiver;
    private static TcpListener? _tcpResponseReceiver;
    private static List<Socket> _sockets;
    private static List<Socket> _requesterSockets;
    private static string _myIp;
    
    private readonly TcpListenerParameters _tcpListenerParameters;
    private readonly TcpRequestReceiverParameters _tcpRequestReceiverParameters;
    private readonly TcpResponseReceiverParameters _tcpResponseReceiverParameters;
    private readonly Logger _logger;

    public TcpNotifyManager(IConfigFactory configFactory)
    {
        _sockets = new List<Socket>();
        _requesterSockets = new List<Socket>();
        _myIp = GetCurrentIPv4Address();
        
        _tcpListenerParameters = (TcpListenerParameters)configFactory.GetConfiguration("TcpListenerParameters");
        _tcpRequestReceiverParameters = (TcpRequestReceiverParameters)configFactory.GetConfiguration("TcpRequestReceiverParameters");
        _tcpResponseReceiverParameters = (TcpResponseReceiverParameters)configFactory.GetConfiguration("TcpResponseReceiverParameters");
        _logger = LogManager.GetLogger("NotifyManagerLogger");
    }

    #region Create TCP listener
    public async Task<Result<TcpListener>> CreateListenerAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _tcpListener = await StartTcpListenerAsync(_tcpListener, _tcpListenerParameters.Host, _tcpListenerParameters.Port);
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync), Message = $"Tcp listener started and listening on: {_tcpListenerParameters.Host}:{_tcpListenerParameters.Port}"}.ToJson());
            
            return Result.Ok(_tcpListener);
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public async Task<Result<TcpListener>> CreateRequestReceiverAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _tcpRequestReceiver = await StartTcpListenerAsync(_tcpRequestReceiver, IPAddress.Any,
                _tcpRequestReceiverParameters.Port);
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync), Message = $"Tcp request receiver started and listening on: {_tcpRequestReceiverParameters.Host}:{_tcpRequestReceiverParameters.Port}"}.ToJson());
            
            return Result.Ok(_tcpRequestReceiver);
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public async Task<Result<TcpListener>> CreateResponseReceiverAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _tcpResponseReceiver = await StartTcpListenerAsync(_tcpResponseReceiver, IPAddress.Any,
                _tcpResponseReceiverParameters.Port);
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync), Message = $"Tcp response receiver started and listening on: {_tcpResponseReceiverParameters.Host}:{_tcpResponseReceiverParameters.Port}"}.ToJson());
            
            return Result.Ok(_tcpResponseReceiver);
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    #endregion
    
    #region Send request to receiver
    public async Task<Result> SendRequestAsync(string destinationIp, int port)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(destinationIp, port);
                _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request sent to {destinationIp}"}.ToJson());

                var socket = await _tcpResponseReceiver.AcceptSocketAsync();
                var response = await GetMessage(socket);
                while (!IsResponse(response))
                {
                    response = await GetMessage(socket);
                }
                if (ResponseResult(response))
                {
                    _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request accepted by {destinationIp}."}.ToJson());
                        
                    return Result.Ok();
                }
                    
                _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request not accepted by {destinationIp}."}.ToJson());
                    
                return Result.Fail("Receiver not accepted the file.");
                //using (var stream = client.GetStream())
                //{
                //    var response = await ReadBufferAsync(stream);
//
                //    while (!IsResponse(response))
                //    {
                //        response = await ReadBufferAsync(stream);
                //    }
//
                //    if (ResponseResult(response))
                //    {
                //        _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request accepted by {destinationIp}."}.ToJson());
                //        
                //        return Result.Ok();
                //    }
                //    
                //    _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = $"Request not accepted by {destinationIp}."}.ToJson());
                //    
                //    return Result.Fail("Receiver not accepted the file.");
                //}
            }
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    #endregion

    public async Task<Result> SendResponseAsync(bool response, string destinationIp, int port)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await SendResponseAsync(destinationIp, port, response.ToString());
            Console.WriteLine($"Response sent to: {destinationIp}. Response {response}"); //Will be removed
            _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendResponseAsync), Message = $"Response sent to {destinationIp}", Response = $"{response}"}.ToJson());
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.Error(new {Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendResponseAsync), Message = e.InnerException?.Message ?? e.Message}.ToJson());
            
            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public async IAsyncEnumerable<string> GetReceivedFilenameAsync()
    {
        await foreach (var client in this.AcceptClientsAsync(_tcpListener))
        {
            yield return await GetMessage(client);
        }
    }

    public async IAsyncEnumerable<string?> GetRequestAsync()
    {
        await foreach (var client in this.AcceptClientsAsync(_tcpRequestReceiver))
        {
            Console.WriteLine("Request received from: "+ GetRemoteIpFromSocket(client)); //Will be removed
            yield return GetRemoteIpFromSocket(client);
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


    private async IAsyncEnumerable<Socket> AcceptClientsAsync(TcpListener? listener)
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            if (listener != null)
            {
                var socket = await listener.AcceptSocketAsync();
                _logger.Info(new { Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(AcceptClientsAsync), Message = "New socket accepted."}.ToJson());
                AddClient(socket, _sockets);
                
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
            await client.DisconnectAsync(true); //TODO
            RemoveClient(client, _sockets);

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
            
            return string.Empty;
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

    private void AddClient(Socket socket, List<Socket> sockets)
    {
        sockets.Add(socket);
    }

    private void RemoveClient(Socket socket, List<Socket> sockets)
    {
        sockets.Remove(socket);
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

    private async Task<TcpListener?> StartTcpListenerAsync(TcpListener? listener,string host, int port)
    {
        var ipAddress = IPAddress.Parse(host);
        await Task.Run(() => 
        {
            if (listener == null)
            {
                listener = new TcpListener(ipAddress,  port);
                listener.Start();
            }
        });

        return listener;
    }
    private async Task<TcpListener?> StartTcpListenerAsync(TcpListener? listener,IPAddress ipAddress, int port)
    {
        await Task.Run(() => 
        {
            if (listener == null)
            {
                listener = new TcpListener(ipAddress,  port);
                listener.Start();
            }
        });

        return listener;
    }

    private string? GetSenderIpFromMessage(string message)
    {
        var dashIndex = message.LastIndexOf('-');
        return message[(dashIndex + 1)..];
    }

    private string? GetRemoteIpFromSocket(Socket socket)
    {
        if (socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
        {
            return remoteEndPoint.Address.ToString();
        }

        return null;
    }

    private async Task SendResponseAsync(string destinationIp, int port, string response)
    {
        var tcpClient = new TcpClient(destinationIp, port);
        var stream = tcpClient.GetStream();
        var data = Encoding.UTF8.GetBytes(response.ToString());
        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }
}