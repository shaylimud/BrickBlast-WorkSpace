
using UnityEngine;

public abstract class ChoiceCMP
{

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Debug = 2,
    }
    private static string code;

    public static LogLevel ChoiceLogLevel;

    /// <summary>
    /// InMobi Choice Unity plugin version
    /// </summary>
    public const string ChoicePluginVersion = "1.0.0";


    static ChoiceCMP()
    {
        if (ChoiceCMPManager.Instance == null)
            new GameObject("ChoiceCMPManager", typeof(ChoiceCMPManager));
    }


    /// <summary>
    /// Start Choice with pCode and optional shouldDisplayIDFA
    /// </summary>
    /// <param name="pCode">PCode for initialising Choice.
    /// <param name="shouldDisplayIDFA">Optional param to show IDFA pop up on iOS.
    public static void StartChoice(string pCode, bool shouldDisplayIDFA = false)
    {
        ChoiceCMPLog.Log("StartChoice", ChoiceCMPLog.SdkLogEvent.InitStarted);
        ChoiceCMPManager.ChoiceCMPPlatformApi.StartChoice(pCode, shouldDisplayIDFA);
        code = pCode;
    }


    /// <summary>
    /// Show CCPA popup
    /// </summary>
    public static void ShowCCPA()
    {
        ChoiceCMPManager.ChoiceCMPPlatformApi.ShowCCPA(code);
    }

    /// <summary>
    /// Force show Choice popup
    /// </summary>
    public static void ForceDisplayUI()
    {
        ChoiceCMPManager.ChoiceCMPPlatformApi.ForceDisplayUI();
    }

    /// <summary>
    /// Show Choice in webview
    /// </summary>
    public static void StartChoiceFromWeb()
    {
        ChoiceCMPManager.ChoiceCMPPlatformApi.StartChoiceFromWeb();
    }

    /// <summary>
    /// Get the TC String
    /// </summary>
    public static string GetTCString()
    {
        return ChoiceCMPManager.ChoiceCMPPlatformApi.GetTCString();
    }

}
