using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Applies Windows 11 dark/light theme to WinForms forms.
/// Uses DwmSetWindowAttribute for title bar and manual colors for controls.
/// </summary>
internal static class ThemeManager
{
    // DwmSetWindowAttribute attributes
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_MICA_EFFECT = 1029;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    /// <summary>
    /// Applies dark/light theme to a form based on the current Windows theme setting.
    /// </summary>
    public static void ApplyTheme(Form form)
    {
        try
        {
            bool isDark = IsWindowsDarkMode();
            IntPtr hwnd = form.Handle;

            // Set dark mode for title bar
            int darkMode = isDark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

            // Enable Mica (Windows 11 acrylic effect) — only on Win11
            if (Environment.OSVersion.Version.Build >= 22000)
            {
                int mica = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref mica, sizeof(int));
            }

            // Apply colors to controls
            ApplyControlColors(form, isDark);
        }
        catch
        {
            // Ignore theme errors on older Windows
        }
    }

    private static void ApplyControlColors(Control parent, bool isDark)
    {
        if (isDark)
        {
            parent.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            parent.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
        }
        else
        {
            parent.BackColor = System.Drawing.Color.FromArgb(243, 243, 243);
            parent.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        }

        foreach (Control ctrl in parent.Controls)
        {
            if (ctrl is Button btn)
            {
                if (isDark)
                {
                    btn.BackColor = System.Drawing.Color.FromArgb(62, 62, 62);
                    btn.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(102, 102, 102);
                }
                else
                {
                    btn.BackColor = System.Drawing.SystemColors.Control;
                    btn.ForeColor = System.Drawing.Color.Black;
                    btn.FlatStyle = FlatStyle.Standard;
                }
            }
            else if (ctrl is GroupBox gb)
            {
                if (isDark)
                {
                    gb.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
                }
                else
                {
                    gb.ForeColor = System.Drawing.Color.Black;
                }
                ApplyControlColors(gb, isDark); // recurse into group box
            }
            else if (ctrl is ComboBox cb)
            {
                if (isDark)
                {
                    cb.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
                    cb.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
                }
                else
                {
                    cb.BackColor = System.Drawing.Color.White;
                    cb.ForeColor = System.Drawing.Color.Black;
                }
            }
            else if (ctrl is CheckBox chk)
            {
                if (isDark)
                {
                    chk.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
                }
                else
                {
                    chk.ForeColor = System.Drawing.Color.Black;
                }
            }
            else if (ctrl is Label lbl)
            {
                if (isDark)
                {
                    lbl.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
                }
                else
                {
                    lbl.ForeColor = System.Drawing.Color.Black;
                }
            }
        }
    }

    /// <summary>
    /// Checks if Windows is in dark mode via registry.
    /// </summary>
    public static bool IsWindowsDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var val = key.GetValue("AppsUseLightTheme");
                if (val is int intVal)
                    return intVal == 0; // 0 = dark, 1 = light
            }
        }
        catch
        {
            // Ignore
        }
        return false;
    }
}