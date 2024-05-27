using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.Abstraction;

public interface IConfiguration
{
    IConfigItem GetConfiguration(string section);
}