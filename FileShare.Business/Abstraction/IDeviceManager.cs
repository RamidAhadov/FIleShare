namespace FileShare.Business.Abstraction;

public interface IDeviceManager
{
    List<string> GetLocalDeviceIPs(string? subnetMask = default, int timeOut = 500);
    IAsyncEnumerable<string> GetLocalDeviceIPsAsync(string? subnetMask = default, int timeOut = 500);
    string? GetSubnetMask();
}