using System.Runtime.InteropServices;

/// <summary>
/// Bridge between the Choice Unity Instance-wide API and iOS implementation.
/// </summary>
/// <para>
/// Publishers integrating with Choice should make all calls through the <see cref="ChoiceCMP"/> class, and handle any
/// desired ChoiceCMP Events from the <see cref="ChoiceCMPManager"/> class.
/// </para>
/// <para>
/// For other platform-specific implementations, see <see cref="ChoiceCMPUnityEditor"/> and <see cref="ChoiceCMPAndroid"/>.
/// </para>

internal class ChoiceCMPiOS : ChoiceCMPPlatformApi
{
    #region SdkSetup

    internal override void StartChoice(string pCode, bool shouldDisplayIDFA)
    {
        _StartChoice(pCode, shouldDisplayIDFA);
    }

    internal override void ShowCCPA(string pCode)
    {
        _ShowCCPA(pCode);
    }

    internal override void ForceDisplayUI()
    {
        _ForceDisplayUI();
    }

    internal override void StartChoiceFromWeb()
    {
        _StartChoiceFromWeb();
    }

    internal override string GetTCString()
    {
        return _GetTCString();
    }


    #endregion SdkSetup

    #region DllImports
#if ENABLE_IL2CPP && UNITY_ANDROID
    // IL2CPP on Android scrubs DllImports, so we need to provide stubs to unblock compilation
    private static void _StartChoice(string pCode, bool shouldDisplayIDFA) {}
    private static void _ShowCCPA(string pCode) {}
    private static string _GetTCString() { return ""; }
    private static void _ForceDisplayUI() {}
    private static void _StartChoiceFromWeb() {}
#else
    [DllImport("__Internal")]
    private static extern void _StartChoice(string pCode, bool shouldDisplayIDFA);

    [DllImport("__Internal")]
    private static extern void _ShowCCPA(string pCode);

    [DllImport("__Internal")]
    private static extern void _ForceDisplayUI();

    [DllImport("__Internal")]
    private static extern void _StartChoiceFromWeb();

    [DllImport("__Internal")]
    private static extern string _GetTCString();


#endif
    #endregion DllImports
}
