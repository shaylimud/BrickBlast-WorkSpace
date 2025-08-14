using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static GameSettingsRay;

namespace Ray.Services
{
    public class RewardedService : MonoBehaviour
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        private Dictionary<string, string> _adUnits;
        private RewardedType _currentRewarededType;

        public static RewardedService Instance;

        private void Awake()
        {
            Instance = this; // No DontDestroyOnLoad — destroyed on scene change
        }

        private void OnEnable()
        {
            EventService.Ad.OnApplovinInitialized += Initialize;
            EventService.UI.OnRewardedBtn += PlayRewardedAd;
        }

        private void OnDisable()
        {
            EventService.Ad.OnApplovinInitialized -= Initialize;
            EventService.UI.OnRewardedBtn -= PlayRewardedAd;

            // Unregister ad event callbacks to avoid leaks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
        }

        private void Initialize(Component c)
        {
            _rayDebug.Event("Initialize", c, this);

            // Initialize the dictionary from GameSettings
            _adUnits = ConvertToDictionary(Database.GameSettings.Advertising.Rewarded);

            // Register ad event callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            LoadAllAds();
        }

        public static Dictionary<string, string> ConvertToDictionary(RewardedAdUnits rewardedAdUnits)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (PropertyInfo property in typeof(RewardedAdUnits).GetProperties())
            {
                object value = property.GetValue(rewardedAdUnits);
                if (value is string adUnitId && !string.IsNullOrEmpty(adUnitId))
                {
                    dictionary[adUnitId] = property.Name;
                }
            }

            return dictionary;
        }

        private void LoadAllAds()
        {
            foreach (var adUnit in _adUnits.Keys)
            {
                _rayDebug.Log($"Loading Ad Unit: {adUnit}", this);
                MaxSdk.LoadRewardedAd(adUnit);
            }
        }

        private void LoadAd(string adUnitId)
        {
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void ShowRewardedAd(string adUnit)
        {
            MaxSdk.ShowRewardedAd(adUnit);
        }

        public bool IsRewardedReady(RewardedType type)
        {
            switch (type)
            {
                case RewardedType.Penalty:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.Penalty);
                case RewardedType.NoEnemies:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.NoEnemies);
                case RewardedType.Revive:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.Revive);
                case RewardedType.Triple:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.Triple);
                case RewardedType.FreeGift:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.FreeGift);
                case RewardedType.ExtraSpace:
                    return MaxSdk.IsRewardedAdReady(Database.GameSettings.Advertising.Rewarded.ExtraSpace);
                default:
                    _rayDebug.LogWarning($"Unknown reward type: {_currentRewarededType}", this);
                    return false;
            }
        }

        private void PlayRewardedAd(Component c, RewardedType type)
        {
            _rayDebug.Event("PlayRewardedAd", c, this);
            EventService.Ad.OnWatchAdBtn.Invoke(this);

            _currentRewarededType = type;

            switch (type)
            {
                case RewardedType.Penalty:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.Penalty);
                    break;
                case RewardedType.NoEnemies:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.NoEnemies);
                    break;
                case RewardedType.Revive:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.Revive);
                    break;
                case RewardedType.Triple:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.Triple);
                    break;
                case RewardedType.FreeGift:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.FreeGift);
                    break;
                case RewardedType.ExtraSpace:
                    ShowRewardedAd(Database.GameSettings.Advertising.Rewarded.ExtraSpace);
                    break;
            }
        }

        private void GiveReward()
        {
            ResourceService.Instance.IncreaseRvCount.Value = true;

            switch (_currentRewarededType)
            {
                case RewardedType.Penalty:
                    EventService.Ad.OnPenaltyWatched?.Invoke(this);
                    break;
                case RewardedType.NoEnemies:
                    EventService.Ad.OnNoEnemiesWatched?.Invoke(this);
                    break;
                case RewardedType.Revive:
                    EventService.Ad.OnReviveWatched?.Invoke(this);
                    break;
                case RewardedType.Triple:
                    EventService.Ad.OnTripleWatched?.Invoke(this);
                    break;
                case RewardedType.FreeGift:
                    EventService.Ad.OnFreeGiftWatched?.Invoke(this);
                    break;
                case RewardedType.ExtraSpace:
                    EventService.Ad.OnExtraSpaceWatched?.Invoke(this);
                    break;
                default:
                    _rayDebug.LogWarning($"Unknown reward type: {_currentRewarededType}", this);
                    break;
            }
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rayDebug.Log($"Rewarded ad loaded: {adUnitId}", this);
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rayDebug.LogWarning($"Failed to load ad: {adUnitId}. Error: {errorInfo.Message}", this);
            LoadAd(adUnitId);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rayDebug.Log($"Rewarded ad hidden: {adUnitId}", this);
            LoadAd(adUnitId);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _rayDebug.LogWarning($"Ad failed to display: {adUnitId}. Reloading...", this);
            LoadAd(adUnitId);
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            GiveReward();
        }
    }
}
