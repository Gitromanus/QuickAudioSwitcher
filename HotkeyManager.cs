using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Manages global hotkey registration using RegisterHotKey API.
/// </summary>
internal class HotkeyManager : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 9000;

    private readonly HotkeyWindow _window;
    private bool _registered;

    public event Action? HotkeyPressed;

    public HotkeyManager()
    {
        _window = new HotkeyWindow();
        _window.HotkeyPressed += () => HotkeyPressed?.Invoke();
    }

    public void Register(Modifiers modifiers, Keys key)
    {
        // Unregister previous
        if (_registered)
        {
            UnregisterHotKey(_window.Handle, HOTKEY_ID);
            _registered = false;
        }

        // Register new
        uint mod = 0;
        if (modifiers.HasFlag(Modifiers.Alt)) mod |= 0x0001;
        if (modifiers.HasFlag(Modifiers.Control)) mod |= 0x0002;
        if (modifiers.HasFlag(Modifiers.Shift)) mod |= 0x0004;
        if (modifiers.HasFlag(Modifiers.Win)) mod |= 0x0008;

        _registered = RegisterHotKey(_window.Handle, HOTKEY_ID, mod, (uint)key);

        if (!_registered)
        {
            int error = Marshal.GetLastWin32Error();
            // Error 1409 = ERROR_HOTKEY_ALREADY_REGISTERED
            if (error != 1409)
            {
                throw new InvalidOperationException(
                    $"Failed to register hotkey (Error: {error}). Try a different key combination.");
            }
        }
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_window.Handle, HOTKEY_ID);
            _registered = false;
        }
        _window.Dispose();
    }

    [Flags]
    public enum Modifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    /// <summary>
    /// Hidden window to receive hotkey messages.
    /// </summary>
    private class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        public event Action? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke();
            }
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}