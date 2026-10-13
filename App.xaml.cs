using System;
using System.IO;
using System.Windows;

namespace Ketch
{
    public partial class App : System.Windows.Application
    {
        private static readonly string LogFilePath = GetLogFilePath();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handlers
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                LogException(e.Exception, "DispatcherUnhandledException");
                MessageBox.Show($"ketch has encountered an error and needs to close. A log file was written to:\n{LogFilePath}", "ketch - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            finally
            {
                // Make sure we exit
                Environment.Exit(1);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                LogException(ex, "CurrentDomain_UnhandledException");
            }
            catch { }
        }

        private static void TaskScheduler_UnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                LogException(e.Exception, "UnobservedTaskException");
                e.SetObserved();
            }
            catch { }
        }

        private static void LogException(Exception? ex, string source)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("--- ketch Exception Log ---");
                sb.AppendLine(DateTime.UtcNow.ToString("o"));
                sb.AppendLine("Source: " + source);
                if (ex != null)
                {
                    sb.AppendLine(ex.ToString());
                }
                else
                {
                    sb.AppendLine("(no exception object)");
                }
                sb.AppendLine();

                var path = LogFilePath;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(path, sb.ToString());
            }
            catch { }
        }

        private static string GetLogFilePath()
        {
            // Try next to EXE first, fallback to LocalAppData
            try
            {
                var exeDir = AppContext.BaseDirectory;
                var p = Path.Combine(exeDir, "ketch_error.log");
                // Test write
                File.AppendAllText(p, "");
                return p;
            }
            catch
            {
                try
                {
                    var local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ketch");
                    if (!Directory.Exists(local)) Directory.CreateDirectory(local);
                    return Path.Combine(local, "ketch_error.log");
                }
                catch
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ketch_error.log");
                }
            }
        }
    }
}
