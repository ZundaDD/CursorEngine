using CursorEngine.Model;
using CursorEngine.Services;
using CursorEngine.View;
using CursorEngine.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace CursorEngine;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddTransient<RenamePanel>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();    
                services.AddSingleton<CursorControl>();
                services.AddSingleton<RuleControl>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<PathService>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();
        var startupForm = AppHost.Services.GetService<MainWindow>();
        if (startupForm != null) startupForm.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}
