using System;
using System.Windows.Forms;

namespace PrototypeUltimateTool
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            EmbeddedAssemblyResolver.Register();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => MessageBox.Show(e.Exception.ToString(), "Ошибка потока", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => MessageBox.Show(e.ExceptionObject.ToString(), "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске программы:\n\n" + ex.Message + "\n\nДетали:\n" + ex.ToString(), "Сбой запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}