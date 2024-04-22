namespace FileShare.Business.Abstraction;

public interface IDeviceManager
{
    List<string> GetLocalDeviceIPs(string? subnetMask = default);
    string? GetSubnetMask();
}