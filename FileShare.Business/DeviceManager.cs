using System.Net.NetworkInformation;
using FileShare.Business.Abstraction;

namespace FileShare.Business;

public class DeviceManager:IDeviceManager
{
    public List<string> GetLocalDeviceIPs()
    {
        throw new NotImplementedException();
    }

    public string? GetSubnetMask()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        
        foreach (NetworkInterface networkInterface in networkInterfaces)
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.Name == "en0")
            {
                IPInterfaceProperties properties = networkInterface.GetIPProperties();
                
                IPv4InterfaceProperties ipv4Properties = properties.GetIPv4Properties();
                if (ipv4Properties != null)
                {
                    foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.IPv4Mask.ToString();
                        }
                    }
                }
            }

            return null;
        }
        throw new NetworkInformationException();
    }
}