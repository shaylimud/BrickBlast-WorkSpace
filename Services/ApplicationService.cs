using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ray.Services;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Ray.Services
{
    public class ApplicationService : MonoBehaviour
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            EventService.UI.OnUpdateApplicationBtn += RedirectToAppstorePage;
        }

        void Start()
        {
            SingleCodeEntry();
        }

        private async void SingleCodeEntry()
        {
            _rayDebug.Log("Single Code Entry - Initializing Application", this);

            await ConnectionService.Instance.WaitForConnection();
            _rayDebug.Log("Single Code Entry - Connection Established", this);

            TenjinService.Instance.Initialize();
            _rayDebug.Log("Single Code Entry - TenjinService Initialized", this);

            await Database.Instance.Initialize();
            _rayDebug.Log("Single Code Entry - Database Initialized", this);

#if UNITY_ANDROID
            //await CMPService.Instance.Initialize();
            _rayDebug.Log("Single Code Entry - CMPService Initialized", this);
#endif

            await ApplovinService.Instance.Initialize();
            _rayDebug.Log("Single Code Entry - ApplovinService Initialized", this);

            await IAPService.Instance.Initialize();
            _rayDebug.Log("Single Code Entry - IAPService Initialized", this);

#if UNITY_IOS && !UNITY_EDITOR
            await BrdService.Instance.InitializeBrdSdk();
            _rayDebug.Log("Single Code Entry - BrightData Initialized", this);
#endif

            //EventService.Application.OnGameContentStart.Invoke(this);
            SceneManager.LoadSceneAsync("Game");
        }

        private void RedirectToAppstorePage(Component c)
        {
            _rayDebug.Event("RedirectToAppstorePage", c, this);

            string url = $"https://play.google.com/store/apps/details?id={Application.identifier}";
            Application.OpenURL(url);
        }
    }
}

