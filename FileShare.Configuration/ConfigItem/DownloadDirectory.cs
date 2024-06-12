using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.ConfigItem;

public class DownloadDirectory:IConfigItem
{
    public string Path { get; set; }
}