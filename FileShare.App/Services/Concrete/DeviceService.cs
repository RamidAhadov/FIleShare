using FileShare.App.Services.Abstraction;
using FileShare.Business.Abstraction;

namespace FileShare.App.Services.Concrete;

public class DeviceService:IDeviceService
{
    private IDeviceManager _deviceManager;

    public DeviceService(IDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    public async IAsyncEnumerable<string> GetDevicesAsync()
    {
        await foreach (var ip in _deviceManager.GetLocalDeviceIPsAsync())
        {
            yield return ip;
        }
    }

    public int GetProgressCount()
    {
        return _deviceManager.GetProgressCount();
    }
}