using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PrototypeUltimateTool
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ApplicationLog.BeginSession();
            ApplicationLog.Write("STARTUP", "Process started; exe=" + Application.ExecutablePath +
                "; cwd=" + Environment.CurrentDirectory + "; commandLine=" + Environment.CommandLine);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                ApplicationLog.Write("UNHANDLED_UI_EXCEPTION", "Unhandled WinForms UI exception.", e.Exception);
                MessageBox.Show(e.Exception.ToString(), "Ошибка потока", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception exception = e.ExceptionObject as Exception;
                ApplicationLog.Write("UNHANDLED_DOMAIN_EXCEPTION", "Unhandled AppDomain exception; terminating=" + e.IsTerminating,
                    exception ?? new Exception(Convert.ToString(e.ExceptionObject)));
                MessageBox.Show(Convert.ToString(e.ExceptionObject), "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            try
            {
                // Windows Explorer launches registered applications with the selected document path
                // as the first command-line argument. Keep the parsed CLR command line canonical.
                string[] commandLineArgs = Environment.GetCommandLineArgs() ?? new string[0];
                string[] effectiveArgs = commandLineArgs.Length > 1
                    ? commandLineArgs.Skip(1).ToArray()
                    : (args ?? new string[0]);

                string startupFile = ResolveStartupFile(effectiveArgs);
                LogStartup(effectiveArgs, startupFile, null);
                if (effectiveArgs.Length > 0 && string.IsNullOrEmpty(startupFile))
                {
                    MessageBox.Show(
                        "Программа получила параметры запуска, но не смогла найти среди них существующий файл.\n\n" +
                        string.Join("\n", effectiveArgs) + "\n\nПодробности записаны в " + ApplicationLog.CurrentLogPath,
                        "Не удалось определить открываемый файл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Application.Run(new MainForm(startupFile));
                ApplicationLog.Write("SHUTDOWN", "Application main loop exited normally.");
            }
            catch (Exception ex)
            {
                LogStartup(args, null, ex);
                MessageBox.Show("Ошибка при запуске программы:\n\n" + ex.Message + "\n\nДетали:\n" + ex.ToString(), "Сбой запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void LogStartup(string[] args, string startupFile, Exception exception)
        {
            string renderedArgs = args == null || args.Length == 0 ? "<no arguments>" : string.Join(" | ", args);
            ApplicationLog.Write(exception == null ? "STARTUP_ARGUMENTS" : "STARTUP_FAILED",
                "CommandLine=" + Environment.CommandLine + "; Args=" + renderedArgs +
                "; ResolvedFile=" + (startupFile ?? "<none>"), exception);
        }

        private static string ResolveStartupFile(string[] args)
        {
            if (args == null || args.Length == 0) return null;

            foreach (string argument in args)
            {
                if (string.IsNullOrWhiteSpace(argument)) continue;
                string candidate = Environment.ExpandEnvironmentVariables(argument.Trim().Trim('"'));
                try
                {
                    Uri fileUri;
                    if (Uri.TryCreate(candidate, UriKind.Absolute, out fileUri) && fileUri.IsFile)
                        candidate = fileUri.LocalPath;
                    candidate = Path.GetFullPath(candidate);
                    if (File.Exists(candidate)) return candidate;
                }
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
                catch (PathTooLongException) { }
            }

            return null;
        }
    }
}
