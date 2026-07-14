using System;
using System.Diagnostics;
using System.IO;

namespace QuickAudioSwitcher;

/// <summary>
/// Sets default audio endpoint using NirCmd utility.
/// NirCmd is a small (52 KB) freeware command-line utility from NirSoft.
/// It is bundled with the application and extracted on first run.
/// </summary>
internal class AudioEndpointManager
{
    private readonly string _nircmdPath;

    public AudioEndpointManager()
    {
        // Look for nircmd.exe next to the application executable first,
        // then in the nircmd subfolder, then extract from embedded resources
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] searchPaths =
        {
            Path.Combine(baseDir, "nircmd.exe"),
            Path.Combine(baseDir, "nircmd", "nircmd.exe"),
        };

        _nircmdPath = "";
        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                _nircmdPath = path;
                break;
            }
        }

        // If not found, extract from embedded resources
        if (string.IsNullOrEmpty(_nircmdPath))
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "QuickAudioSwitcher");
            Directory.CreateDirectory(tempDir);
            _nircmdPath = Path.Combine(tempDir, "nircmd.exe");

            if (!File.Exists(_nircmdPath))
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = "QuickAudioSwitcher.nircmd.nircmd.exe";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var fileStream = File.Create(_nircmdPath);
                    stream.CopyTo(fileStream);
                }
                else
                {
                    throw new InvalidOperationException(
                        "nircmd.exe not found. Please place nircmd.exe next to the application.");
                }
            }
        }
    }

    /// <summary>
    /// Gets the display name of a device as recognized by NirCmd.
    /// NirCmd uses the short name (without the parenthesized part).
    /// </summary>
    public static string GetNircmdName(string naudioName)
    {
        // NirCmd strips the part in parentheses
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
        string nircmdName = GetNircmdName(deviceName);

        var psi = new ProcessStartInfo
        {
            FileName = _nircmdPath,
            Arguments = $"setdefaultsounddevice \"{nircmdName}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start nircmd.exe");

        process.WaitForExit(5000);

        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException(
                $"nircmd error (exit={process.ExitCode}): {error}");
        }
    }
}