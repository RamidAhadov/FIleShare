using System.Diagnostics;
using FileShare.App.Services.Abstraction;
using FileShare.Business.Abstraction;
using FluentResults;
using NLog;
using NuGet.Protocol;

namespace FileShare.App.Services.Concrete;

public class EngineService : IEngineService
{
    private readonly IConnectionManager _connectionManager;
    private readonly INotifyManager _notifyManager;

    private Logger _logger;

    public EngineService(IConnectionManager connectionManager, INotifyManager notifyManager)
    {
        _connectionManager = connectionManager;
        _notifyManager = notifyManager;

        _logger = LogManager.GetLogger("EngineServiceLogger");
    }

    public async Task<Result> StartEngineAsync(IProgress<int> progress, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        var listenerTask = _notifyManager.CreateListenerAsync(token);
        var requestListenerTask = _notifyManager.CreateRequestReceiverAsync(token);
        var responseListenerTask = _notifyManager.CreateResponseReceiverAsync(token);
        var serverTask = _connectionManager.ConnectServerAsync(token);

        var tasks = new List<Task<Result>> { listenerTask, requestListenerTask, responseListenerTask, serverTask };

        int completedTasks = 0;
        foreach (var task in tasks)
        {
            var result = await task;
            completedTasks++;
            progress.Report(completedTasks);

            if (result.IsFailed)
            {
                string message = result == listenerTask.Result ? "Tcp listener not started" :
                    result == requestListenerTask.Result ? "Tcp request listener not started" :
                    result == responseListenerTask.Result ? "Tcp response listener not started" :
                    "Could not connect to SFTP server.";

                _logger.Error(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms",
                    Method = nameof(StartEngineAsync),
                    Message = message
                }.ToJson());

                return Result.Fail(message);
            }
            else
            {
                string message = result == listenerTask.Result ? "Tcp listener successfully started." :
                    result == requestListenerTask.Result ? "Tcp request listener successfully started." :
                    result == responseListenerTask.Result ? "Tcp response listener successfully started." :
                    "Connected to the SFTP server successfully.";

                _logger.Info(new
                {
                    Elapsed = $"{sw.ElapsedMilliseconds} ms",
                    Method = nameof(StartEngineAsync),
                    Message = message
                }.ToJson());
            }
        }

        sw.Stop();
        
        return Result.Ok();
    }
}