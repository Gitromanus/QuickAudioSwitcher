using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Settings dialog for configuring hotkey and language.
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
    private readonly Label _previewLabel;

    public SettingsForm()
    {
        var lang = LanguageManager.Instance;

        Text = lang.GetString("SettingsTitle");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new System.Drawing.Size(420, 370);

        var settings = Settings.Instance;

        // Modifiers group
        var modGroup = new GroupBox
        {
            Text = lang.GetString("Modifiers"),
            Location = new System.Drawing.Point(12, 12),
            Size = new System.Drawing.Size(390, 80)
        };

        _ctrlCheckBox = new CheckBox { Text = "Ctrl", Location = new System.Drawing.Point(15, 25), Size = new System.Drawing.Size(60, 24) };
        _altCheckBox = new CheckBox { Text = "Alt", Location = new System.Drawing.Point(85, 25), Size = new System.Drawing.Size(60, 24) };
        _shiftCheckBox = new CheckBox { Text = "Shift", Location = new System.Drawing.Point(155, 25), Size = new System.Drawing.Size(60, 24) };
        _winCheckBox = new CheckBox { Text = "Win", Location = new System.Drawing.Point(225, 25), Size = new System.Drawing.Size(60, 24) };

        _ctrlCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Control);
        _altCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Alt);
        _shiftCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Shift);
        _winCheckBox.Checked = settings.HotkeyModifiers.HasFlag(HotkeyManager.Modifiers.Win);

        modGroup.Controls.AddRange(new Control[] { _ctrlCheckBox, _altCheckBox, _shiftCheckBox, _winCheckBox });

        // Key combo
        var keyLabel = new Label
        {
            Text = lang.GetString("Key"),
            Location = new System.Drawing.Point(12, 105),
            Size = new System.Drawing.Size(60, 24)
        };

        _keyComboBox = new ComboBox
        {
            Location = new System.Drawing.Point(75, 103),
            Size = new System.Drawing.Size(120, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Add common keys
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

        // Language
        var langLabel = new Label
        {
            Text = lang.GetString("Language"),
            Location = new System.Drawing.Point(12, 140),
            Size = new System.Drawing.Size(60, 24)
        };

        _langComboBox = new ComboBox
        {
            Location = new System.Drawing.Point(75, 138),
            Size = new System.Drawing.Size(200, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        foreach (var code in LanguageManager.GetAvailableLanguages())
            _langComboBox.Items.Add(LanguageManager.GetLanguageDisplayName(code));

        // Find index of current language
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

        // Preview
        _previewLabel = new Label
        {
            Text = lang.GetString("Preview") + ":",
            Location = new System.Drawing.Point(12, 175),
            Size = new System.Drawing.Size(390, 24)
        };
        UpdatePreview();

        // Windows Sound Settings button
        var soundSettingsBtn = new Button
        {
            Text = "🔊 Windows Sound Settings",
            Location = new System.Drawing.Point(12, 210),
            Size = new System.Drawing.Size(390, 32),
            FlatStyle = FlatStyle.Flat,
        };
        soundSettingsBtn.Click += (s, e) =>
        {
            try
            {
                Process.Start("ms-settings:sound");
            }
            catch
            {
                // Fallback
                try { Process.Start("control", "mmsys.cpl"); }
                catch { }
            }
        };

        // Buttons
        _saveButton = new Button
        {
            Text = lang.GetString("Save"),
            Location = new System.Drawing.Point(230, 290),
            Size = new System.Drawing.Size(80, 28)
        };
        _saveButton.Click += (s, e) => SaveAndClose();

        _cancelButton = new Button
        {
            Text = lang.GetString("Cancel"),
            Location = new System.Drawing.Point(320, 290),
            Size = new System.Drawing.Size(80, 28)
        };
        _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;

        // Update preview on any change
        EventHandler updatePreview = (s, e) => UpdatePreview();
        _ctrlCheckBox.CheckedChanged += updatePreview;
        _altCheckBox.CheckedChanged += updatePreview;
        _shiftCheckBox.CheckedChanged += updatePreview;
        _winCheckBox.CheckedChanged += updatePreview;
        _keyComboBox.SelectedIndexChanged += updatePreview;

        Controls.AddRange(new Control[]
        {
            modGroup, keyLabel, _keyComboBox,
            langLabel, _langComboBox,
            _previewLabel, soundSettingsBtn,
            _saveButton, _cancelButton
        });

        // Apply Windows 11 theme (dark/light + Mica)
        Load += (s, e) => ThemeManager.ApplyTheme(this);
    }

    private void UpdatePreview()
    {
        var lang = LanguageManager.Instance;
        var parts = new System.Collections.Generic.List<string>();
        if (_ctrlCheckBox.Checked) parts.Add("Ctrl");
        if (_altCheckBox.Checked) parts.Add("Alt");
        if (_shiftCheckBox.Checked) parts.Add("Shift");
        if (_winCheckBox.Checked) parts.Add("Win");

        var key = _keyComboBox.SelectedItem?.ToString() ?? "?";
        _previewLabel.Text = $"{lang.GetString("Preview")}: {string.Join(" + ", parts)} + {key}";
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

        // Validate
        if (modifiers == HotkeyManager.Modifiers.None)
        {
            MessageBox.Show(lang.GetString("SelectModifier"),
                lang.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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