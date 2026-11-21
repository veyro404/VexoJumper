using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using VexoJumper.Views;

namespace VexoJumper;

public partial class App : Application
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VexoJumper", "logs");

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
            Log("Application started successfully.");
        }
        catch (Exception ex)
        {
            LogException("Startup failure", ex);
            MessageBox.Show(
                "Vexo Jumper نتوانست اجرا شود. گزارش خطا در مسیر زیر ذخیره شد:\n" +
                GetLogPath() + "\n\n" + ex.Message,
                "خطا در اجرای Vexo Jumper",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException("Unhandled UI exception", e.Exception);
        MessageBox.Show(
            "یک خطای اجرایی رخ داد. جزئیات در گزارش خطا ذخیره شد:\n" + GetLogPath() +
            "\n\n" + e.Exception.Message,
            "خطای Vexo Jumper",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogException("Unhandled application-domain exception", ex);
        else
            Log("Unhandled application-domain exception: " + e.ExceptionObject);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException("Unobserved task exception", e.Exception);
        e.SetObserved();
    }

    public static void LogException(string context, Exception exception)
    {
        Log(context + Environment.NewLine + exception);
    }

    public static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(GetLogPath(),
                $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}{Environment.NewLine}",
                new UTF8Encoding(false));
        }
        catch
        {
            // Logging must never prevent the application from starting or closing.
        }
    }

    private static string GetLogPath() => Path.Combine(LogDirectory, "startup.log");
}
