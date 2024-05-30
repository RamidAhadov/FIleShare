using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.ConfigItem.Concrete;

public class TcpResponseReceiverParameters:IConfigItem
{
    public string Host { get; set; }
    public int Port { get; set; }
}