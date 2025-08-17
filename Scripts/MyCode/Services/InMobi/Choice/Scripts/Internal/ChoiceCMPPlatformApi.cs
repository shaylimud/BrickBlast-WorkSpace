/// <summary>
/// Bridge between the InMobiChoice Unity Instance-wide API and platform-specific implementations.
/// </summary>
/// <para>
/// Publishers integrating with InMobiChoice should make all calls through the <see cref="ChoiceCMP"/> class, and handle any
/// desired ChoiceCMP Events from the <see cref="ChoiceCMPManager"/> class.
/// </para>
/// <para>
/// For platform-specific implementations, see
/// <see cref="ChoiceCMPUnityEditor"/>, <see cref="ChoiceCMPAndroid"/>, and <see cref="ChoiceCMPiOS"/>.
/// </para>
internal abstract class ChoiceCMPPlatformApi
{

    #region SdkSetup

    internal abstract void StartChoice(string pCode, bool shouldDisplayIDFA);

    internal abstract void ShowCCPA(string pCode);

    internal abstract void ForceDisplayUI();

    internal abstract void StartChoiceFromWeb();

    internal abstract string GetTCString();

    #endregion SdkSetup

}
