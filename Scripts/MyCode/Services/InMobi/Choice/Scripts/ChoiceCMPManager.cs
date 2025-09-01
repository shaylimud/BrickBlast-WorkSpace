using System;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Handler for Choice integration across publisher apps and Unity Editor.
/// </summary>
/// <para>
/// Publishers integrating with InMobiChoice should make all calls through the <see cref="ChoiceCMP"/> class, and handle any
/// desired ChoiceCMP Events from this class.
/// </para>

public class ChoiceCMPManager : MonoBehaviour
{
    #region ChoiceCMPEvents


    // Fired when the SDK has finished loading
    public static event Action<PingResult> CMPDidLoadEvent;

    // Fired when popup is shown.
    public static event Action<PingResult> CMPDidShowEvent;

    // Fired when SDK fails to load
    public static event Action<string> CMPDidErrorEvent;

    // Fired when on receiving the IAB Vendor Consent
    public static event Action<TCData> CMPDidReceiveIABVendorConsentEvent;

    // Fired when on receiving the NON IAB Vendor Consent
    public static event Action<NonIABData> CMPDidReceiveNonIABVendorConsentEvent;

    // Fired when on receiving the Additional Consent
    public static event Action<ACData> CMPDidReceiveAdditionalConsentEvent;

    // Fired when on receiving the Additional Consent
    public static event Action<string> CMPDidReceiveCCPAConsentEvent;

    #endregion ChoiceCMPEvents

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        CMPDidLoadEvent = null;
        CMPDidShowEvent = null;
        CMPDidErrorEvent = null;
        CMPDidReceiveIABVendorConsentEvent = null;
        CMPDidReceiveNonIABVendorConsentEvent = null;
        CMPDidReceiveAdditionalConsentEvent = null;
        CMPDidReceiveCCPAConsentEvent = null;
    }


    // Singleton.
    public static ChoiceCMPManager Instance { get; protected set; }

    #region ChoiceCMPManagerPrefab

   
  
    // API to make calls to the platform-specific Choice SDK.
    internal static ChoiceCMPPlatformApi ChoiceCMPPlatformApi { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (ChoiceCMPPlatformApi == null)
            ChoiceCMPPlatformApi = new
#if UNITY_EDITOR
                ChoiceCMPUnityEditor
#elif UNITY_ANDROID
                ChoiceCMPAndroid
#else
                ChoiceCMPiOS
#endif
                ();

        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);

    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    #endregion ChoiceCMPManagerPrefab


    #region PlatformCallbacks

    public void EmitCMPDidLoadEvent(string pingReturnJson)
    {
        PingResult pingResult;
        try
        {
            pingResult = JsonUtility.FromJson<PingResult>(pingReturnJson);
        } catch (Exception)
        {
            pingResult = new PingResult();
            ChoiceCMPLog.Log("EmitCMPDidLoadEvent", "Error decoding ping result");
        }

        ChoiceCMPLog.Log("EmitCMPDidLoadEvent", ChoiceCMPLog.SdkLogEvent.InitFinished);
        ChoiceCMPLog.Log("EmitCMPDidLoadEvent", "LoadEvent: " + pingReturnJson);
        var evt = CMPDidLoadEvent;
        if (evt != null) evt(pingResult);
    }


    public void EmitCMPDidShowEvent(string pingReturnJson)
    {
        PingResult pingResult;
        try
        {
            pingResult = JsonUtility.FromJson<PingResult>(pingReturnJson);
        }
        catch (Exception)
        {
            pingResult = new PingResult();
            ChoiceCMPLog.Log("EmitCMPDidShowEvent", "Error decoding ping result");
        }
        
        ChoiceCMPLog.Log("EmitCMPDidShowEvent", ChoiceCMPLog.AdLogEvent.ShowSuccess);
        ChoiceCMPLog.Log("EmitCMPDidShowEvent", "ShowEvent: " + pingReturnJson);
        var evt = CMPDidShowEvent;
        if (evt != null) evt(pingResult);
    }


    public void EmitCMPDidErrorEvent(string error)
    {
        ChoiceCMPLog.Log("EmitCMPDidErrorEvent", ChoiceCMPLog.AdLogEvent.LoadFailed, error);
        var evt = CMPDidErrorEvent;
        if (evt != null) evt(error);
    }


    public void EmitCMPDidReceiveIABVendorConsentEvent(string tcDataJson)
    {
        TCData tcData;
        try
        {
            tcData = JsonUtility.FromJson<TCData>(tcDataJson);
        } catch (Exception)
        {
            tcData = new TCData();
            ChoiceCMPLog.Log("EmitCMPDidReceiveIABVendorConsentEvent", "Error decoding TCData");

        }

        ChoiceCMPLog.Log("EmitCMPDidReceiveIABVendorConsentEvent", ChoiceCMPLog.AdLogEvent.DidReceiveIABVendorConsent);
        var evt = CMPDidReceiveIABVendorConsentEvent;
        if (evt != null) evt(tcData);
    }

    public void EmitCMPDidReceiveNonIABVendorConsentEvent(string nonIABDataJson)
    {
        NonIABData nonIABData;
        try
        {
            nonIABData = JsonConvert.DeserializeObject<NonIABData>(nonIABDataJson);
        } catch (Exception)
        {
            nonIABData = new NonIABData();
            ChoiceCMPLog.Log("EmitCMPDidReceiveNonIABVendorConsentEvent", "Error decoding NonIABData");
        }
        
        ChoiceCMPLog.Log("EmitCMPDidReceiveNonIABVendorConsentEvent", ChoiceCMPLog.AdLogEvent.DidReceiveNONIABVendorConsent);
        ChoiceCMPLog.Log("EmitCMPDidReceiveNonIABVendorConsentEvent", "NonIABVendorConsent: " + nonIABDataJson);
        var evt = CMPDidReceiveNonIABVendorConsentEvent;
        if (evt != null) evt(nonIABData);
    }

    public void EmitCMPDidReceiveAdditionalConsentEvent(string acDataJson)
    {
        ACData acData;
        try
        {
            acData = JsonUtility.FromJson<ACData>(acDataJson);
        } catch (Exception)
        {
            acData = new ACData();
            ChoiceCMPLog.Log("EmitCMPDidReceiveAdditionalConsentEvent", "Error decoding ACData");
        }

        ChoiceCMPLog.Log("EmitCMPDidReceiveAdditionalConsentEvent", ChoiceCMPLog.AdLogEvent.DidReceiveAdditionalConsent);
        var evt = CMPDidReceiveAdditionalConsentEvent;
        if (evt != null) evt(acData);
    }

    public void EmitCMPDidReceiveCCPAConsentEvent(string ccpaConsent)
    {
        ChoiceCMPLog.Log("EmitCMPDidReceiveCCPAConsentEvent", ChoiceCMPLog.AdLogEvent.DidReceiveCCPAConsent);
        var evt = CMPDidReceiveCCPAConsentEvent;
        if (evt != null) evt(ccpaConsent);
    }

    #endregion PlatformCallbacks
}
