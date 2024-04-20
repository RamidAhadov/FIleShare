namespace FileShare.Business.Abstraction;

public interface IDeviceService
{
    List<string> GetLocalDevices();
}