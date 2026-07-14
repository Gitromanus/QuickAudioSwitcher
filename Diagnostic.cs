using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace QuickAudioSwitcher;

static class Diagnostic
{
    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(
        ref Guid clsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        ref Guid riid,
        out IntPtr ppv);

    private const uint CLSCTX_INPROC_SERVER = 1;

    public static void Run()
    {
        Console.WriteLine("=== Audio Devices Diagnostic ===\n");

        var enumerator = new MMDeviceEnumerator();

        Console.WriteLine("Active render devices:\n");
        var activeDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        foreach (var device in activeDevices)
        {
            Console.WriteLine($"  [{device.State}] {device.FriendlyName}");
            Console.WriteLine($"    ID: {device.ID}");
        }

        var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        Console.WriteLine($"\nDefault device: {defaultDevice?.FriendlyName ?? "N/A"}");

        Console.WriteLine("\n=== Testing COM CLSIDs ===\n");

        var clsidsToTest = new (string name, Guid clsid, Guid iid)[]
        {
            ("CPolicyConfig (Win7/8)", 
                new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9"),
                new Guid("F8679F50-850A-41CF-9C72-430F290290C8")),
            ("PolicyConfig (Win10/11)", 
                new Guid("F8679F50-850A-41CF-9C72-430F290290C8"),
                new Guid("F8679F50-850A-41CF-9C72-430F290290C8")),
            ("MMDeviceEnumerator", 
                new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"),
                new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")),
        };

        foreach (var entry in clsidsToTest)
        {
            var clsid = entry.clsid;
            var iid = entry.iid;
            try
            {
                int hr = CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iid, out IntPtr ptr);
                Console.WriteLine($"  {entry.name}: HRESULT={hr} (0x{hr:X8}), Ptr={ptr}");
                if (ptr != IntPtr.Zero)
                {
                    Marshal.Release(ptr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {entry.name}: ERROR - {ex.GetType().Name}: {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Testing SetDefaultEndpoint ===\n");

        if (activeDevices.Count >= 2)
        {
            foreach (var device in activeDevices)
            {
                if (defaultDevice == null || device.ID != defaultDevice.ID)
                {
                    Console.WriteLine($"Attempting to switch to: {device.FriendlyName}");

                    try
                    {
                        var manager = new AudioEndpointManager();
                        manager.SetDefaultEndpoint(device.ID);
                        Console.WriteLine("  SUCCESS: No exception!");

                        System.Threading.Thread.Sleep(1000);
                        var newDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                        Console.WriteLine($"  New default: {newDefault?.FriendlyName ?? "N/A"}");

                        if (newDefault != null && newDefault.ID == device.ID)
                        {
                            Console.WriteLine("  ✓ VERIFIED: Changed successfully!");
                        }
                        else
                        {
                            Console.WriteLine("  ✗ NOT VERIFIED: Did not change");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");
                    }

                    break;
                }
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        try { Console.ReadKey(); } catch { Console.ReadLine(); }
    }
}