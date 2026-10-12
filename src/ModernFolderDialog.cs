using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrototypeUltimateTool
{
    public class ModernFolderBrowserDialog : IDisposable
    {
        public string Title { get; set; } = "Выберите папку";
        public string InitialDirectory { get; set; }
        public string SelectedPath { get; private set; }

        public DialogResult ShowDialog(IWin32Window owner = null)
        {
            IntPtr hwnd = owner != null ? owner.Handle : IntPtr.Zero;
            IFileOpenDialog dialog = null;
            try
            {
                dialog = (IFileOpenDialog)new FileOpenDialog();
                dialog.GetOptions(out uint options);
                options |= FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM;
                dialog.SetOptions(options);

                if (!string.IsNullOrEmpty(this.Title))
                    dialog.SetTitle(this.Title);

                if (!string.IsNullOrEmpty(this.InitialDirectory) && Directory.Exists(this.InitialDirectory))
                {
                    SHCreateItemFromParsingName(this.InitialDirectory, IntPtr.Zero, typeof(IShellItem).GUID, out IShellItem folderItem);
                    if (folderItem != null) dialog.SetFolder(folderItem);
                }

                if (dialog.Show(hwnd) != 0) return DialogResult.Cancel;

                dialog.GetResult(out IShellItem item);
                if (item != null)
                {
                    item.GetDisplayName(SIGDN_FILESYSPATH, out IntPtr pszPath);
                    if (pszPath != IntPtr.Zero)
                    {
                        try
                        {
                            this.SelectedPath = Marshal.PtrToStringUni(pszPath);
                            return DialogResult.OK;
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pszPath);
                        }
                    }
                }
            }
            catch
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = this.Title;
                    if (!string.IsNullOrEmpty(this.InitialDirectory)) fbd.SelectedPath = this.InitialDirectory;
                    if (fbd.ShowDialog(owner) == DialogResult.OK)
                    {
                        this.SelectedPath = fbd.SelectedPath;
                        return DialogResult.OK;
                    }
                }
            }
            finally
            {
                if (dialog != null) Marshal.ReleaseComObject(dialog);
            }

            return DialogResult.Cancel;
        }

        public void Dispose() { }

        private const uint FOS_PICKFOLDERS = 0x00000020;
        private const uint FOS_FORCEFILESYSTEM = 0x00000040;
        private const uint SIGDN_FILESYSPATH = 0x80058000;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        [ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialog { }

        [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(uint fos);
            void GetOptions(out uint pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdap);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
            void GetResults(out IntPtr ppenum);
            void GetSelectedItems(out IntPtr ppsai);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(uint sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }
    }
}