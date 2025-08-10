using System.Collections.Generic;
using UnityEngine;

public class ServiceAllocator : MonoBehaviour
{
    [System.Serializable]
    public class RayDebugServices
    {
        public ConfigType ConfigType;
        public RayDebugService Service;
    }

    [Header("Debugger")]
    public List<RayDebugServices> DebugServices = new List<RayDebugServices>();

    public RayDebugService GetDebugService(ConfigType configType)
    {
        foreach (var Service in DebugServices)
        {
            if (Service.ConfigType == configType)
            {
                return Service.Service;
            }
        }
        return null;
    }

    [Header("Configs")]
    [SerializeField] private RayDebugConfig _rayDebugConfig;
    public RayDebugConfig RayDebugConfig => _rayDebugConfig;

    public static ServiceAllocator Instance;
    private void Awake()
    {
        Instance = this;

        Debug.unityLogger.logEnabled = _rayDebugConfig.IncludeUnityLogs;
    }
}