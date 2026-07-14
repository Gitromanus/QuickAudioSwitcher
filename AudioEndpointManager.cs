using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace QuickAudioSwitcher;

/// <summary>
/// Sets default audio endpoint using SoundVolumeView (NirSoft utility).
/// SoundVolumeView.exe is embedded as a resource for single-file deployment.
/// </summary>
internal class AudioEndpointManager
{
    private readonly string _soundVolumeViewPath;

    public AudioEndpointManager()
    {
        // Extract SoundVolumeView.exe from embedded resources to temp folder
        string tempDir = Path.Combine(Path.GetTempPath(), "QuickAudioSwitcher");
        Directory.CreateDirectory(tempDir);
        _soundVolumeViewPath = Path.Combine(tempDir, "SoundVolumeView.exe");

        if (!File.Exists(_soundVolumeViewPath))
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "QuickAudioSwitcher.SoundVolumeView.exe";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var fileStream = File.Create(_soundVolumeViewPath);
                stream.CopyTo(fileStream);
            }
            else
            {
                // Fallback: look next to the executable
                _soundVolumeViewPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SoundVolumeView.exe");
            }
        }
    }

    /// <summary>
    /// Gets the display name of a device as recognized by SoundVolumeView.
    /// SoundVolumeView uses a shorter name than NAudio.
    /// </summary>
    public string GetSoundVolumeViewName(string naudioName)
    {
        // SoundVolumeView strips the part in parentheses
        // e.g., "Динамики (High Definition Audio Device)" -> "Динамики"
        // e.g., "PHILIPS FTV (NVIDIA High Definition Audio)" -> "PHILIPS FTV"
        int parenIndex = naudioName.IndexOf(" (");
        if (parenIndex > 0)
        {
            return naudioName[..parenIndex];
        }
        return naudioName;
    }

    public void SetDefaultEndpoint(string deviceId, string deviceName)
    {
        // Get the short name for SoundVolumeView
        string svvName = GetSoundVolumeViewName(deviceName);

        // Try SoundVolumeView
        try
        {
            SetDefaultViaSoundVolumeView(svvName);
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SoundVolumeView failed: {ex.Message}");
        }

        throw new InvalidOperationException(
            $"Could not set default audio endpoint to: {deviceName}");
    }

    private void SetDefaultViaSoundVolumeView(string deviceName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _soundVolumeViewPath,
            Arguments = $"/SetDefault \"{deviceName}\" all",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start SoundVolumeView");

        process.WaitForExit(10000);

        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException(
                $"SoundVolumeView error (exit={process.ExitCode}): {error}");
        }
    }
}