namespace FileShare.App.Services.Abstraction;

public interface IDeviceService
{
    IAsyncEnumerable<string> GetDevicesAsync();
    int GetProgressCount();
}