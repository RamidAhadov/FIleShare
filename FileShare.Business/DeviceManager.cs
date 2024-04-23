using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using FileShare.Business.Abstraction;
using FileShare.Business.Constants;

namespace FileShare.Business;

public class DeviceManager:IDeviceManager
{
    private static readonly object lockObject = new ();
    public List<string> GetLocalDeviceIPs(string? subnetMask = default, int timeOut = 500)
    {
        var localDeviceIPs = new List<string>();
        var IP = GetLocalIPAddress();
        var sw = Stopwatch.StartNew();
        int[] loopCounts = LoopCounts(subnetMask);

        var fullIPs = AllAvailableIPs(IP, loopCounts);
        var threads = new Thread[100];
        int attempt = 0;
        foreach (var ip in fullIPs)
        {
            if (attempt == 100)
            {
                attempt = 0;
            }

            threads[attempt] = new Thread(() => SendPing(ip, localDeviceIPs, timeOut));
            threads[attempt].Start();
            attempt++;
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
        

        Console.WriteLine(sw.ElapsedMilliseconds);
        foreach (var localDeviceIP in localDeviceIPs)
        {
            Console.WriteLine(localDeviceIP);
        }

        return localDeviceIPs;
    }

    public async Task<List<string>> GetLocalDeviceIPsAsync(string? subnetMask = default, int timeOut = 500)
    {
        var localDeviceIPs = new List<string>();
        var IP = GetLocalIPAddress();
        int[] loopCounts = LoopCounts(subnetMask);

        var fullIPs = AllAvailableIPs(IP, loopCounts);
        var tasks = new Task[100];
        int attempt = 0;
        foreach (var ip in fullIPs)
        {
            if (attempt == 100)
            {
                attempt = 0;
            }

            tasks[attempt] = Task.Run(async() => await SendPingAsync(ip, localDeviceIPs, timeOut));
            attempt++;
        }

        await Task.WhenAll(tasks);

        return localDeviceIPs;
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
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.IPv4Mask.ToString();
                        }
                    }
                }
            }
        }

        return null;
    }
    
    static string GetLocalIPAddress()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        
        foreach (NetworkInterface networkInterface in networkInterfaces)
        {
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var ipAddress = ipProperties.UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipAddress != null)
                {
                    return ipAddress.Address.ToString();
                }
            }
        }

        throw new Exception($"{ErrorMessages.LocalIPNotFound} \n {ErrorMessages.DNSInnerExceptionMessage}");
    }

    private int[] LoopCounts(string? subnetMask)
    {
        int[] loopCounts = new int[2];
        if (subnetMask == null)
        {
            loopCounts[0] = 1;
            loopCounts[1] = 256;
        }
        else
        {
            string[] subnetMaskSectors = subnetMask.Split('.');
            for (int i = 2; i < subnetMaskSectors.Length; i++)
            {
                loopCounts[i - 2] = 256 - int.Parse(subnetMaskSectors[i]);
            }
        }

        return loopCounts;
    }

    private List<string> AllAvailableIPs(string IP, int[] loopCounts)
    {
        var IPs = new List<string>();
        for (int i = 0; i < loopCounts[0]; i++)
        {
            var subIP = IP[..IP.LastIndexOf('.')];
            var lastOcta = subIP[(subIP.LastIndexOf('.') + 1)..];
            lastOcta = (int.Parse(lastOcta) + i).ToString();
            subIP = subIP[..(subIP.LastIndexOf('.') + 1)];
            subIP += lastOcta + ".";
            for (int j = 1; j < loopCounts[1]; j++)
            {
                string fullIP = subIP + j;
                IPs.Add(fullIP);
            }
        }
        
        return IPs;
    }

    private void SendPing(string ip, List<string> successIPs, int timeOut)
    {
        Ping ping = new Ping();
        var reply = ping.Send(ip, timeOut);
        if (reply.Status == IPStatus.Success)
        {
            lock (lockObject)
            {
                successIPs.Add(ip);
            }
        }
    }
    
    private async Task SendPingAsync(string ip, List<string> successIPs, int timeOut)
    {
        Ping ping = new Ping();
        var reply = await ping.SendPingAsync(ip, timeOut);
        if (reply.Status == IPStatus.Success)
        {
            lock (lockObject)
            {
                successIPs.Add(ip);
            }
        }
    }
}