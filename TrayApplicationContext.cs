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
        catch (Exception ex) { _trayIcon.Text = $"❌ {ex.Message}"; }
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
        var d = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        _trayIcon.Text = d != null ? $"🔊 {d.FriendlyName}" : "🔊 Нет активных устройств";
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
        RefreshDevices();
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        _menu?.Dispose();
        _menu = new ContextMenuStrip();

        var h = _menu.Items.Add("🔊 Устройства вывода"); h.Enabled = false;
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
                catch (Exception ex) { _trayIcon.Text = $"❌ {ex.Message}"; }
            };
        }

        _menu.Items.Add(new ToolStripSeparator());
        var si = _menu.Items.Add("⚙ Настройки...");
        si.Click += (s, e) => ShowSettings();
        _menu.Items.Add(new ToolStripSeparator());
        var ei = _menu.Items.Add("✕ Выход");
        ei.Click += (s, e) =>
        {
            _hotkeyManager.Dispose(); _trayIcon.Visible = false;
            _trayIcon.Dispose();
            foreach (var icon in _deviceIcons.Values) icon.Dispose();
            Application.Exit();
        };
    }

    private void ShowSettings()
    {
        using var dialog = new SettingsForm();
        if (dialog.ShowDialog() == DialogResult.OK)
            _hotkeyManager.Register(Settings.Instance.HotkeyModifiers, Settings.Instance.HotkeyKey);
    }
}