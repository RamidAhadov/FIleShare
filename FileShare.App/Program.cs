using Autofac;
using FileShare.App.DependencyResolvers.Autofac;

namespace FileShare.App;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        var scope = InitializeScope();
        var mainForm = scope.Resolve<Application>();
        System.Windows.Forms.Application.Run(mainForm);
    }

    private static ILifetimeScope InitializeScope()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new AutofacBusinessModule());
        var container = builder.Build();
        return container.BeginLifetimeScope();
    }
}