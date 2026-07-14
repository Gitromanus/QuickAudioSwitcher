using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using NAudio.CoreAudioApi;

namespace QuickAudioSwitcher;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly AudioEndpointManager _endpointManager;
    private readonly HotkeyManager _hotkeyManager;
    private readonly Dictionary<string, Icon> _deviceIcons = new();
    private ContextMenuStrip _menu = null!;
    private List<MMDevice> _activeDevices = new();

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private const string IconRegValue = "{259abffc-50a7-47ce-af08-68c9a7d73366},12";

    public TrayApplicationContext()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        _endpointManager = new AudioEndpointManager();
        _hotkeyManager = new HotkeyManager();

        _trayIcon = new NotifyIcon
        {
            Icon = GetDefaultIcon(),
            Text = "Audio Switcher",
            Visible = true
        };

        _trayIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left) SwitchToNextDevice();
            if (e.Button == MouseButtons.Right) { RebuildMenu(); _menu.Show(Cursor.Position); }
        };

        _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
        _hotkeyManager.Register(Settings.Instance.HotkeyModifiers, Settings.Instance.HotkeyKey);

        RefreshDevices();
    }

    private void OnHotkeyPressed() => SwitchToNextDevice();

    private void SwitchToNextDevice()
    {
        RefreshDevices();
        if (_activeDevices.Count < 2) return;

        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        int startIndex = 0;
        if (defaultDevice != null)
            for (int i = 0; i < _activeDevices.Count; i++)
                if (_activeDevices[i].ID == defaultDevice.ID) { startIndex = i; break; }

        int nextIndex = (startIndex + 1) % _activeDevices.Count;
        var nextDevice = _activeDevices[nextIndex];

        try
        {
            _endpointManager.SetDefaultEndpoint(nextDevice.ID, nextDevice.FriendlyName);
            UpdateTrayIcon(nextDevice);
            UpdateTrayText();
        }
        catch
        {
            SetTrayError(LanguageManager.Instance.GetString("SwitchError"));
        }
    }

    private void RefreshDevices()
    {
        _activeDevices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                                          .OrderBy(d => d.FriendlyName).ToList();
        UpdateTrayIcon(null);
        UpdateTrayText();
    }

    private void UpdateTrayText()
    {
        var lang = LanguageManager.Instance;
        var d = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        _trayIcon.Text = d != null ? $"🔊 {d.FriendlyName}" : lang.GetString("NoDevices");
    }

    private void UpdateTrayIcon(MMDevice? currentDevice)
    {
        if (currentDevice == null)
            currentDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        if (currentDevice != null)
        {
            var icon = GetOrLoadDeviceIcon(currentDevice);
            if (icon != null) _trayIcon.Icon = icon;
        }
    }

    private Icon? GetOrLoadDeviceIcon(MMDevice device)
    {
        if (_deviceIcons.TryGetValue(device.ID, out var cached))
            return cached;

        try
        {
            // Extract device GUID from ID: {0.0.0.00000000}.{GUID}
            var idParts = device.ID.Split('.');
            if (idParts.Length < 2) return null;

            var guidStr = idParts[^1].Trim('{', '}');
            string regPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render\{{{guidStr}}}";

            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key == null) return null;

            using var propsKey = key.OpenSubKey("Properties");
            if (propsKey == null) return null;

            var iconVal = propsKey.GetValue(IconRegValue);
            if (iconVal == null) return null;

            string iconPath = iconVal.ToString() ?? "";
            if (string.IsNullOrEmpty(iconPath)) return null;

            // Expand environment variables
            iconPath = Environment.ExpandEnvironmentVariables(iconPath);

            // Parse "path.dll,index" format
            var parts = iconPath.Split(',');
            if (parts.Length != 2) return null;

            string dllPath = parts[0].TrimStart('@');
            if (!int.TryParse(parts[1], out int idx)) return null;

            if (!System.IO.Path.IsPathRooted(dllPath))
                dllPath = System.IO.Path.Combine(Environment.SystemDirectory, dllPath);

            IntPtr hIcon = ExtractIcon(IntPtr.Zero, dllPath, idx);
            if (hIcon == IntPtr.Zero || hIcon == (IntPtr)1) return null;

            var icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            _deviceIcons[device.ID] = icon;
            return icon;
        }
        catch
        {
            return null;
        }
    }

    private static Icon GetDefaultIcon()
    {
        IntPtr hIcon = ExtractIcon(IntPtr.Zero,
            System.IO.Path.Combine(Environment.SystemDirectory, "mmres.dll"), 0);
        if (hIcon != IntPtr.Zero && hIcon != (IntPtr)1)
        {
            var icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            return icon;
        }
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        return Icon.FromHandle(bmp.GetHicon());
    }

    private void RebuildMenu()
    {
        var lang = LanguageManager.Instance;
        RefreshDevices();
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        _menu?.Dispose();
        _menu = new ContextMenuStrip();
        ApplyMenuTheme(_menu);

        var h = _menu.Items.Add(lang.GetString("Devices")); h.Enabled = false;
        _menu.Items.Add(new ToolStripSeparator());

        foreach (var device in _activeDevices)
        {
            var isDef = defaultDevice != null && device.ID == defaultDevice.ID;
            var name = device.FriendlyName.Length > 50 ? device.FriendlyName[..47] + "..." : device.FriendlyName;
            var item = _menu.Items.Add(isDef ? $"✓ {name}" : $"  {name}");
            if (isDef) item.Font = new Font(_menu.Font, FontStyle.Bold);

            var devIcon = GetOrLoadDeviceIcon(device);
            if (devIcon != null)
                item.Image = devIcon.ToBitmap();

            var did = device.ID; var dnm = device.FriendlyName;
            item.Click += (s, e) =>
            {
                try { _endpointManager.SetDefaultEndpoint(did, dnm); UpdateTrayIcon(device); UpdateTrayText(); }
                catch { SetTrayError(lang.GetString("SwitchError")); }
            };
        }

        _menu.Items.Add(new ToolStripSeparator());
        var si = _menu.Items.Add(lang.GetString("Settings"));
        si.Click += (s, e) => ShowSettings();
        _menu.Items.Add(new ToolStripSeparator());
        var ei = _menu.Items.Add(lang.GetString("Exit"));
        ei.Click += (s, e) =>
        {
            _hotkeyManager.Dispose(); _trayIcon.Visible = false;
            _trayIcon.Dispose();
            foreach (var icon in _deviceIcons.Values) icon.Dispose();
            Application.Exit();
        };
    }

    internal static bool IsAutoStartEnabled()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", false);
        if (key == null) return false;
        var val = key.GetValue("QuickAudioSwitcher");
        return val != null;
    }

    internal static void SetAutoStart(bool enable)
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key == null) return;
        if (enable)
        {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            if (!string.IsNullOrEmpty(exePath))
                key.SetValue("QuickAudioSwitcher", $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue("QuickAudioSwitcher", false);
        }
    }

    private void ShowSettings()
    {
        using var dialog = new SettingsForm();
        if (dialog.ShowDialog() == DialogResult.OK)
            _hotkeyManager.Register(Settings.Instance.HotkeyModifiers, Settings.Instance.HotkeyKey);
    }

    /// <summary>
    /// Sets tray text safely — NotifyIcon.Text has a 128 character limit.
    /// </summary>
    private void SetTrayError(string message)
    {
        try
        {
            if (message.Length > 120)
                message = message[..117] + "...";
            _trayIcon.Text = message;
        }
        catch
        {
            // Ignore — better to have no error than crash
        }
    }

    /// <summary>
    /// Applies dark/light theme to the context menu strip based on Windows theme.
    /// </summary>
    private static void ApplyMenuTheme(ContextMenuStrip menu)
    {
        bool isDark = ThemeManager.IsWindowsDarkMode();
        if (isDark)
        {
            menu.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            menu.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            menu.Renderer = new DarkMenuRenderer();
        }
        else
        {
            menu.BackColor = System.Drawing.SystemColors.Control;
            menu.ForeColor = System.Drawing.SystemColors.ControlText;
            menu.Renderer = new ToolStripProfessionalRenderer();
        }
    }

    /// <summary>
    /// Custom renderer for dark context menu.
    /// </summary>
    private class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColors()) { }
    }

    /// <summary>
    /// Dark color table for context menu.
    /// </summary>
    private class DarkMenuColors : ProfessionalColorTable
    {
        public override System.Drawing.Color MenuItemSelected => System.Drawing.Color.FromArgb(70, 70, 70);
        public override System.Drawing.Color MenuItemSelectedGradientBegin => System.Drawing.Color.FromArgb(70, 70, 70);
        public override System.Drawing.Color MenuItemSelectedGradientEnd => System.Drawing.Color.FromArgb(70, 70, 70);
        public override System.Drawing.Color MenuBorder => System.Drawing.Color.FromArgb(100, 100, 100);
        public override System.Drawing.Color MenuItemBorder => System.Drawing.Color.FromArgb(100, 100, 100);
        public override System.Drawing.Color ToolStripDropDownBackground => System.Drawing.Color.FromArgb(45, 45, 45);
        public override System.Drawing.Color ImageMarginGradientBegin => System.Drawing.Color.FromArgb(45, 45, 45);
        public override System.Drawing.Color ImageMarginGradientMiddle => System.Drawing.Color.FromArgb(45, 45, 45);
        public override System.Drawing.Color ImageMarginGradientEnd => System.Drawing.Color.FromArgb(45, 45, 45);
        public override System.Drawing.Color SeparatorDark => System.Drawing.Color.FromArgb(80, 80, 80);
        public override System.Drawing.Color SeparatorLight => System.Drawing.Color.FromArgb(80, 80, 80);
    }
}