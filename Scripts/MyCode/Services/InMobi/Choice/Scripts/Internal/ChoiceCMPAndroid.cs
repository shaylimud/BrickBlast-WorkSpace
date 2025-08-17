using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/// <summary>
/// Bridge between the Choice Unity Instance-wide API and Android implementation.
/// </summary>
/// <para>
/// Publishers integrating with Choice should make all calls through the <see cref="ChoiceCMP"/> class, and handle any
/// desired ChoiceCMP Events from the <see cref="ChoiceCMPManager"/> class.
/// </para>
/// <para>
/// For other platform-specific implementations, see <see cref="ChoiceCMPUnityEditor"/> and <see cref="ChoiceCMPiOS"/>.
/// </para>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal class ChoiceCMPAndroid : ChoiceCMPPlatformApi
{
    private static readonly AndroidJavaClass PluginClass = new AndroidJavaClass("com.inmobi.choice.unityplugin.InMobiChoiceCmp");

    #region SdkSetup

    internal override void ForceDisplayUI()
    {
        PluginClass.CallStatic("forceDisplayUI");
    }

    internal override string GetTCString()
    {
        return PluginClass.CallStatic<string>("getTCString");
    }

    internal override void ShowCCPA(string pCode)
    {
        PluginClass.CallStatic("showCCPAScreen");
    }

    internal override void StartChoice(string pCode, bool shouldDisplayIDFA)
    {
        PluginClass.CallStatic("startChoice", new object[1] { pCode });
    }

    internal override void StartChoiceFromWeb()
    {
        
    }

    #endregion SdkSetup
}
