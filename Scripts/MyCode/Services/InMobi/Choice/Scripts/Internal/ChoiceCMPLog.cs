﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ChoiceCMPLog
{
    public static class SdkLogEvent
    {
        public const string InitStarted = "Choice initialization started";
        public const string InitFinished = "Choice initialized successfully.";
    }

    public static class AdLogEvent
    {
        public const string LoadSuccess = "Choice loaded";
        public const string LoadFailed = "Choice failed to load, error: {0}";
        public const string ShowSuccess = "Popup shown";
        public const string DidReceiveIABVendorConsent = "CMP did receive IABVendor Consent";
        public const string DidReceiveNONIABVendorConsent = "CMP did receive NONIABVendor Consent";
        public const string DidReceiveAdditionalConsent = "CMP did receive Additional Consent";
        public const string DidReceiveCCPAConsent = "CMP did receive CCPA Consent";
        public const string InvalidJson = "Invalid JSON data: {0}";
        public const string MissingValues = "Missing one or more values: {0} (expected {1})";
    }

    private static readonly Dictionary<string, ChoiceCMP.LogLevel> logLevelMap =
        new Dictionary<string, ChoiceCMP.LogLevel>
    {
        { SdkLogEvent.InitStarted, ChoiceCMP.LogLevel.Debug },
        { SdkLogEvent.InitFinished, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.LoadSuccess, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.LoadFailed, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.ShowSuccess, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.DidReceiveIABVendorConsent, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.DidReceiveNONIABVendorConsent, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.DidReceiveAdditionalConsent, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.DidReceiveCCPAConsent, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.InvalidJson, ChoiceCMP.LogLevel.Debug },
        { AdLogEvent.MissingValues, ChoiceCMP.LogLevel.Debug }

    };

    public static void Log(string callerMethod, string message, params object[] args)
    {
        ChoiceCMP.LogLevel messageLogLevel;
        if (!logLevelMap.TryGetValue(message, out messageLogLevel))
            messageLogLevel = ChoiceCMP.LogLevel.Debug;

        if (ChoiceCMP.ChoiceLogLevel < messageLogLevel) return;

        var formattedMessage = "[Choice-Unity] [" + callerMethod + "] " + message;
        try
        {
            Debug.LogFormat(formattedMessage, args);
        }
        catch (FormatException)
        {
            Debug.Log("Format exception while logging message { " + formattedMessage + " } with arguments { " +
                       string.Join(",", args.Select(a => a.ToString()).ToArray()) + " }");
        }
    }
}
