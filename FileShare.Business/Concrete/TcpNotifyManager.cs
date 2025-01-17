using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using FileShare.Business.Abstraction;
using FileShare.Business.Constants;
using FileShare.Business.Models;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem.Concrete;
using FluentResults;
using Newtonsoft.Json;
using NLog;
using NuGet.Protocol;

namespace FileShare.Business.Concrete;

public class TcpNotifyManager : INotifyManager
{
    private static TcpListener? _tcpListener;
    private static TcpListener? _tcpRequestReceiver;
    private static TcpListener? _tcpResponseReceiver;
    private static List<TcpClient> _tcpClients;
    private static List<TcpClient> _requesterTcpClients;
    private static string _myIp;

    private readonly TcpListenerParameters _tcpListenerParameters;
    private readonly TcpRequestReceiverParameters _tcpRequestReceiverParameters;
    private readonly TcpResponseReceiverParameters _tcpResponseReceiverParameters;
    private readonly Logger _logger;

    public TcpNotifyManager(IConfigFactory configFactory)
    {
        _tcpClients = new List<TcpClient>();
        _requesterTcpClients = new List<TcpClient>();
        _myIp = GetCurrentIPv4Address();

        _tcpListenerParameters = (TcpListenerParameters)configFactory.GetConfiguration("TcpListenerParameters");
        _tcpRequestReceiverParameters =
            (TcpRequestReceiverParameters)configFactory.GetConfiguration("TcpRequestReceiverParameters");
        _tcpResponseReceiverParameters =
            (TcpResponseReceiverParameters)configFactory.GetConfiguration("TcpResponseReceiverParameters");
        _logger = LogManager.GetLogger("NotifyManagerLogger");
    }

    #region Create TCP listener

    public async Task<Result> CreateListenerAsync(CancellationToken token, bool restart)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (restart)
            {
                _tcpListener =
                    await StartTcpListenerAsync(_tcpListener, IPAddress.Any, _tcpListenerParameters.Port,
                        token);
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync),
                    Message =
                        $"Tcp listener restarted and listening on: {_tcpListenerParameters.Host}:{_tcpListenerParameters.Port}"
                }.ToJson());
            }
            else
            {
                if (_tcpListener == null)
                {
                    _tcpListener =
                        await StartTcpListenerAsync(_tcpListener, IPAddress.Any, _tcpListenerParameters.Port,
                            token);
                    _logger.Info(new
                    {
                        Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync),
                        Message =
                            $"Tcp listener started and listening on: {_tcpListenerParameters.Host}:{_tcpListenerParameters.Port}"
                    }.ToJson());
                }
            }

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateListenerAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public async Task<Result> CreateRequestReceiverAsync(CancellationToken token, bool restart)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (restart)
            {
                _tcpRequestReceiver = await StartTcpListenerAsync(_tcpRequestReceiver, IPAddress.Any,
                    _tcpRequestReceiverParameters.Port, token);
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync),
                    Message =
                        $"Tcp request receiver restarted and listening on: {_tcpRequestReceiverParameters.Host}:{_tcpRequestReceiverParameters.Port}"
                }.ToJson());
            }
            else
            {
                if (_tcpRequestReceiver == null)
                {
                    _tcpRequestReceiver = await StartTcpListenerAsync(_tcpRequestReceiver, IPAddress.Any,
                        _tcpRequestReceiverParameters.Port, token);
                    _logger.Info(new
                    {
                        Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync),
                        Message =
                            $"Tcp request receiver started and listening on: {_tcpRequestReceiverParameters.Host}:{_tcpRequestReceiverParameters.Port}"
                    }.ToJson());
                }
            }

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateRequestReceiverAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    public async Task<Result> CreateResponseReceiverAsync(CancellationToken token, bool restart)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (restart)
            {
                _tcpResponseReceiver = await StartTcpListenerAsync(_tcpResponseReceiver, IPAddress.Any,
                    _tcpResponseReceiverParameters.Port, token);
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync),
                    Message =
                        $"Tcp response receiver restarted and listening on: {_tcpResponseReceiverParameters.Host}:{_tcpResponseReceiverParameters.Port}"
                }.ToJson());
            }
            else
            {
                if (_tcpResponseReceiver == null)
                {
                    _tcpResponseReceiver = await StartTcpListenerAsync(_tcpResponseReceiver, IPAddress.Any,
                        _tcpResponseReceiverParameters.Port, token);
                    _logger.Info(new
                    {
                        Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync),
                        Message =
                            $"Tcp response receiver started and listening on: {_tcpResponseReceiverParameters.Host}:{_tcpResponseReceiverParameters.Port}"
                    }.ToJson());
                }
            }

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(CreateResponseReceiverAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    #endregion

    #region Send request to receiver

    public async Task<Result> SendRequestAsync(string destinationIp, CancellationToken token, int port = 8990)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(destinationIp, port, token);
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                    Message = $"Request sent to {destinationIp}"
                }.ToJson());

                var tcpClient = await _tcpResponseReceiver.AcceptTcpClientAsync(token);
                var response = await this.GetMessage(tcpClient, token);

                ArgumentNullException.ThrowIfNull(response);

                while (!this.IsResponse(response, destinationIp))
                {
                    _logger.Warn(new
                    {
                        Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                        Message = $"Response is not correct: {response}.", DestinationIp = destinationIp
                    }.ToJson());
                    response = await this.GetMessage(tcpClient, token);
                }

                if (ResponseResult(response.Message))
                {
                    _logger.Info(new
                    {
                        Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                        Message = $"Request accepted by {destinationIp}."
                    }.ToJson());

                    return Result.Ok();
                }

                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                    Message = $"Request not accepted by {destinationIp}."
                }.ToJson());

                return Result.Fail("Receiver not accepted the file.");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendRequestAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    #endregion

    #region Send confirm response

    public async Task<Result> SendResponseAsync(Responses response, string destinationIp, CancellationToken token,
        int port = 8991)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var model = new MessageModel
            {
                Message = response.ToString(),
                Endpoint = _myIp
            };
            var message = JsonConvert.SerializeObject(model);
            await SendMessageAsync(destinationIp, port, message, token);
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendResponseAsync),
                Message = $"Response sent to {destinationIp}", Response = $"{response}"
            }.ToJson());

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendResponseAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendResponseAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
    }

    #endregion

    #region Get request

    public async IAsyncEnumerable<MessageModel?> GetReceivedFilenameAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var client in this.AcceptClientsAsync(_tcpListener).WithCancellation(token))
        {
            yield return await GetMessage(client, token);
        }
    }

    public async IAsyncEnumerable<string?> GetRequestAsync([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var client in this.AcceptClientsAsync(_tcpRequestReceiver).WithCancellation(token))
        {
            yield return GetRemoteIpFromTcpClient(client);
        }
    }

    #endregion

    #region Filename send

    public async Task<Result> SendFilenameAsync(string destinationIp, string filename, CancellationToken token,
        int port = 8989)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var model = new MessageModel
            {
                Message = filename,
                Endpoint = _myIp
            };
            var message = JsonConvert.SerializeObject(model);

            await this.SendMessageAsync(destinationIp, port, message, token);
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendFilenameAsync),
                Message = $"Filename sent to {destinationIp}", Filename = filename
            }.ToJson());

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.Info(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendFilenameAsync),
                Message = "Operation cancelled."
            }.ToJson());

            return Result.Fail("Operation cancelled.");
        }
        catch (Exception e)
        {
            _logger.Error(new
            {
                Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(SendFilenameAsync),
                Message = e.InnerException?.Message ?? e.Message
            }.ToJson());

            return Result.Fail(e.InnerException?.Message ?? e.Message);
        }
        finally
        {
            sw.Stop();
        }
    }

    #endregion

    private async IAsyncEnumerable<TcpClient> AcceptClientsAsync(TcpListener? listener)
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            if (listener != null)
            {
                var socket = await listener.AcceptTcpClientAsync();
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(AcceptClientsAsync),
                    Message = "New socket accepted."
                }.ToJson());
                AddClient(socket, _tcpClients);
                sw.Stop();

                yield return socket;
            }
            else
            {
                sw.Stop();

                yield break;
            }
        }
    }

    private async Task<MessageModel?> GetMessage(TcpClient client, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        if (client.Connected)
        {
            try
            {
                var message = await Task.Run(() => this.ReceiveMessage(client), token);
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);

                return messageModel;
            }
            catch (OperationCanceledException)
            {
                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(GetMessage),
                    Message = "Operation cancelled."
                }.ToJson());
            }
            catch (Exception e)
            {
                _logger.Error(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms", Method = nameof(GetMessage),
                    Message = e.InnerException?.Message ?? e.Message
                }.ToJson());
            }
            finally
            {
                sw.Stop();
                client.Close();
            }
        }

        return null;
    }

    private async Task<string> ReceiveMessage(TcpClient client)
    {
        using (client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return message;
            }
        }

        return string.Empty;
    }

    [Obsolete]
    private async Task<string?> ReceiveMessage(Socket client)
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
        catch (Exception)
        {
            client.Dispose();
            return null;
        }
    }

    private string GetRemoteIpFromTcpClient(TcpClient client)
    {
        if (client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
        {
            return remoteEndPoint.Address.ToString();
        }

        return string.Empty;
    }

    private void AddClient(TcpClient client, List<TcpClient> clients)
    {
        clients.Add(client);
    }

    private void RemoveClient(TcpClient client, List<TcpClient> clients)
    {
        clients.Remove(client);
    }

    private bool IsResponse(MessageModel response, string destinationIp)
    {
        if (response.Endpoint == destinationIp)
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
        return response == Responses.Accept.ToString();
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

    private async Task<TcpListener?> StartTcpListenerAsync(TcpListener? listener, string host, int port,
        CancellationToken token)
    {
        var ipAddress = IPAddress.Parse(host);
        await Task.Run(() =>
        {
            if (listener == null)
            {
                listener = new TcpListener(ipAddress, port);
                listener.Start();
            }
        }, token);

        return listener;
    }

    private async Task<TcpListener?> StartTcpListenerAsync(TcpListener? listener, IPAddress ipAddress, int port,
        CancellationToken token)
    {
        await Task.Run(() =>
        {
            if (listener == null)
            {
                listener = new TcpListener(ipAddress, port);
                listener.Start();
            }
        }, token);

        return listener;
    }

    private string? GetSenderIpFromMessage(string message)
    {
        var dashIndex = message.LastIndexOf('-');
        return message[(dashIndex + 1)..];
    }

    private async Task SendMessageAsync(string destinationIp, int port, string message, CancellationToken token)
    {
        var tcpClient = new TcpClient(destinationIp, port);
        var stream = tcpClient.GetStream();
        var data = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(data, token);
        await Task.Delay(2000, token);
        await stream.FlushAsync(token);
    }
}