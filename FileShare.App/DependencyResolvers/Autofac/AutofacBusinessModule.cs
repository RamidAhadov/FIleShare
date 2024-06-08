using Autofac;
using FileShare.App.Services.Abstraction;
using FileShare.App.Services.Concrete;
using FileShare.Business.Abstraction;
using FileShare.Business.Concrete;
using FileShare.Configuration.Abstraction;
using FileShare.Configuration.Concrete;
using Microsoft.Extensions.Configuration;

namespace FileShare.App.DependencyResolvers.Autofac;

public class AutofacBusinessModule:Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //Managers
        builder.RegisterType<SftpConnectionManager>().As<IConnectionManager>();
        builder.RegisterType<DeviceManager>().As<IDeviceManager>();
        builder.RegisterType<TcpNotifyManager>().As<INotifyManager>();
        builder.RegisterType<ConfigFactory>().As<IConfigFactory>();
        builder.RegisterType<EngineService>().As<IEngineService>();
        builder.RegisterType<DeviceService>().As<IDeviceService>();
        builder.RegisterType<FileService>().As<IFileService>();
        builder.RegisterType<Application>();
        
        //Modules
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("C:\\Users\\ASUS\\RiderProjects\\FileShare\\FileShare.App\\appsettings.json")
            .Build();
        builder.RegisterInstance(configurationBuilder).As<IConfigurationRoot>().SingleInstance();
    }
}