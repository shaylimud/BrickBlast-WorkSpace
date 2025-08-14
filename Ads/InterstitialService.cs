using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ray.Services;

namespace Ray.Controllers
{
    public class InterstitialService : MonoBehaviour
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        private int _count;

        public static InterstitialService Instance;
        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventService.Ad.OnApplovinInitialized += Initialize;
            EventService.Ad.OnTripleDismissed += TryShowInter;
        }

        private void Initialize(Component c)
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

            MaxSdk.LoadInterstitial(Database.GameSettings.Advertising.Interstitial.Regular);
            MaxSdk.LoadInterstitial(Database.GameSettings.Advertising.Interstitial.Penalty);
        }

        private void TryShowInter(Component c)
        {
            _rayDebug.Event("TryShowInter", c, this);

            if (IAPService.Instance.IsSubsribed(Database.GameSettings.InAppPurchases.SubscriptionNoAds)
                || Database.GameSettings.Advertising.Freqs.InterFreq == 0) return;

            _count++;

            int chosenFreq;
            string chosenUnitId;

            if (ResourceService.Instance.PanalizedUser())
            {
                chosenFreq = 1;
                chosenUnitId = Database.GameSettings.Advertising.Interstitial.Penalty;
            }
            else
            {
                chosenFreq = Database.GameSettings.Advertising.Freqs.InterFreq;
                chosenUnitId = Database.GameSettings.Advertising.Interstitial.Regular;
            } 

            if(_count >= chosenFreq && MaxSdk.IsInterstitialReady(chosenUnitId))
            {
                MaxSdk.ShowInterstitial(chosenUnitId);
                _count = 0;
            }
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) 
        {
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) 
        {
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        {
            MaxSdk.LoadInterstitial(adUnitId);
        }
    }
}