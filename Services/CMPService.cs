using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public class CMPService : MonoBehaviour
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static CMPService Instance;
        private void Awake()
        {
            Instance = this;
        }

        private TaskCompletionSource<bool> cmpLoadedTCS;

        public async Task Initialize()
        {
            cmpLoadedTCS = new TaskCompletionSource<bool>();

            // Set log level for debugging
            ChoiceCMP.ChoiceLogLevel = ChoiceCMP.LogLevel.Debug;

            ChoiceCMPManager.CMPDidLoadEvent += OnCMPLoaded;
            ChoiceCMPManager.CMPDidErrorEvent += OnCMPError;

            // Start CMP with your publisher ID
            ChoiceCMP.StartChoice("p-baWWJmW0s00Hb", false);

            // Wait until CMP has loaded
            await cmpLoadedTCS.Task;

            // Remove event listeners to prevent memory leaks
            ChoiceCMPManager.CMPDidLoadEvent -= OnCMPLoaded;
            ChoiceCMPManager.CMPDidErrorEvent -= OnCMPError;
            _rayDebug.Log("Single Code Entry - CMPService Initialized", this);
        }

        private void OnCMPLoaded(PingResult pingResult)
        {
            _rayDebug.Log("CMP Loaded!", this);
            cmpLoadedTCS?.TrySetResult(true);
        }

        private void OnCMPError(string error)
        {
            _rayDebug.LogError($"CMP Failed to Load: {error}", this);
            cmpLoadedTCS?.TrySetResult(false);
        }
    }
}