using System;
using System.Runtime.InteropServices;

namespace QuickAudioSwitcher;

/// <summary>
/// COM interface to set default audio endpoint via IPolicyConfig
/// </summary>
[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    void GetMixFormat(string pszDeviceName, IntPtr ppFormat);
    void GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);
    void ResetDeviceFormat(string pszDeviceName);
    void SetDeviceFormat(string pszDeviceName, IntPtr pFormat, IntPtr ppFormatToSet);
    void GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr ppPeriod);
    void SetProcessingPeriod(string pszDeviceName, IntPtr pPeriod);
    void GetShareMode(string pszDeviceName, IntPtr ppMode);
    void SetShareMode(string pszDeviceName, IntPtr pMode);
    void GetPropertyValue(string pszDeviceName, bool bFx, IntPtr pKey, IntPtr pv);
    void SetPropertyValue(string pszDeviceName, bool bFx, IntPtr pKey, IntPtr pv);
    void GetDefaultEndpoint(string pszDeviceName, IntPtr ppEndpoint);
    void SetDefaultEndpoint(string pszDeviceName, IntPtr pEndpointId, IntPtr role);
    void SetEndpointVisibility(string pszDeviceName, bool bVisible);
}

[ComImport]
[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
internal class CPolicyConfig
{
}

/// <summary>
/// Wrapper for IPolicyConfig to set default audio endpoint
/// </summary>
internal class PolicyConfigClient : IDisposable
{
    private readonly IPolicyConfig _policyConfig;

    public PolicyConfigClient()
    {
        _policyConfig = (IPolicyConfig)new CPolicyConfig();
    }

    public void SetDefaultEndpoint(string deviceId, int role)
    {
        _policyConfig.SetDefaultEndpoint(deviceId, IntPtr.Zero, (IntPtr)role);
    }

    public void Dispose()
    {
        if (_policyConfig is MarshalByRefObject marshalByRef)
        {
            Marshal.ReleaseComObject(marshalByRef);
        }
    }
}