using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Settings dialog for configuring hotkey, language and auto-start.
/// </summary>
internal class SettingsForm : Form
{
    private readonly CheckBox _ctrlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    private readonly ComboBox _langComboBox;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;
    private readonly Label _quickSwitchLabel;
    private readonly GroupBox _hotkeyGroup;
    private readonly Label _langLabel;
    private readonly Button _soundSettingsBtn;
    private readonly CheckBox _autoStartCheckBox;

    public SettingsForm()
    {
        var lang = LanguageManager.Instance;

        Text = lang.GetString("SettingsTitle");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(480, 370);

        var settings = Settings.Instance;
        int leftCol = 12;
        int labelW = 100;
        int rightCol = leftCol + labelW + 6; // 118
        int fieldW = 140;
        int formW = 450;
        int rowH = 28;
        int y = 12;

        // ── Hotkey group (modifiers + key + quick switch) ──
        _hotkeyGroup = new GroupBox
        {
            Text = lang.GetString("Hotkey"),
            Location = new Point(leftCol, y),
            Size = new Size(formW, 150)
        };

        _ctrlCheckBox = new CheckBox { Text = "Ctrl", Location = new Point(15, 25), Size = new Size(60, 24) };
        _altCheckBox = new CheckBox { Text = "Alt", Location = new Point(85, 25), Size = new Size(60, 24) };
        _shiftCheckBox = new CheckBox { Text = "Shift", Location = new Point(155, 25), Size = new Size(60, 24) };
        _winCheckBox = new CheckBox { Text = "Win", Location = new Point(225, 25), Size = new Size(60, 24) };

        _ctrlCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Control);
        _altCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Alt);
        _shiftCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Shift);
        _winCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Win);

        var keyLabel = new Label
        {
            Text = lang.GetString("Key"),
            Location = new Point(15, 55),
            Size = new Size(labelW, 24)
        };

        _keyComboBox = new ComboBox
        {
            Location = new Point(rightCol - leftCol + 15, 55),
            Size = new Size(fieldW, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DrawMode = DrawMode.OwnerDrawFixed,
        };
        _keyComboBox.DrawItem += ComboBox_DrawItem;

        _keyComboBox.Items.AddRange(new object[]
        {
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6,
            Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
            Keys.F13, Keys.F14, Keys.F15, Keys.F16, Keys.F17, Keys.F18,
            Keys.F19, Keys.F20, Keys.F21, Keys.F22, Keys.F23, Keys.F24,
            Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G,
            Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N,
            Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U,
            Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
            Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
            Keys.D6, Keys.D7, Keys.D8, Keys.D9,
            Keys.Oemtilde, Keys.OemMinus, Keys.Oemplus,
            Keys.OemOpenBrackets, Keys.OemCloseBrackets,
            Keys.OemSemicolon, Keys.OemQuotes, Keys.Oemcomma,
            Keys.OemPeriod, Keys.OemQuestion, Keys.OemBackslash,
            Keys.Tab, Keys.Space, Keys.Enter, Keys.Escape,
            Keys.Insert, Keys.Delete, Keys.Home, Keys.End,
            Keys.PageUp, Keys.PageDown,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right,
            Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3,
            Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7,
            Keys.NumPad8, Keys.NumPad9,
            Keys.Multiply, Keys.Add, Keys.Subtract, Keys.Divide,
            Keys.Decimal,
        });

        _keyComboBox.SelectedItem = settings.HotkeyKey;
        if (_keyComboBox.SelectedItem == null)
            _keyComboBox.SelectedIndex = 0;

        // Quick switch preview inside the group box
        _quickSwitchLabel = new Label
        {
            Location = new Point(15, 90),
            Size = new Size(formW - 30, 24)
        };
        UpdateQuickSwitch();

        _hotkeyGroup.Controls.AddRange(new Control[]
        {
            _ctrlCheckBox, _altCheckBox, _shiftCheckBox, _winCheckBox,
            keyLabel, _keyComboBox, _quickSwitchLabel
        });

        y += 160;

        // ── Language ──
        _langLabel = new Label
        {
            Text = lang.GetString("Language"),
            Location = new Point(leftCol, y + 2),
            Size = new Size(labelW, 24)
        };

        _langComboBox = new ComboBox
        {
            Location = new Point(rightCol, y),
            Size = new Size(fieldW, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DrawMode = DrawMode.OwnerDrawFixed,
        };
        _langComboBox.DrawItem += ComboBox_DrawItem;

        foreach (var code in LanguageManager.GetAvailableLanguages())
            _langComboBox.Items.Add(LanguageManager.GetLanguageDisplayName(code));

        int langIndex = 0;
        var codes = LanguageManager.GetAvailableLanguages();
        for (int i = 0; i < codes.Length; i++)
        {
            if (codes[i] == settings.Language)
            {
                langIndex = i;
                break;
            }
        }
        _langComboBox.SelectedIndex = langIndex;

        // Apply language on-the-fly when selection changes
        _langComboBox.SelectedIndexChanged += (s, e) => ApplyLanguage();

        y += rowH + 8;

        // ── Auto-start ──
        _autoStartCheckBox = new CheckBox
        {
            Text = lang.GetString("AutoStartCheckBox"),
            Location = new Point(leftCol, y),
            Size = new Size(200, 24),
            Checked = TrayApplicationContext.IsAutoStartEnabled(),
        };

        y += rowH + 8;

        // ── Windows Sound Settings button ──
        _soundSettingsBtn = new Button
        {
            Text = "🔊 Windows Sound Settings",
            Location = new Point(leftCol, y),
            Size = new Size(formW, 34),
            FlatStyle = FlatStyle.Flat,
        };
        _soundSettingsBtn.Click += (s, e) =>
        {
            try
            {
                Process.Start("ms-settings:sound");
            }
            catch
            {
                try { Process.Start("control", "mmsys.cpl"); }
                catch { }
            }
        };

        y += 44;

        // ── Buttons ──
        _saveButton = new Button
        {
            Text = lang.GetString("Save"),
            Location = new Point(formW - 210, y),
            Size = new Size(100, 28)
        };
        _saveButton.Click += (s, e) => SaveAndClose();

        _cancelButton = new Button
        {
            Text = lang.GetString("Cancel"),
            Location = new Point(formW - 100, y),
            Size = new Size(100, 28)
        };
        _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;

        // Update quick switch preview on any change
        EventHandler updateQuickSwitch = (s, e) => UpdateQuickSwitch();
        _ctrlCheckBox.CheckedChanged += updateQuickSwitch;
        _altCheckBox.CheckedChanged += updateQuickSwitch;
        _shiftCheckBox.CheckedChanged += updateQuickSwitch;
        _winCheckBox.CheckedChanged += updateQuickSwitch;
        _keyComboBox.SelectedIndexChanged += updateQuickSwitch;

        Controls.AddRange(new Control[]
        {
            _hotkeyGroup,
            _langLabel, _langComboBox,
            _autoStartCheckBox,
            _soundSettingsBtn,
            _saveButton, _cancelButton
        });

        // Apply Windows 11 theme (dark/light + Mica)
        Load += (s, e) => ThemeManager.ApplyTheme(this);
    }

    /// <summary>
    /// Applies the selected language to all UI elements in real-time.
    /// </summary>
    private void ApplyLanguage()
    {
        var codes = LanguageManager.GetAvailableLanguages();
        int idx = _langComboBox.SelectedIndex;
        if (idx < 0 || idx >= codes.Length) return;

        string newLang = codes[idx];
        LanguageManager.Instance.SetLanguage(newLang);
        var lang = LanguageManager.Instance;

        Text = lang.GetString("SettingsTitle");
        _hotkeyGroup.Text = lang.GetString("Hotkey");
        _langLabel.Text = lang.GetString("Language");
        _autoStartCheckBox.Text = lang.GetString("AutoStartCheckBox");
        _saveButton.Text = lang.GetString("Save");
        _cancelButton.Text = lang.GetString("Cancel");
        UpdateQuickSwitch();
    }

    /// <summary>
    /// Custom draw for ComboBox items — respects dark/light theme.
    /// </summary>
    private void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var cb = (ComboBox)sender!;
        bool isDark = ThemeManager.IsWindowsDarkMode();

        Color bg, fg;
        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
        {
            bg = isDark ? Color.FromArgb(70, 70, 70) : SystemColors.Highlight;
            fg = isDark ? Color.FromArgb(241, 241, 241) : SystemColors.HighlightText;
        }
        else
        {
            bg = isDark ? Color.FromArgb(45, 45, 45) : SystemColors.Window;
            fg = isDark ? Color.FromArgb(241, 241, 241) : SystemColors.WindowText;
        }

        e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
        var text = cb.Items[e.Index]?.ToString() ?? "";
        TextRenderer.DrawText(e.Graphics, text,
            e.Font, e.Bounds, fg,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        e.DrawFocusRectangle();
    }

    private void UpdateQuickSwitch()
    {
        var lang = LanguageManager.Instance;
        var parts = new System.Collections.Generic.List<string>();
        if (_ctrlCheckBox.Checked) parts.Add("Ctrl");
        if (_altCheckBox.Checked) parts.Add("Alt");
        if (_shiftCheckBox.Checked) parts.Add("Shift");
        if (_winCheckBox.Checked) parts.Add("Win");

        var key = _keyComboBox.SelectedItem?.ToString() ?? "?";
        _quickSwitchLabel.Text = $"{lang.GetString("QuickSwitch")} {string.Join(" + ", parts)} + {key}";
    }

    private void SaveAndClose()
    {
        var lang = LanguageManager.Instance;
        var modifiers = HotkeyManager.Modifiers.None;
        if (_ctrlCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Control;
        if (_altCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Alt;
        if (_shiftCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Shift;
        if (_winCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Win;

        var key = (Keys)(_keyComboBox.SelectedItem ?? Keys.F12);

        if (modifiers == HotkeyManager.Modifiers.None)
        {
            MessageBox.Show(lang.GetString("SelectModifier"),
                lang.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Save auto-start
        TrayApplicationContext.SetAutoStart(_autoStartCheckBox.Checked);

        // Save language
        var codes = LanguageManager.GetAvailableLanguages();
        int langIndex = _langComboBox.SelectedIndex;
        if (langIndex >= 0 && langIndex < codes.Length)
        {
            string newLang = codes[langIndex];
            if (newLang != Settings.Instance.Language)
            {
                Settings.Instance.Language = newLang;
                LanguageManager.Instance.SetLanguage(newLang);
            }
        }

        Settings.Instance.HotkeyModifiers = modifiers;
        Settings.Instance.HotkeyKey = key;
        Settings.Instance.Save();

        DialogResult = DialogResult.OK;
    }
}