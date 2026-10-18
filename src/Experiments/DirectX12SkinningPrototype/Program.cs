using System;
using System.Windows.Forms;

namespace PrototypeUltimateTool.DirectX12SkinningPrototype;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SkinningPrototypeForm());
    }
}
