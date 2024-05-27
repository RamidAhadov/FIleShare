using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.ConfigItem.Concrete;

public class TcpListenerParameters:IConfigItem
{
    public string Host { get; set; }
    public int Port { get; set; }
}