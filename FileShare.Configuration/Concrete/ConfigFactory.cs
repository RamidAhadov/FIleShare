using System.Reflection;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.ConfigItem.Abstraction;
using FileShare.Configuration.Exceptions;
using Microsoft.Extensions.Configuration;

namespace FileShare.Configuration.Concrete;

public class ConfigFactory:IConfigFactory
{
    private readonly IConfigurationRoot _configuration;

    public ConfigFactory()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("ConfigItem/Settings/appsettings.json");
        _configuration = builder.Build();
    }
    
    public IConfigItem GetConfiguration(string section)
    {
        var configSection = _configuration.GetSection(section);
        if (configSection.Exists())
        {
            var configType = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.Name.Equals(section, StringComparison.OrdinalIgnoreCase) && typeof(IConfigItem).IsAssignableFrom(t));
            if (configType != null)
            {
                var configInstance = (IConfigItem)configSection.Get(configType);
                return configInstance;
            }
        }

        throw new ConfigSectionException(section);
    }
}