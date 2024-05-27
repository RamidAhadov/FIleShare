using System.Reflection;
using FileShare.Configuration.ConfigItem.Abstraction;
using Microsoft.Extensions.Configuration;
using IConfiguration = FileShare.Configuration.Abstraction.IConfiguration;

namespace FileShare.Configuration.Concrete;

public class Configuration:IConfiguration
{
    private readonly IConfigurationRoot _configuration;

    public Configuration()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("ConfigItem/appsettings.json");
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
        return null;
    }
}