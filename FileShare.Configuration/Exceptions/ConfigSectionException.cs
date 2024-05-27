namespace FileShare.Configuration.Exceptions;

public class ConfigSectionException:Exception
{
    public ConfigSectionException():base()
    {
        
    }

    public ConfigSectionException(string sectionName):base($"{sectionName} is not a correct config item.")
    {
        
    }
}