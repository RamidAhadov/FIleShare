using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.ConfigItem.Concrete;

public class ConnectionParameters:IConfigItem
{
    public string Uri { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
}