using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.ConfigItem.Concrete;

public class SftpDirectory:IConfigItem
{
    public string Path { get; set; }
}