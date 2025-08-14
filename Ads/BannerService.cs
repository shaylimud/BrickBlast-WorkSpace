using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ray.Services;

namespace Ray.Controllers
{
    public class BannerService : MonoBehaviour
    {
        [Header("Banner Settings")]
        [SerializeField] private MaxSdkBase.BannerPosition _bannerPosition;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static BannerService Instance;
        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventService.Ad.OnApplovinInitialized += Initialize;
        }

        private void Initialize(Component c)
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;

            MaxSdk.SetBannerBackgroundColor(Database.GameSettings.Advertising.Banner, Color.clear);
            MaxSdk.SetBannerExtraParameter(Database.GameSettings.Advertising.Banner, "adaptive_banner", "true");
            MaxSdk.CreateBanner(Database.GameSettings.Advertising.Banner, _bannerPosition);
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MaxSdk.ShowBanner(Database.GameSettings.Advertising.Banner);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) { }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    }
}