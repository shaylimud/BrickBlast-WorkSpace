using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public class ApplovinService : MonoBehaviour
    {
        [Header("Applovin Settings")]
        [SerializeField] private bool _showMediationDebugger;
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static ApplovinService Instance;
        private void Awake()
        {
            Instance = this;
        }

        public async Task Initialize()
        {
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();

            // Await SDK initialization
            await WaitForApplovinInitialization();

            if (_showMediationDebugger) MaxSdk.ShowMediationDebugger();

            _rayDebug.Log($"Applovin Initialized.", this);
            EventService.Ad.OnApplovinInitialized.Invoke(this);
        }

        private Task WaitForApplovinInitialization()
        {
            var tcs = new TaskCompletionSource<bool>();

            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                tcs.SetResult(true);
            };

            return tcs.Task;
        }
    }
}