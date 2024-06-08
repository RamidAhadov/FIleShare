namespace FileShare.Business.Abstraction;

public interface IDeviceManager
{
    List<string> GetLocalDeviceIPs(string? subnetMask = default, int timeOut = 500);
    IAsyncEnumerable<string> GetLocalDeviceIPsAsync(int timeOut = 500);
    string? GetSubnetMask();
    Task<bool> IsOnline(string ip);
    int GetProgressCount();
}