using Serilog;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole;
using System.Configuration;
using System.Data;
using System.Windows;


namespace WPF_Wave;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .Enrich.FromLogContext()
            .CreateLogger();

        Log.Information("アプリケーションが起動しました。");

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("アプリケーションが終了します。");

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
