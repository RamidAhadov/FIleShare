using FileShare.Configuration.ConfigItem.Abstraction;

namespace FileShare.Configuration.Abstraction;

public interface IConfigFactory
{
    IConfigItem GetConfiguration(string section);
}