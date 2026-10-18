using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Forms;

namespace PrototypeUltimateTool
{
    /// <summary>
    /// Registers current-user Explorer verbs and Open With metadata. It deliberately does not
    /// modify FileExts\UserChoice: Windows protects that selection and users choose the default app.
    /// </summary>
    internal static class ExplorerIntegration
    {
        private const string ApplicationName = "Prototype Ultimate Tool";
        private const string ProgId = "PrototypeUltimateTool.Document";
        private const string VerbName = "OpenWithPrototypeUltimateTool";
        private const string CapabilitiesPath = @"Software\PrototypeUltimateTool\Capabilities";
        private static readonly string[] Extensions = { ".rcf", ".p3d", ".rz" };

        private const uint ShcneAssocChanged = 0x08000000;
        private const uint ShcnfIdList = 0x0000;

        [DllImport("shell32.dll", EntryPoint = "SHChangeNotify")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static bool IsInstalled()
        {
            string exePath = GetExePath();
            string command = BuildOpenCommand(exePath);
            foreach (string extension in Extensions)
            {
                string verbPath = @"Software\Classes\SystemFileAssociations\" + extension + @"\shell\" + VerbName + @"\command";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(verbPath, false))
                {
                    if (key == null || !string.Equals(Convert.ToString(key.GetValue(null)), command, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }
            return true;
        }

        public static void Install()
        {
            string exePath = GetExePath();
            if (!File.Exists(exePath))
                throw new FileNotFoundException("Не найден исполняемый файл Prototype Ultimate Tool.", exePath);

            string exeName = Path.GetFileName(exePath);
            string openCommand = BuildOpenCommand(exePath);
            string icon = "\"" + exePath + "\",0";

            // Register the application under "Open with" without taking over file defaults.
            string applicationKeyPath = @"Software\Classes\Applications\" + exeName;
            using (RegistryKey appKey = CreateKey(applicationKeyPath))
            {
                appKey.SetValue("FriendlyAppName", ApplicationName, RegistryValueKind.String);
                appKey.SetValue("ApplicationCompany", "hopelessnes5", RegistryValueKind.String);
                using (RegistryKey commandKey = appKey.CreateSubKey(@"shell\open\command"))
                    commandKey.SetValue(null, openCommand, RegistryValueKind.String);
                using (RegistryKey iconKey = appKey.CreateSubKey("DefaultIcon"))
                    iconKey.SetValue(null, icon, RegistryValueKind.String);
                using (RegistryKey typesKey = appKey.CreateSubKey("SupportedTypes"))
                {
                    foreach (string extension in Extensions)
                        typesKey.SetValue(extension, string.Empty, RegistryValueKind.String);
                }
            }

            using (RegistryKey progKey = CreateKey(@"Software\Classes\" + ProgId))
                progKey.SetValue(null, ApplicationName + " document", RegistryValueKind.String);
            SetDefaultValue(@"Software\Classes\" + ProgId + @"\shell\open\command", openCommand);
            SetDefaultValue(@"Software\Classes\" + ProgId + @"\DefaultIcon", icon);

            foreach (string extension in Extensions)
            {
                // Open With registration and a dedicated right-click action for each supported type.
                using (RegistryKey openWithKey = CreateKey(@"Software\Classes\" + extension + @"\OpenWithProgids"))
                    openWithKey.SetValue(ProgId, string.Empty, RegistryValueKind.String);

                CreateContextVerb(@"Software\Classes\SystemFileAssociations\" + extension + @"\shell\" + VerbName,
                    openCommand, icon);
            }

            // Publish app capabilities in Windows' Default Apps list; UserChoice remains Windows-managed.
            using (RegistryKey capabilities = CreateKey(CapabilitiesPath))
            {
                capabilities.SetValue("ApplicationName", ApplicationName, RegistryValueKind.String);
                capabilities.SetValue("ApplicationDescription", "Открытие файлов Prototype: .p3d, .p3d.rz и .rcf", RegistryValueKind.String);
            }
            using (RegistryKey associations = CreateKey(CapabilitiesPath + @"\FileAssociations"))
            {
                foreach (string extension in Extensions)
                    associations.SetValue(extension, ProgId, RegistryValueKind.String);
            }
            using (RegistryKey registeredApps = CreateKey(@"Software\RegisteredApplications"))
                registeredApps.SetValue(ApplicationName, CapabilitiesPath, RegistryValueKind.String);

            NotifyExplorer();
        }

        public static void Remove()
        {
            string exeName = Path.GetFileName(GetExePath());
            foreach (string extension in Extensions)
            {
                DeleteSubKeyTreeIfPresent(@"Software\Classes\SystemFileAssociations\" + extension + @"\shell\" + VerbName);
                string openWithPath = @"Software\Classes\" + extension + @"\OpenWithProgids";
                using (RegistryKey openWithKey = Registry.CurrentUser.OpenSubKey(openWithPath, true))
                {
                    if (openWithKey != null)
                    {
                        openWithKey.DeleteValue(ProgId, false);
                        if (openWithKey.ValueCount == 0 && openWithKey.SubKeyCount == 0)
                            DeleteSubKeyTreeIfPresent(openWithPath);
                    }
                }
            }

            DeleteSubKeyTreeIfPresent(@"Software\Classes\Applications\" + exeName);
            DeleteSubKeyTreeIfPresent(@"Software\Classes\" + ProgId);
            DeleteSubKeyTreeIfPresent(CapabilitiesPath);
            using (RegistryKey registeredApps = Registry.CurrentUser.OpenSubKey(@"Software\RegisteredApplications", true))
            {
                if (registeredApps != null) registeredApps.DeleteValue(ApplicationName, false);
            }

            NotifyExplorer();
        }

        private static string GetExePath()
        {
            return Path.GetFullPath(Application.ExecutablePath);
        }

        private static string BuildOpenCommand(string exePath)
        {
            return "\"" + exePath + "\" \"%1\"";
        }

        private static RegistryKey CreateKey(string path)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(path);
            if (key == null) throw new InvalidOperationException("Не удалось создать раздел реестра: HKCU\\" + path);
            return key;
        }

        private static void SetDefaultValue(string path, string value)
        {
            using (RegistryKey key = CreateKey(path))
                key.SetValue(null, value, RegistryValueKind.String);
        }

        private static void CreateContextVerb(string verbPath, string command, string icon)
        {
            using (RegistryKey verbKey = CreateKey(verbPath))
            {
                verbKey.SetValue(null, "Открыть в Prototype Ultimate Tool", RegistryValueKind.String);
                verbKey.SetValue("MUIVerb", "Открыть в Prototype Ultimate Tool", RegistryValueKind.String);
                verbKey.SetValue("Icon", icon, RegistryValueKind.String);
                using (RegistryKey commandKey = verbKey.CreateSubKey("command"))
                    commandKey.SetValue(null, command, RegistryValueKind.String);
            }
        }

        private static void DeleteSubKeyTreeIfPresent(string path)
        {
            string parentPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parentPath)) return;
            using (RegistryKey parent = Registry.CurrentUser.OpenSubKey(parentPath.Replace('/', '\\'), true))
            {
                string leaf = Path.GetFileName(path);
                if (parent == null) return;
                using (RegistryKey existing = parent.OpenSubKey(leaf, false))
                {
                    if (existing != null) parent.DeleteSubKeyTree(leaf, false);
                }
            }
        }

        private static void NotifyExplorer()
        {
            try { SHChangeNotify(ShcneAssocChanged, ShcnfIdList, IntPtr.Zero, IntPtr.Zero); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }
    }
}
