using UnityEngine;
using Ray.Services;

[CreateAssetMenu(fileName = "RayDebugService", menuName = "Services/RayDebugService")]
public class RayDebugService : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private bool _showThisLog;
    [SerializeField] private string _prefix;
    [SerializeField] private Color _prefixColor;

    private RayDebugConfig _rayDebugConfig { get { return ServiceAllocator.Instance.RayDebugConfig; } }

    public void Event(object message, Component Sender, Object receiver)
    {
        if (!_showThisLog || !_rayDebugConfig.IncludeEventLogs) return;

        // Guard against destroyed objects which evaluate to null in Unity
        var senderName = Sender != null ? Sender.name : "null";
        var receiverName = receiver != null ? receiver.name : "null";
        var context = receiver != null ? receiver : null;

        Debug.Log($"{GetColorHex(Color.cyan)}EVENT: {senderName} >> {receiverName} :: {message}</color>", context);
    }

    public void Log(object message, Object sender)
    {
        if (!_showThisLog || !_rayDebugConfig.IncludeNormalLogs) return;
        Debug.Log($"<color=white>{_prefix}: {message}</color>", sender);
    }

    public void LogWarning(object message, Object sender)
    {
        if (!_showThisLog || !_rayDebugConfig.IncludeWarningLogs) return;
        Debug.LogWarning($"WARNING: {GetColorHex(_prefixColor)}{_prefix}: {message}</color>", sender);
    }

    public void LogError(object message, Object sender)
    {
        if (!_showThisLog || !_rayDebugConfig.IncludeErrorLogs) return;
        Debug.LogError($"ERROR:{GetColorHex(_prefixColor)}{_prefix}: {message}</color>", sender);
    }

    private string GetColorHex(Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
    }
}