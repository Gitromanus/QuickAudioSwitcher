using System;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Settings dialog for configuring hotkey.
/// </summary>
internal class SettingsForm : Form
{
    private readonly CheckBox _ctrlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;
    private readonly Label _previewLabel;

    public SettingsForm()
    {
        Text = "⚙ Настройки Audio Switcher";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new System.Drawing.Size(350, 250);

        var settings = Settings.Instance;

        // Modifiers group
        var modGroup = new GroupBox
        {
            Text = "Модификаторы",
            Location = new System.Drawing.Point(12, 12),
            Size = new System.Drawing.Size(320, 80)
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
            Text = "Клавиша:",
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

        // Preview
        _previewLabel = new Label
        {
            Text = "Предпросмотр:",
            Location = new System.Drawing.Point(12, 140),
            Size = new System.Drawing.Size(320, 24)
        };
        UpdatePreview();

        // Buttons
        _saveButton = new Button
        {
            Text = "Сохранить",
            Location = new System.Drawing.Point(160, 180),
            Size = new System.Drawing.Size(80, 28)
        };
        _saveButton.Click += (s, e) => SaveAndClose();

        _cancelButton = new Button
        {
            Text = "Отмена",
            Location = new System.Drawing.Point(250, 180),
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
            _previewLabel, _saveButton, _cancelButton
        });
    }

    private void UpdatePreview()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (_ctrlCheckBox.Checked) parts.Add("Ctrl");
        if (_altCheckBox.Checked) parts.Add("Alt");
        if (_shiftCheckBox.Checked) parts.Add("Shift");
        if (_winCheckBox.Checked) parts.Add("Win");

        var key = _keyComboBox.SelectedItem?.ToString() ?? "?";
        _previewLabel.Text = $"Предпросмотр: {string.Join(" + ", parts)} + {key}";
    }

    private void SaveAndClose()
    {
        var modifiers = HotkeyManager.Modifiers.None;
        if (_ctrlCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Control;
        if (_altCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Alt;
        if (_shiftCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Shift;
        if (_winCheckBox.Checked) modifiers |= HotkeyManager.Modifiers.Win;

        var key = (Keys)(_keyComboBox.SelectedItem ?? Keys.F12);

        // Validate
        if (modifiers == HotkeyManager.Modifiers.None)
        {
            MessageBox.Show("Выберите хотя бы один модификатор (Ctrl, Alt, Shift или Win).",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Settings.Instance.HotkeyModifiers = modifiers;
        Settings.Instance.HotkeyKey = key;
        Settings.Instance.Save();

        DialogResult = DialogResult.OK;
    }
}