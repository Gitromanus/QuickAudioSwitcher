using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

static class Program
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [STAThread]
    static void Main()
    {
        // Hide console window
        var handle = GetConsoleWindow();
        ShowWindow(handle, 0);

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}