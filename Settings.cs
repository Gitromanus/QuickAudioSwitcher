using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace QuickAudioSwitcher;

/// <summary>
/// Application settings with JSON serialization.
/// </summary>
internal class Settings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuickAudioSwitcher",
        "settings.json");

    private static Settings? _instance;

    public static Settings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    public HotkeyManager.Modifiers HotkeyModifiers { get; set; } =
        HotkeyManager.Modifiers.Control | HotkeyManager.Modifiers.Alt;

    public Keys HotkeyKey { get; set; } = Keys.F12;

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    private static Settings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                if (settings != null)
                    return settings;
            }
        }
        catch
        {
            // Ignore load errors, use defaults
        }

        return new Settings();
    }
}