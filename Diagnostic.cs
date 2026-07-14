using System;
using System.IO;
using System.Reflection;

namespace QuickAudioSwitcher;

internal static class Diagnostic
{
    public static void CheckResources()
    {
        Console.WriteLine("=== Diagnostic ===");
        Console.WriteLine($"BaseDir: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine($"TempDir: {Path.Combine(Path.GetTempPath(), "QuickAudioSwitcher")}");
        
        var assembly = Assembly.GetExecutingAssembly();
        Console.WriteLine($"Assembly: {assembly.FullName}");
        
        Console.WriteLine("\nEmbedded resources:");
        foreach (var name in assembly.GetManifestResourceNames())
        {
            Console.WriteLine($"  {name}");
        }
        
        string tempPath = Path.Combine(Path.GetTempPath(), "QuickAudioSwitcher", "SoundVolumeView.exe");
        Console.WriteLine($"\nTemp SVV exe exists: {File.Exists(tempPath)}");
        
        // Try to extract
        string resourceName = "QuickAudioSwitcher.SoundVolumeView.exe";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        Console.WriteLine($"Resource stream: {stream?.GetType()}");
        if (stream != null)
        {
            Console.WriteLine($"Stream length: {stream.Length}");
        }
    }
}